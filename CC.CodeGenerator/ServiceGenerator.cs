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
        public List<(ClassDeclarationSyntax classSyntax, AttributeSyntax attrSyntax)> CandidateClasses { get; } = new List<(ClassDeclarationSyntax classSyntax, AttributeSyntax attrSyntax)>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds
                && cds.AttributeLists.Count > 0)
            {
                var attrSyntaxs = cds.AttributeLists.SelectMany(x => x.Attributes.Where(y => y.Name.ToString() == "Service")).FirstOrDefault();
                if (attrSyntaxs == null) return;
                CandidateClasses.Add(new(cds, attrSyntaxs));
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
        {
            return;
        }

        if (receiver.CandidateClasses.Count == 0) return;

        //得到添加代码
        StringBuilder addCode = new StringBuilder();
        foreach (var item in receiver.CandidateClasses)
        {
            addCode.AppendLine(GetAddServiceCode(item.classSyntax, item.attrSyntax));
        }

        //组装代码
        string autoDICode = @$"using Microsoft.AspNetCore.Builder; 
namespace CC.CodeGenerator;
public static class AutoDI
{{
    public static void AddServices(WebApplicationBuilder builder)
    {{
{addCode}
    }}
}}
        ";

        context.AddSource($@"Program.InjectService.g.cs", SourceText.From(autoDICode, Encoding.UTF8));
    }

    /// <summary>
    /// 生成添加注入代码
    /// </summary>
    public string GetAddServiceCode(ClassDeclarationSyntax classSyntax, AttributeSyntax attrSyntax)
    {
        var containingNamespace = classSyntax.GetNamespace();
        var className = classSyntax.Identifier.Text;

        //获得LifeCycle的名字
        var attLifeCycleText = "Scoped";
        var attArgumentList = attrSyntax.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.AttributeArgumentList)) as AttributeArgumentListSyntax;

        if (attArgumentList != null)
        {
            var lifeCycle = attArgumentList.Arguments[0].ChildNodes().First(x => x.IsKind(SyntaxKind.SimpleMemberAccessExpression)).ToFullString();

            attLifeCycleText = (lifeCycle) switch
            {
                "ELifeCycle.Transient" => "Transient",
                "ELifeCycle.Singleton" => "Singleton",
                _ => "Scoped"
            };
        }

        var code = $"        builder.Services.Add{attLifeCycleText}<{containingNamespace}.{className}>();";

        return code;

    }

}


