﻿#pragma warning disable CS8632
using CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;

namespace CC.CodeGenerato.NotifyPropertyChangeds.CodeBuilds;

/// <summary>
/// 用于构建代码
/// </summary>
internal abstract class CodeBuilderBase
{
    private readonly Lazy<Location> attributeLocation;

    public CodeBuilderBase() => attributeLocation = new(GetLocation);

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

    public abstract bool ReportError(string id, string err, bool result = false
        , Location? source = null, int offset = 1);

    public CodeBuilderBase ReportError(string id, string err)
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


}

internal abstract class CodeBuilderBase<T> : CodeBuilderBase where T : TargetValidationBase
{
    public CodeBuilderBase(T node) => Node = node;

    public T Node { get; }

    public override bool ReportError(string id, string err, bool result = false
        , Location? location = null, int offset = 1)
    {
        location ??= AttributeLocation;
        Error = err;
        return Node.NodeData.Context.ReportError(new(location, id, err) { LocationOffset = offset }, result);
    }


    protected void TestName(Dictionary<string, Location?> memberNames, string? memberName)
    {
        if (Error is not null || !IsExistsName(memberNames, memberName!, AttributeLocation)) return;
        var err = $"类型 \"{Node.ContainingType.Name}\" 已经包含“{memberName}”的定义";
        ReportError("NameConflict", err);
    }

    private bool IsExistsName(Dictionary<string, Location?> memberNames, string memberName, Location? location)
    {
        if (memberNames.ContainsKey(memberName)) return true;
        memberNames[memberName] = location;
        return false;
    }
}