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
using System.Collections.Immutable;

namespace CC.CodeGenerator;

[Generator]
public class DtoGenerator : ISourceGenerator
{

    public void Initialize(GeneratorInitializationContext context)
    {
//#if DEBUG
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif

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

        //把DtoAttribute加入当前的编译中
        Compilation compilation = context.Compilation.AddSyntaxTrees(dtoAtt);

        //获得DtoAttribute类符号
        INamedTypeSymbol dtoAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoAttribute");

        //创建Dto扩展
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
            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)//有特性的类都进行候选，将来可以筛选出只有需要的特性的类
            {
                CandidateClasses.Add(cds);
            }
        }
    }



    /// <summary>
    /// 创建DtoAttribute代码
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public SyntaxTree CreateDtoAttribute(GeneratorExecutionContext context)
    {
        string attTemplate = @"
namespace CC.CodeGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DtoAttribute: Attribute
{
    public string DBContext { get; set; }

    public Type Entity { get; set; }

    public string KeyId { get; set; }
}";
        SourceText sourceText = SourceText.From(attTemplate, Encoding.UTF8);
        context.AddSource("DtoAttribute.cs", sourceText);

        return CSharpSyntaxTree.ParseText(SourceText.From(attTemplate, Encoding.UTF8));
    }

    public void CreateDto(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol dtoAttSymbol, ClassDeclarationSyntax dtoClass)
    {
        //获得dto类的类型符号
        if (compilation.GetSemanticModel(dtoClass.SyntaxTree).GetDeclaredSymbol(dtoClass) is not ITypeSymbol dtoSymbol) return;

        //寻找是否有DtoAttribute
        var dtoAttr = dtoSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(dtoAttSymbol, SymbolEqualityComparer.Default));
        if (dtoAttr == null) return;

        //获得dto成员列表
        var dtoProperties = dtoSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property).ToList();

        //获得DBContext的名字
        var contextName = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "DBContext").Value.Value?.ToString();

        //获得实体类型
        ITypeSymbol entitySymbol = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Entity").Value.Value as ITypeSymbol;
        var entityProperties = entitySymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).ToList();

        //获得实体主键
        var entityKeyId = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "KeyId").Value.Value?.ToString();

        //组装代码
        string dtoCode = @$"
namespace {dtoSymbol.ContainingNamespace.ToDisplayString()};

public partial class {dtoSymbol.Name}
{{
{FirstQueryable(dtoSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyId)}

{CopyFormDto(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}

{CopyToEntity(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}

{EntitySelect(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}
}}
";

        context.AddSource($@"{dtoSymbol.ContainingNamespace.ToDisplayString()}.{dtoSymbol.Name}.cs", SourceText.From(dtoCode, Encoding.UTF8));

    }

    private string FirstQueryable(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId)) return null;

        return @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{entitySymbol.Name}> FirstQueryable({contextName} context)
    {{
        return context.{entitySymbol.Name}.Where(x => x.{keyId} == this.{keyId});
    }}";
    }

    // 从Dto赋值值到自己
    private string CopyFormDto(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties)
    {
        var code = new StringBuilder();
        foreach (var prop in dtoProperties.Where(x => x.Kind == SymbolKind.Property))
        {
            code.AppendLine($"        this.{prop.Name} = dto.{prop.Name};");
        }

        return @$"
    /// <summary>
    /// 从Dto赋值值到自己
    /// </summary>
    public virtual void CopyFormDto({dtoSymbol.Name} dto)
    {{
{code.ToString()}
    }}";

    }

    // 自己的值复制到实体
    private string CopyToEntity(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties)
    {
        if (entitySymbol == null) return null;

        var code = new StringBuilder();
        foreach (var entityProp in entityProperties)
        {
            var dtoProp = dtoProperties.FirstOrDefault(x => x.Name == entityProp.Name);
            if (dtoProp == null) continue;
            code.AppendLine($"        entity.{entityProp.Name} = this.{dtoProp.Name};");
        }

        return @$"    
    /// <summary>
    /// 自己的值复制到实体
    /// </summary>
    public virtual void CopyToEntity({entitySymbol.Name} entity)
    {{
{code.ToString()}
    }}";

    }

    //EntitySelect
    private string EntitySelect(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties)
    {
        if (entitySymbol == null) return null;

        var code = new StringBuilder();
        foreach (var entityProp in entityProperties)
        {
            var dtoProp = dtoProperties.FirstOrDefault(x => x.Name == entityProp.Name);
            if (dtoProp == null) continue;
            code.AppendLine($"            {dtoProp.Name} = x.{entityProp.Name}");
        }

        return @$"
    /// <summary>
    /// EntitySelect
    /// </summary>
    public static IQueryable<{dtoSymbol.Name}> Select(IQueryable<{entitySymbol.Name}> query)
    {{
        return query.Select(x => new {dtoSymbol.Name}()
        {{
{code}
        }});
    }}";

    }
}

