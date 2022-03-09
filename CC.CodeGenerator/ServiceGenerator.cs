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
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {

            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
            {
                CandidateClasses.Add((ClassDeclarationSyntax)syntaxNode);
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        //生成ServiceAttribute
        SyntaxTree serviceAtt = CreateServiceAttribute(context);

        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
        {
            return;
        }

        //把DtoAttribute加入当前的编译中
        Compilation compilation = context.Compilation.AddSyntaxTrees(serviceAtt);

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

//标记类是否时Service类
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


    public void CreateService(GeneratorExecutionContext context, Compilation compilation, List<ClassDeclarationSyntax> candidateClasses)
    {
        //获得ServiceAttribute类符号
        INamedTypeSymbol attSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.ServiceAttribute");

        List<string> addCode = new List<string>();

        //创建Service扩展
        foreach (ClassDeclarationSyntax serviceClass in candidateClasses)
        {
            //获得Service类的类型符号
            if (compilation.GetSemanticModel(serviceClass.SyntaxTree).GetDeclaredSymbol(serviceClass) is not ITypeSymbol serviceSymbol) return;

            //寻找是否有ServiceAttribute
            var serverAttr = serviceSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(attSymbol, SymbolEqualityComparer.Default));
            if (serverAttr == null) continue;

            //获得LifeCycle的名字
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

        //如果没有使用就停止生成
        if (addCode.Count is 0) return;

        var code = "";
        if (addCode.Count > 0)
            code = addCode.Aggregate((a, b) => a + "\r\n" + b);

        //组装代码
        string autoDICode = @$"using Microsoft.AspNetCore.Builder; 

namespace CC.CodeGenerator;
public static class AutoDI
{{
    public static void AddServices(WebApplicationBuilder builder)
    {{
{code}
    }}
}}
";
        context.AddSource($@"AutoDI.cs", SourceText.From(autoDICode, Encoding.UTF8));

    }


}


