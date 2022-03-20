#pragma warning disable CS8632 
using CC.CodeGenerator.NotifyPropertyChangeds.Nodes;
namespace CC.CodeGenerator.NotifyPropertyChangeds.CodeBuilds;

/// <summary>
/// 负责构建代码
/// </summary>
public abstract class NotifyPropCodeBuilderBase
{
    private readonly Lazy<Location> attributeLocation;

    public NotifyPropCodeBuilderBase(NotifyPropNodeBase node)
    {
        Node = node;
        attributeLocation = new(GetLocation);
    }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string? PropertyName { get; set; } = null!;

    /// <summary>
    /// 字段名称
    /// </summary>
    public string? FieldName { get; set; } = null!;

    /// <summary>
    /// 类型
    /// </summary>
    public string? TypeName { get; set; } = null!;

    public string? XmlSummary => AttributeData
        .GetNamedArgumentValue("XmlSummary");

    public string? SetPropertyMethodName => AttributeData
        .GetNamedArgumentValue("SetPropertyMethodName");

    public string? OnPropertyChangedMethodName => AttributeData
        .GetNamedArgumentValue("OnPropertyChangedMethodName");

    /// <summary>
    /// 当前符号
    /// </summary>
    public SyntaxNode SyntaxNode { get; set; } = null!;

    /// <summary>
    /// 特性
    /// </summary>
    public AttributeData AttributeData { get; set; } = null!;

    public Location AttributeLocation => attributeLocation.Value;

    public string? Error { get; protected set; }

    public NotifyPropNodeBase Node { get; }

    public virtual bool IsBuild() => Error is null;

    private Location GetLocation() => AttributeData.GetSyntaxNode().GetLocation();


    /// <summary>
    /// 检查名字
    /// </summary>
    public abstract void TestName(Dictionary<string, Location?> memberNames);

    /// <summary>
    /// 使用规则
    /// </summary>
    public abstract void TestRule();

    /// <summary>
    /// 如果错误为空就继续执行
    /// </summary>
    protected void TestRules(params Action[] actions)
    {
        foreach (var item in actions)
        {
            if (Error is not null) return;
            item();
        }
    }

    public virtual bool ReportError(string id, string err, bool result = false
        , Location? source = null, int offset = 1)
    {
        source ??= AttributeLocation;
        Error = err;
        return Node.ContextData.Context.ReportError(new(source, id, err) { LocationOffset = offset }, result);
    }

    public NotifyPropCodeBuilderBase ReportError(string id, string err)
    {
        ReportError(id, err, false);
        return this;
    }

    internal void CreateCode(CodeBuilder codeBuilder, string setPropName)
    {
        codeBuilder.AddLine();
        if (Error is not null)
            codeBuilder.AddCode($"// >>> {Error}")
                .AddCode("#if _____generate_abnormal_____");

        CreateProperty(codeBuilder, setPropName);

        if (Error is not null)
            codeBuilder.AddCode("#endif");
    }

    internal virtual CodeBuilder CreateProperty(CodeBuilder codeBuilder, string setPropName) =>
        AddXml(codeBuilder)
        .AddCode($"public {GetPlaceholder(TypeName)} {GetPlaceholder(PropertyName)}")
        .AddLine("{")
        .AddTab(x => x.AddCode($"get => {GetPlaceholder(FieldName)};")
                      .AddCode($"set => {setPropName}(ref {GetPlaceholder(FieldName)}, value);")
        ).AddLine("}");


    protected CodeBuilder AddXml(CodeBuilder codeBuilder)
    {
        if (XmlSummary is not null)
        {
            var insert = "/// ";
            codeBuilder.AddCode("<summary>", insert)
                .AddCode(XmlSummary, insert)
                .AddCode("</summary>", insert);
        }
        return codeBuilder;
    }

    protected string GetPlaceholder(string? value) => value ?? "?";

    protected void TestName(Dictionary<string, Location?> memberNames, string? memberName)
    {
        if (Error is not null || !IsExistsName(memberNames, memberName!, AttributeLocation)) return;
        var err = $"类型 \"{Node.TargetData.ContainingType.Name}\" 已经包含“{memberName}”的定义";
        ReportError("NameConflict", err);
    }

    private bool IsExistsName(Dictionary<string, Location?> memberNames, string memberName, Location? location)
    {
        if (memberNames.ContainsKey(memberName)) return true;
        memberNames[memberName] = location;
        return false;
    }
}