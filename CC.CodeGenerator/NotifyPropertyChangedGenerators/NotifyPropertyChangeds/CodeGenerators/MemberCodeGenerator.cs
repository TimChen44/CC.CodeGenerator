#nullable enable
namespace CC.CodeGenerator.NotifyPropertyChangeds;

/// <summary>
/// 成员代码生成器
/// </summary>
internal class MemberShadowCode
{
    public MemberShadowCode()
    {

    }

    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName { get; set; } = null!;

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// 成员类型
    /// </summary>
    public string MemberType { get; set; } = null!;

    /// <summary>
    /// Xml文档内容
    /// </summary>
    public string? XmlSummary { get; set; }

    /// <summary>
    /// 是否需要创建字段
    /// </summary>
    public bool IsCreateField { get; set; }

    /// <summary>
    /// 是否需检查字段命名重复
    /// </summary>
    public bool IsCheckFiledName { get; set; } = true;

    /// <summary>
    /// 特性来源
    /// </summary>
    public AttributeData Source { get; set; } = null!;


    /// <summary>
    /// 创建xml文档
    /// </summary>
    public void CreateXmldoc(CodeBuilder code)
    {
        if (XmlSummary is null) return;
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        foreach (var item in XmlSummary.GetLines())
            sb.Append("/// ").AppendLine(item);
        sb.AppendLine("/// </summary>");
        code.AddMember(sb.ToString(), false);
    }


    /// <summary>
    /// 创建字段
    /// </summary>
    public void CreateField(CodeBuilder code)
    {
        CreateXmldoc(code);
        code.AddMember($"private {MemberType} {FieldName};");
    }

    /// <summary>
    /// 创建属性
    /// </summary>
    public void CreateProperty(CodeBuilder code, string setPropertyMethodName)
    {
        CreateXmldoc(code);
        code.AddMember($@"public {MemberType} {PropertyName} 
{{ 
   get => {FieldName}; 
   set => {setPropertyMethodName}(ref {FieldName}, value); 
}}");
    }



}
