using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace CC.CodeGenerator;

[Generator]
public class DtoGenerator : ISourceGenerator
{

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        context.RegisterForSyntaxNotifications(() => new DtoSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        //生成DtoAttribute
        SyntaxTree dtoAtt = CreateDtoAttribute(context);

        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
        {
            return;
        }

        //执行当前的编译，并把DtoAttribute加入进去
        Compilation compilation = context.Compilation.AddSyntaxTrees(dtoAtt);

        //获得DtoAttribute类
        INamedTypeSymbol dtoAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoAttribute");


        foreach (ClassDeclarationSyntax dtoClass in receiver.CandidateClasses)
        {
            CreateDto(context, compilation, dtoAttSymbol, dtoClass);
        }


    }

    class DtoSyntaxReceiver : ISyntaxReceiver
    {
        //需要生成Dto操作代码的类
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)//有特性的类
            {
                CandidateClasses.Add(cds);
            }
        }
    }



    public SyntaxTree CreateDtoAttribute(GeneratorExecutionContext context)
    {
        string attTemplate = @"
namespace CC.CodeGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DtoAttribute: Attribute
{
    public string DBContext { get; set; }
}";
        SourceText sourceText = SourceText.From(attTemplate, Encoding.UTF8);
        context.AddSource("DtoAttribute.cs", sourceText);

        return CSharpSyntaxTree.ParseText(SourceText.From(attTemplate, Encoding.UTF8));
    }

    public void CreateDto(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol dtoAttSymbol, ClassDeclarationSyntax dtoClass)
    {


        //获得dto类的类型符号
        if (compilation.GetSemanticModel(dtoClass.SyntaxTree).GetDeclaredSymbol(dtoClass) is not ITypeSymbol dtoClassTypeSymbol) return;


        //寻找是否有DtoAttribute
        var dtoAttr = dtoClassTypeSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(dtoAttSymbol, SymbolEqualityComparer.Default));
        if (dtoAttr == null) return;

        string dtoTemplate = @"
namespace $Namespace$;

public partial class $ClassName$
{
    ///// <summary>
    ///// 主键检索
    ///// </summary>
    //private IQueryable<$Entity$> FirstQueryable($Context$ context)
    //{
    //    return context.$Entity$.Where(x => x.$Entity$Id == this.$Entity$Id);
    //}

    ///// <summary>
    ///// 从Dto赋值值到自己
    ///// </summary>
    //public void CopyFormDto($Entity$Dto dto)
    //{
    //    $CopyFormDto$
    //}

    ///// <summary>
    ///// 自己的值复制到实体
    ///// </summary>
    //private void CopyToEntity($Entity$ entity)
    //{
    //    $CopyToEntity$
    //}

    ///// <summary>
    ///// Select
    ///// </summary>
    //public static IQueryable<$Entity$Dto> Select(IQueryable<$Entity$> query)
    //{
    //    return query.Select(x => new $Entity$Dto()
    //    {
    //        $Select$
    //    });
    //}
}
";

        dtoTemplate = dtoTemplate
            .Replace("$Namespace$", dtoClassTypeSymbol.ContainingNamespace.ToDisplayString())
            .Replace("$ClassName$", dtoClassTypeSymbol.Name);


        foreach (var prop in dtoClassTypeSymbol.GetMembers())
        {

        }

        foreach (var item in dtoClass.ChildNodes())
        {

        }

        context.AddSource($@"{dtoClassTypeSymbol.ContainingNamespace.ToDisplayString()}.{dtoClassTypeSymbol.Name}.cs", SourceText.From(dtoTemplate, Encoding.UTF8));

    }
}

