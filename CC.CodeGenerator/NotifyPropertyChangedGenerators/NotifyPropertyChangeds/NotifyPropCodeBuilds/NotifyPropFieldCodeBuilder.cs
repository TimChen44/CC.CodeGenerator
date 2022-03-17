using CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropValidations;
namespace CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropCodeBuilds;

//使用字段创建属性
internal partial class NotifyPropFieldCodeBuilder : NotifyPropCodeBuilderBase
{
    public NotifyPropFieldCodeBuilder(NotifyPropNodeBase node) : base(node) { }

    public VariableDeclaratorSyntax Variable { get; set; } = null!;

    public override void TestName(Dictionary<string, Location?> memberNames)
    {
        if (IsBuild() && PropertyName is not null)
            TestName(memberNames, PropertyName);
    }

    public override void TestRule()
    {
        if (base.IsBuild()) TestRules(Rule1, Rule2, Rule3);
    }

    /// <summary>
    /// 字段规则1:在字段无法上自定义函数名.
    /// </summary>
    private void Rule1()
    {
        if (SetPropertyMethodName is null && OnPropertyChangedMethodName is null) return;
        ReportError("field01", $"在字段 {Variable.Identifier} 上无法自定义函数名 。(SetProperty, OnPropertyChanged 仅在类型上设置有效。)");
    }

    /// <summary>
    /// 字段规则2 ：属性名不能为空
    /// </summary>
    private void Rule2()
    {
        if (!string.IsNullOrEmpty(PropertyName)) return;
        var location = default(Location);
        var offset = 1;

        if (NotifyPropNodeBase.FormatName(Variable.ToString()) is null)
        {
            location = Variable.GetLocation();
            offset = 0;
        }
        ReportError("field02", $"使用字段 {Variable.Identifier} 创建属性时，字段名或设置的属性名不能为 \"\"、全符号、全数字，请参考命名规则。",
            source: location, offset: offset);
    }

    /// <summary>
    /// 字段规则3 : 在字段上创建属性时不能修改类型，参数 “propertyType”无效
    /// </summary>
    private void Rule3()
    {
        var target = $"{NotifyPropGenerator.attributeCtor}(string, System.Type)";
        var ctor = AttributeData.AttributeConstructor!.ToString();
        if (ctor.StartsWith(target))
            ReportError("field03", $"在字段 {Variable.Identifier} 上创建属性时 不能修改类型，参数 “propertyType”无效");
    }
}
