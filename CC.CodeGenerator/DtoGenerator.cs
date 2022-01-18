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

        //把DtoAttribute加入当前的编译中
        Compilation compilation = context.Compilation.AddSyntaxTrees(dtoAtt);

        //获得DtoAttribute类符号
        INamedTypeSymbol dtoAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoAttribute");

        //创建Dto扩展
        foreach (TypeDeclarationSyntax dtoClass in receiver.CandidateClasses)
        {
            CreateDto(context, compilation, dtoAttSymbol, dtoClass);
        }

    }

    class DtoSyntaxReceiver : ISyntaxReceiver
    {
        //需要生成Dto操作代码的类
        public List<TypeDeclarationSyntax> CandidateClasses { get; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if ((syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
                || (syntaxNode is RecordDeclarationSyntax rds && rds.AttributeLists.Count > 0))//有特性的类都进行候选，将来可以筛选出只有需要的特性的类
            {
                CandidateClasses.Add((TypeDeclarationSyntax)syntaxNode);
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

//标记类是否时Dto类
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DtoAttribute: Attribute
{
    public string DBContext { get; set; }

    public Type Entity { get; set; }

    public string KeyId { get; set; }
}

//标记属性是否需要忽略
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IgnoreAttribute : Attribute
{
}
";
        SourceText sourceText = SourceText.From(attTemplate, Encoding.UTF8);
        context.AddSource("DtoAttribute.cs", sourceText);

        return CSharpSyntaxTree.ParseText(SourceText.From(attTemplate, Encoding.UTF8));
    }

    public void CreateDto(GeneratorExecutionContext context, Compilation compilation, INamedTypeSymbol dtoAttSymbol, TypeDeclarationSyntax dtoClass)
    {
        //获得dto类的类型符号
        if (compilation.GetSemanticModel(dtoClass.SyntaxTree).GetDeclaredSymbol(dtoClass) is not ITypeSymbol dtoSymbol) return;

        //寻找是否有DtoAttribute
        var dtoAttr = dtoSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(dtoAttSymbol, SymbolEqualityComparer.Default));
        if (dtoAttr == null) return;

        //获得dto成员列表
        var dtoProperties = dtoSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property)
            .Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.IgnoreAttribute") == false)//排除忽略属性
            .Where(x =>
            {
                var type = ((Microsoft.CodeAnalysis.IPropertySymbol)x).Type;
                if (type?.BaseType?.Name == "ValueType") return true;
                if (type?.MetadataName == "String") return true;
                return false;
            })//排除非值类型的属性
            .ToList();

        //获得DBContext的名字
        var contextName = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "DBContext").Value.Value?.ToString();

        //获得实体类型
        ITypeSymbol entitySymbol = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Entity").Value.Value as ITypeSymbol;
        var entityProperties = entitySymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).ToList();

        //获得实体主键
        var entityKeyId = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "KeyId").Value.Value?.ToString();

        var typeName = (dtoClass is ClassDeclarationSyntax) ? "class" : "record";

        //组装代码
        string dtoCode = @$"
namespace {dtoSymbol.ContainingNamespace.ToDisplayString()};

public partial {typeName} {dtoSymbol.Name}
{{

#region 数据赋值

{CopyFormDto(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}

{CopyToEntity(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}

{EntitySelect(dtoSymbol, dtoProperties, entitySymbol, entityProperties)}

#endregion

#region 数据库操作

{FirstQueryable(dtoSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyId)}

{ReLoad(dtoSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyId)}

{Save(dtoSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyId)}

{Delete(dtoSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyId)}

#endregion 
}}
";

        context.AddSource($@"{dtoSymbol.ContainingNamespace.ToDisplayString()}.{dtoSymbol.Name}.cs", SourceText.From(dtoCode, Encoding.UTF8));

    }

    private string FirstQueryable(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, string keyId)
    {
        if (string.IsNullOrWhiteSpace(contextName) || string.IsNullOrWhiteSpace(keyId)) return null;

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
        foreach (var prop in dtoProperties)
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
            code.AppendLine($"            {dtoProp.Name} = x.{entityProp.Name},");
        }

        return @$"
    /// <summary>
    /// EntitySelect
    /// </summary>
    public static IQueryable<{dtoSymbol.Name}> SelectQueryable(IQueryable<{entitySymbol.Name}> query)
    {{
        return query.Select(x => new {dtoSymbol.Name}()
        {{
{code}
        }});
    }}";

    }

    //ReLoad 重新加载
    private string ReLoad(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, string keyId)
    {
        if (string.IsNullOrWhiteSpace(contextName) || string.IsNullOrWhiteSpace(keyId)) return null;

        return @$"
    /// <summary>
    /// 重新加载
    /// </summary>
    public Result ReLoadGen({contextName} context)
    {{
        var dto = SelectQueryable(FirstQueryable(context)).FirstOrDefault();
        if (dto == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        CopyFormDto(dto);
        return Result.OK;
    }}";
    }


    //Save 保存
    private string Save(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, string keyId)
    {
        if (string.IsNullOrWhiteSpace(contextName) || string.IsNullOrWhiteSpace(keyId)) return null;
        if (entitySymbol == null) return null;

        return @$"
    /// <summary>
    /// 保存
    /// </summary>
    public Result<{entitySymbol.Name}> SaveGen({contextName} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            entity = new {entitySymbol.Name}();
            entity.{keyId} = this.{keyId};  //主键
            context.Add(entity);
        }}
        CopyToEntity(entity);
        return new Result<{entitySymbol.Name}>(entity);
    }}";
    }

    //Delete 删除
    private string Delete(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, string keyId)
    {
        if (string.IsNullOrWhiteSpace(contextName) || string.IsNullOrWhiteSpace(keyId)) return null;

        return @$"
    /// <summary>
    /// 删除
    /// </summary>
    public Result DeleteGen({contextName} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        context.Remove(entity);
        return Result.OK;
    }}";
    }

}

