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
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
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
        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
        {
            return;
        }

        //把DtoAttribute加入当前的编译中
        Compilation compilation = context.Compilation;

        CreateService(context, compilation, receiver.CandidateClasses);
    }

    //创建注入代码
    public void CreateService(GeneratorExecutionContext context, Compilation compilation, List<ClassDeclarationSyntax> candidateClasses)
    {
        //获得ServiceAttribute类符号
        INamedTypeSymbol serviceAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.ServiceAttribute");

        //获得DtoAttribute类符号
        INamedTypeSymbol autoInjectAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.AutoInjectAttribute");

        List<string> addServiceCode = new List<string>();

        //创建Service扩展
        foreach (ClassDeclarationSyntax serviceClass in candidateClasses)
        {
            //获得Service类的类型符号
            if (compilation.GetSemanticModel(serviceClass.SyntaxTree).GetDeclaredSymbol(serviceClass) is not ITypeSymbol serviceSymbol) return;

            //寻找是否有ServiceAttribute
            var serverAttr = serviceSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(serviceAttrSymbol, SymbolEqualityComparer.Default));
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

            addServiceCode.Add($"builder.Services.Add{attLifeCycleText}<{serviceSymbol.ContainingNamespace}.{serviceSymbol.Name}>();");

            CreateAutoInject(context, serviceSymbol, autoInjectAttrSymbol);
        }

        //如果没有使用就停止生成
        if (addServiceCode.Count is 0) return;

        var code = "";
        if (addServiceCode.Count > 0)
            code = addServiceCode.Aggregate((a, b) => a + "\r\n" + b);

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

    public void CreateAutoInject(GeneratorExecutionContext context, ITypeSymbol classSymbol, INamedTypeSymbol autoInjectAttrSymbol)
    {
        //寻找AutoInject
        List<IPropertySymbol> autoInjectProps =
             classSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property)
                      .Where(x => x.Kind == SymbolKind.Property)//只保留属性
                      .Where(x => x.GetAttributes().Any(y => y.AttributeClass.Equals(autoInjectAttrSymbol, SymbolEqualityComparer.Default)))
                      .Cast<IPropertySymbol>()
                      .ToList();

        //入参
        List<string> inputPars = new List<string>();
        //赋值
        List<string> assigns = new List<string>();

        foreach (var prop in autoInjectProps)
        {
            inputPars.Add($"{prop.Type.ContainingNamespace.ToDisplayString()}.{prop.Type.Name} {prop.Name.ToLower()}");
            assigns.Add($"        {prop.Name} = {prop.Name.ToLower()};");
        }

        if (inputPars.Count == 0) return;//如果没有注入就不用创建代码
 
        //类的类型
        var classTypeName = classSymbol.IsRecord ? "record" : "class";

        //组装代码
        string autoInjectCode = @$"namespace {classSymbol.ContainingNamespace.ToDisplayString()};

public partial {classTypeName} {classSymbol.Name}
{{
    public {classSymbol.Name}({inputPars.Aggregate((a, b) => $"{a}, {b}")})
    {{
{assigns.Aggregate((a, b) => $"{a}\r\n{b}")}
    }}
}}
";

        context.AddSource($@"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}.inject.g.cs", SourceText.From(autoInjectCode, Encoding.UTF8));
    }
}


