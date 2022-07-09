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

        StringBuilder addServiceCode = new StringBuilder();

        //创建Service扩展
        foreach (ClassDeclarationSyntax serviceClass in candidateClasses)
        {
            //获得Service类的类型符号
            if (compilation.GetSemanticModel(serviceClass.SyntaxTree).GetDeclaredSymbol(serviceClass) is not ITypeSymbol serviceSymbol) return;

            CreateAddServices(serviceSymbol, serviceAttrSymbol, addServiceCode);

            CreateAutoInject(context, serviceSymbol, autoInjectAttrSymbol);
        }

        //如果没有使用就停止生成
        var code = addServiceCode.ToString();
        if (string.IsNullOrWhiteSpace(code) == false)
        {
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

    /// <summary>
    /// 生成添加注入代码
    /// </summary>
    public void CreateAddServices(ITypeSymbol serviceSymbol, INamedTypeSymbol serviceAttrSymbol, StringBuilder addServiceCode)
    {
        //寻找是否有ServiceAttribute
        var serverAttr = serviceSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(serviceAttrSymbol, SymbolEqualityComparer.Default));
        if (serverAttr == null) return;

        //获得LifeCycle的名字
        var attLifeCycle = serverAttr.NamedArguments.FirstOrDefault(x => x.Key == "LifeCycle").Value.Value;

        var attLifeCycleText = attLifeCycle switch
        {
            0 => "Singleton",
            1 => "Scoped",
            2 => "Transient",
            _ => "Scoped",
        };

        addServiceCode.AppendLine($"builder.Services.Add{attLifeCycleText}<{serviceSymbol.ContainingNamespace}.{serviceSymbol.Name}>();");
    }

    /// <summary>
    /// 生成自动注入代码
    /// </summary>
    public void CreateAutoInject(GeneratorExecutionContext context, ITypeSymbol serviceSymbol, INamedTypeSymbol autoInjectAttrSymbol)
    {
        //寻找是否有ServiceAttribute
        List<AttributeData> injectAttrs = serviceSymbol.GetAttributes().Where(x => x.AttributeClass.Equals(autoInjectAttrSymbol, SymbolEqualityComparer.Default)).ToList();

        //定义
        StringBuilder definitions= new StringBuilder();
        //入参
        List<string> inputPars = new List<string>();
        //赋值
        StringBuilder assigns = new StringBuilder();

        foreach (var injectAttr in injectAttrs)
        {
            //构造参数
            var constArgs = injectAttr.ConstructorArguments.ToList();

            var typeSymbol = constArgs.FirstOrDefault().Value as ITypeSymbol;
            var rename = typeSymbol.Name;
            if (constArgs.Count >= 2)
                rename = constArgs[1].Value as string;

            definitions.AppendLine($"    private readonly {typeSymbol.Name} {rename};");
            inputPars.Add($"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name} inject{rename}");
            assigns.AppendLine($"        {rename} = inject{rename};");
        }

        if (inputPars.Count == 0) return;//如果没有注入就不用创建代码

        //组装代码
        string autoInjectCode = @$"namespace {serviceSymbol.ContainingNamespace.ToDisplayString()};

public partial class {serviceSymbol.Name}
{{
{definitions}

    public {serviceSymbol.Name}({inputPars.Aggregate((a, b) => $"{a}, {b}")})
    {{
{assigns}
    }}
}}
        ";

        context.AddSource($@"{serviceSymbol.ContainingNamespace.ToDisplayString()}.{serviceSymbol.Name}.inject.g.cs", SourceText.From(autoInjectCode, Encoding.UTF8));
    }
}


