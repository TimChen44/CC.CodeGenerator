#pragma warning disable CS8632 
using CC.CodeGenerator.NotifyPropertyChangeds.Nodes;

namespace CC.CodeGenerator.NotifyPropertyChangeds.CodeBuilds;

//使用特性创建属性
internal partial class NotifyPropTypeCodeBuilder : NotifyPropCodeBuilderBase
{
    public NotifyPropTypeCodeBuilder(NotifyPropTypeNode node) : base(node) { }

    public int ParameterCount { get; set; }

    public override bool IsBuild() => base.IsBuild() && ParameterCount > 0;

    public override void TestName(Dictionary<string, Location?> memberNames)
    {
        if (!(IsBuild() && PropertyName is not null && FieldName is not null)) return;
        TestName(memberNames, PropertyName);
        TestName(memberNames, FieldName);
    }

    public override void TestRule()
    {
        if (IsBuild()) TestRules(Rule1, Rule2);
    }

    /// <summary>
    /// 规则 1 : 在类型上创建属性,必须设置 PropertyType.
    /// </summary>
    private void Rule1()
    {
        if (!string.IsNullOrEmpty(TypeName)) return;
        ReportError("Type01", "未提供必需形参“propertyType”, 在类型上创建属性时不能为空。");
    }

    /// <summary>
    /// 规则 2 : PropertyName 不能为空。
    /// </summary>
    private void Rule2()
    {
        if (PropertyName?.Length > 0) return;
        ReportError("Type02", "未提供必需形参“propertyName”, 在类型上创建属性时不能为\"\"、全符号、全数字，请参考命名规则。");
    }

    internal override CodeBuilder CreateProperty(CodeBuilder codeBuilder, string setPropName)
    {
        if (ParameterCount is 0) return codeBuilder;
        codeBuilder.AddCode($"private {TypeName} {FieldName};");
        return base.CreateProperty(codeBuilder, setPropName);
    }   
}