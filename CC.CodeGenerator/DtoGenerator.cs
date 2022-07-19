namespace CC.CodeGenerator;

[Generator]
public class DtoGenerator : ISourceGenerator
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
        public List<TypeDeclarationSyntax> CandidateClasses { get; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if ((syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
                || (syntaxNode is RecordDeclarationSyntax rds && rds.AttributeLists.Count > 0)
                )//有特性的类都进行候选，将来可以筛选出只有需要的特性的类
            {
                CandidateClasses.Add((TypeDeclarationSyntax)syntaxNode);
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

        //获得DtoAttribute类符号
        INamedTypeSymbol dtoAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoAttribute");

        //获得MappingAttribute类符号
        INamedTypeSymbol mappingAttribute = compilation.GetTypeByMetadataName("CC.CodeGenerator.MappingAttribute");

        //创建Dto扩展
        foreach (TypeDeclarationSyntax dtoClass in receiver.CandidateClasses)
        {
            CreateCode(context, compilation, dtoClass, dtoAttSymbol, mappingAttribute);
        }
    }

    public void CreateCode(GeneratorExecutionContext context, Compilation compilation,
        TypeDeclarationSyntax dtoClass,
        INamedTypeSymbol dtoAttSymbol, INamedTypeSymbol mappingAttribute)
    {
        //获得类的类型符号,如果无法获得，就跳出
        if (compilation.GetSemanticModel(dtoClass.SyntaxTree).GetDeclaredSymbol(dtoClass) is not ITypeSymbol classSymbol) return;

        //类的特性
        Lazy<ImmutableArray<AttributeData>> classAttributes = new Lazy<ImmutableArray<AttributeData>>(() => classSymbol.GetAttributes(), true);

        //类中的属性，使用延迟初始化，如果没有对应的特性就免去此处的反射操作优化性能
        Lazy<List<IPropertySymbol>> classProperties = new Lazy<List<IPropertySymbol>>(() =>
             classSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property)
                      .Where(x => x.Kind == SymbolKind.Property)//只保留属性
                      .Cast<IPropertySymbol>()
                      .Where(x => x.Type.IsValueType == true || x.Type?.MetadataName == "String")//排除非值类型的属性
                      .ToList(), true);

        CreateDto(context, dtoAttSymbol, classSymbol, classAttributes, classProperties);

        CreateMapping(context, mappingAttribute, classSymbol, classAttributes, classProperties);
    }

    #region Dto代码

    public void CreateDto(GeneratorExecutionContext context, INamedTypeSymbol dtoAttSymbol,
        ITypeSymbol classSymbol, Lazy<ImmutableArray<AttributeData>> classAttributes, Lazy<List<IPropertySymbol>> classProperties)
    {
        //寻找是否有DtoAttribute
        var dtoAttr = classAttributes.Value.FirstOrDefault(x => x.AttributeClass.Equals(dtoAttSymbol, SymbolEqualityComparer.Default));
        if (dtoAttr == null) return;

        #region Dto实体

        //获得dto成员列表
        var dtoProperties = classProperties.Value
            .Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.DtoIgnoreAttribute") == false)//排除忽略属性
            .ToList();

        #endregion

        #region EF实体

        //获得DBContext的名字
        var contextName = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Context").Value.Value?.ToString();
        //获得实体类型
        ITypeSymbol entitySymbol = dtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Entity").Value.Value as ITypeSymbol;
        //获得实体属性
        var entityProperties = entitySymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList() ?? new List<IPropertySymbol>();
        //获得实体主键
        var entityKeyIds = entityProperties.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "System.ComponentModel.DataAnnotations.KeyAttribute")).ToList();

        #endregion

        //类的类型
        var classTypeName = classSymbol.IsRecord ? "record" : "class";

        //组装代码
        string dtoCode = @$"
using CC.Core;
{(string.IsNullOrWhiteSpace(contextName) ? "" : "using Microsoft.EntityFrameworkCore;")}

namespace {classSymbol.ContainingNamespace.ToDisplayString()};

public partial {classTypeName} {classSymbol.Name}
{{

    #region 数据赋值

{CopyFormDto(classSymbol, dtoProperties, entitySymbol, entityProperties)}

{CopyToEntity(classSymbol, dtoProperties, entitySymbol, entityProperties)}

{EntitySelect(classSymbol, dtoProperties, entitySymbol, entityProperties)}

    #endregion

    #region 数据库操作

{New(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

{Load(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

{FirstQueryable(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

{ReLoad(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

{Save(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

{Delete(classSymbol, dtoProperties, entitySymbol, entityProperties, contextName, entityKeyIds)}

    #endregion 
}}

{EntitySelectExtension(classSymbol, dtoProperties, entitySymbol, entityProperties)}

";

        context.AddSource($@"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}.dto.g.cs", SourceText.From(dtoCode, Encoding.UTF8));
    }

    #region 数据赋值

    // 从Dto赋值值到自己
    private string CopyFormDto(ITypeSymbol dtoSymbol, IEnumerable<IPropertySymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties)
    {
        var code = AssignCode("this", dtoProperties, "dto", dtoProperties, ";");
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
    private string CopyToEntity(ITypeSymbol dtoSymbol, IEnumerable<IPropertySymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<IPropertySymbol> entityProperties)
    {
        if (entitySymbol == null) return null;
        var code = AssignCode("entity", entityProperties, "this", dtoProperties, ";");

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
    private string EntitySelect(ITypeSymbol dtoSymbol, IEnumerable<IPropertySymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<IPropertySymbol> entityProperties)
    {
        if (entitySymbol == null) return null;

        var code = AssignCode("", dtoProperties, "x", entityProperties, ",");

        return @$"
    /// <summary>
    /// EntitySelect
    /// </summary>
    [Obsolete(""使用扩展函数“SelectGen”替代"")]
    public static IQueryable<{dtoSymbol.Name}> SelectGen(IQueryable<{entitySymbol.Name}> query)
    {{
        return query.Select(x => new {dtoSymbol.Name}()
        {{
{code}
        }});
    }}";

    }

    private string EntitySelectExtension(ITypeSymbol dtoSymbol, IEnumerable<IPropertySymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<IPropertySymbol> entityProperties)
    {
        if (entitySymbol == null) return null;

        var code = AssignCode("", dtoProperties, "x", entityProperties, ",");

        return @$"
public static class {dtoSymbol.Name}Extension
{{
    /// <summary>
    /// EntitySelect
    /// </summary>
    public static IQueryable<{dtoSymbol.Name}> To{dtoSymbol.Name}s(this IQueryable<{entitySymbol.Name}> query)
    {{
        return query.Select(x => new {dtoSymbol.Name}()
        {{
{code}
        }});
    }}
}}";

    }

    #endregion

    #region 数据库操作
    private string New(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<IPropertySymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;

        List<string> keyInits = new List<string>();
        foreach (var keyId in keyIds)
        {
            if (keyId.Type.ToString() != "System.Guid") return "";//如果主键中包含非Guid的对象，那么就不要生成初始化代码
            keyInits.Add($"{keyId.Name} = Guid.NewGuid()");
        }
        var keyInit = keyInits.Aggregate((a, b) => a + ", " + b);

        return @$"
    /// <summary>
    /// 创建新实体[模拟工厂模式]
    /// </summary>
    /// <returns></returns>
    public static {dtoSymbol.Name} NewGen()
    {{
        return new {dtoSymbol.Name}() {{ {keyInit} }};
    }}";
    }

    private string Load(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<IPropertySymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;

        List<string> keyParameters = new List<string>();
        List<string> keyCompares = new List<string>();
        foreach (var keyId in keyIds)
        {
            keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
            keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
        }
        var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
        var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

        return @$"
    /// <summary>
    /// 载入已有实体
    /// </summary>
    /// <returns></returns>
    public static {dtoSymbol.Name}? LoadGen({contextName} context, {keyParameter})
    {{
        return context.{entitySymbol.Name}.Where(x => {keyCompare}).To{dtoSymbol.Name}s().FirstOrDefault();
    }}";
    }

    private string FirstQueryable(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<ISymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;

        List<string> keyCompares = new List<string>();
        foreach (var keyId in keyIds)
        {
            keyCompares.Add($"x.{keyId.Name} == this.{keyId.Name}");
        }
        var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

        return @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{entitySymbol.Name}> FirstQueryable({contextName} context)
    {{
        return context.{entitySymbol.Name}.Where(x => {keyCompare});
    }}";
    }

    //ReLoad 重新加载
    private string ReLoad(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<ISymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;

        return @$"
    /// <summary>
    /// 重新加载
    /// </summary>
    public Result ReLoadGen({contextName} context)
    {{
        var dto = FirstQueryable(context).To{dtoSymbol.Name}s().FirstOrDefault();
        if (dto == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        CopyFormDto(dto);
        return Result.OK;
    }}";
    }

    //Save 保存
    private string Save(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<ISymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;
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
            context.Add(entity);
        }}
        CopyToEntity(entity);
        return new Result<{entitySymbol.Name}>(entity);
    }}";
    }

    //Delete 删除
    private string Delete(ITypeSymbol dtoSymbol, IEnumerable<ISymbol> dtoProperties, ITypeSymbol entitySymbol, IEnumerable<ISymbol> entityProperties, string contextName, IEnumerable<IPropertySymbol> keyIds)
    {
        if (string.IsNullOrWhiteSpace(contextName) || keyIds.Count() == 0) return null;

        List<string> keyParameters = new List<string>();
        List<string> keyCompares = new List<string>();
        foreach (var keyId in keyIds)
        {
            keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
            keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
        }
        var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
        var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

        return @$"
    /// <summary>
    /// 删除，基于Dto
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
    }}

    /// <summary>
    /// 删除，基于主键
    /// </summary>
    public static Result DeleteGen({contextName} context, {keyParameter})
    {{
        var entity = context.{entitySymbol.Name}.Where(x => {keyCompare}).FirstOrDefault();
        if (entity == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        context.Remove(entity);
        return Result.OK;
    }}";
    }

    #endregion

    #endregion

    #region Mapping代码

    public void CreateMapping(GeneratorExecutionContext context, INamedTypeSymbol mappingAttribute,
         ITypeSymbol classSymbol, Lazy<ImmutableArray<AttributeData>> classAttributes, Lazy<List<IPropertySymbol>> classProperties)
    {
        //寻找是否有DtoAttribute
        var mappingAttr = classAttributes.Value.FirstOrDefault(x => x.AttributeClass.Equals(mappingAttribute, SymbolEqualityComparer.Default));
        if (mappingAttr == null) return;

        StringBuilder mappingBuilder = new StringBuilder();

        //获得dto成员列表
        var mappingProperties = classProperties.Value
            .Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.MappingIgnoreAttribute") == false)//排除忽略属性
            .ToList();

        var code = new StringBuilder();

        //获得目标类型
        List<ITypeSymbol> targetSymbols = mappingAttr.ConstructorArguments.FirstOrDefault().Values.Select(x => x.Value as ITypeSymbol).Distinct().ToList();

        foreach (var targetSymbol in targetSymbols)
        {
            //获得目标属性
            var targetProperties = targetSymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList() ?? new List<IPropertySymbol>();

            code.AppendLine(MappingCopy(classSymbol, mappingProperties, targetSymbol, targetProperties));
        }

        //检查是否有默认构造，如果有就不用创建默认，否则创建默认构造
        var defaultConstructor = "";
        var constructorDeclaration = (classSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax)?.Members.FirstOrDefault(x => x.Kind() == SyntaxKind.ConstructorDeclaration) as ConstructorDeclarationSyntax;
        if (constructorDeclaration == null || constructorDeclaration.ParameterList.Parameters.Count != 0)
        {
            defaultConstructor = $"    public {classSymbol.Name}() {{ }}";
        }

        //类的类型
        var classTypeName = classSymbol.IsRecord ? "record" : "class";

        //组装代码
        string dtoCode = @$"
using CC.Core;

namespace {classSymbol.ContainingNamespace.ToDisplayString()};

public partial {classTypeName} {classSymbol.Name}
{{
{defaultConstructor}
{code}
}}
";

        context.AddSource($@"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}.mapping.g.cs", SourceText.From(dtoCode, Encoding.UTF8));
    }

    // 映射复制
    private string MappingCopy(ITypeSymbol classSymbol, IEnumerable<IPropertySymbol> mappingProperties, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
    {
        if (targetSymbol == null) return null;

        var codeCopyTo = AssignCode("target", targetProperties, "this", mappingProperties, ";");
        var codeCopyFrom = AssignCode("this", mappingProperties, "source", targetProperties, ";");

        return @$"
    /// <summary>
    /// 基于源赋值初始化
    /// </summary>
    public {classSymbol.Name}({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
        CopyFrom(source);
    }}

    /// <summary>
    /// 将自己赋值到目标
    /// </summary>
    public {classSymbol.Name} CopyTo({targetSymbol.ContainingNamespace}.{targetSymbol.Name} target)
    {{
{codeCopyTo}
        return this;
    }}

    /// <summary>
    /// 从源赋值到自己
    /// </summary>
    public {classSymbol.Name} CopyFrom({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
{codeCopyFrom}
        return this;
    }}";



    }

    #endregion


    #region 公用方法

    /// <summary>
    /// 赋值代码
    /// </summary>
    public StringBuilder AssignCode(string leftName, IEnumerable<IPropertySymbol> leftProps,
        string rightName, IEnumerable<IPropertySymbol> rightProps, string separate)
    {
        var code = new StringBuilder();
        foreach (var leftProp in leftProps)
        {
            if (leftProp.IsReadOnly) continue;
            var rightProp = rightProps.FirstOrDefault(x => x.Name == leftProp.Name);
            if (rightProp == null) continue;
            code.AppendLine($"        {(string.IsNullOrWhiteSpace(leftName) ? "" : $"{leftName}.")}{leftProp.Name} = {rightName}.{rightProp.Name}{separate}");
        }
        return code;
    }

    #endregion
}
