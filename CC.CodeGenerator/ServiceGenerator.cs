using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CC.CodeGenerator;

[Generator]
public class ServiceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG

        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }

#endif

        //注册一个语法修改通知
        context.RegisterForSyntaxNotifications(() => new DtoSyntaxReceiver());
    }

    class DtoSyntaxReceiver : ISyntaxReceiver
    {
        //需要生成Dto操作代码的类
        public List<TypeDeclarationSyntax> CandidateClasses { get; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {

            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
            {
                CandidateClasses.Add((TypeDeclarationSyntax)syntaxNode);
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        //生成DtoAttribute
        SyntaxTree dtoAtt = CreateServiceAttribute(context);

        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
        {
            return;
        }

        //把DtoAttribute加入当前的编译中
        Compilation compilation = context.Compilation.AddSyntaxTrees(dtoAtt);

        CreateService(context, compilation, receiver.CandidateClasses);
    }

    /// <summary>
    /// 创建DtoAttribute代码
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public SyntaxTree CreateServiceAttribute(GeneratorExecutionContext context)
    {
        string attTemplate = @"
namespace CC.CodeGenerator;

//标记类是否时Dto类
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class ServiceAttribute: Attribute
{
    public ELifeCycle LifeCycle { get; set; } = ELifeCycle.Scoped;
}

/// <summary>
/// DI生命周期
/// </summary>
public enum ELifeCycle
{
    Singleton = 0,
    Scoped = 1,
    Transient = 2,
}
";
        SourceText sourceText = SourceText.From(attTemplate, Encoding.UTF8);
        context.AddSource("ServiceAttribute.cs", sourceText);

        return CSharpSyntaxTree.ParseText(SourceText.From(attTemplate, Encoding.UTF8));
    }


    public void CreateService(GeneratorExecutionContext context, Compilation compilation, List<TypeDeclarationSyntax> candidateClasses)
    {
        //获得DtoAttribute类符号
        INamedTypeSymbol attSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.ServiceAttribute");

        List<string> addCode = new List<string>();

        //创建Dto扩展
        foreach (TypeDeclarationSyntax serviceClass in candidateClasses)
        {
            //获得dto类的类型符号
            if (compilation.GetSemanticModel(serviceClass.SyntaxTree).GetDeclaredSymbol(serviceClass) is not ITypeSymbol serviceSymbol) return;

            //寻找是否有DtoAttribute
            var serverAttr = serviceSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(attSymbol, SymbolEqualityComparer.Default));
            if (serverAttr == null) continue;

            //获得DBContext的名字
            var attLifeCycle = serverAttr.NamedArguments.FirstOrDefault(x => x.Key == "LifeCycle").Value.Value;

            var attLifeCycleText = attLifeCycle switch
            {
                0 => "Singleton",
                1 => "Scoped",
                2 => "Transient",
                _ => "Scoped",
            };

            addCode.Add($"builder.Services.Add{attLifeCycleText}<{serviceSymbol.ContainingNamespace}.{serviceSymbol.Name}>();");

        }


        //组装代码
        string autoDICode = @$"using Microsoft.AspNetCore.Builder; 

namespace CC.CodeGenerator;
public static class AutoDI
{{
    public static void AddServices(WebApplicationBuilder builder)
    {{
{addCode.Aggregate((a, b) => a + "\r\n" + b)}
    }}
}}
";
        context.AddSource($@"AutoDI.cs", SourceText.From(autoDICode, Encoding.UTF8));

    }


}


