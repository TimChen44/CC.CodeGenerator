using CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropCodeBuilds;

namespace CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropValidations;
/// <summary>
/// 验证字段上的特性
/// </summary>
internal partial class NotifyPropFieldNode : NotifyPropNodeBase
{
    private readonly Lazy<VariableDeclaratorSyntax[]> variables;

    public NotifyPropFieldNode()
    {
        variables = new(() => ((FieldDeclarationSyntax)SyntaxNode).Declaration.Variables.ToArray());
    }

    protected override SyntaxNode GetAttributeProvider() => variables.Value[0];

    internal override IEnumerable<NotifyPropCodeBuilderBase> CreateCodeBuilders()
    {
        var attrs = TargetData.Attributes;

        //规则检测
        if (!Rule1(attrs, out var error)) return new[]
        {
            new NotifyPropFieldCodeBuilder(this)
            {
                AttributeData = attrs.Last(),
                SyntaxNode = SyntaxNode,
            }.ReportError("field",error!)
        };
        return variables.Value.Select(CreateFieldCodeBuilder);
    }

    private string GetFieldNames() => string.Join(", ", variables.Value.Select(x => $"{x.Identifier}"));

   

    //字段规则 1 : 特性在字段上只能标记一次, 多次标记会导致歧义.
    private bool Rule1(AttributeData[] attrs, out string? error)
    {
        if (attrs.Length > 1)
        {
            error = $"[{attrs[0].AttributeClass?.Name}]特性 在字段 {GetFieldNames()} 上只能标记一次, 多次标记会导致歧义.";
            return false;
        }
        return Rule2(attrs.First(), out error);
    }

    //字段规则 2 :同时标记多个字段时，无法指定 propertyName 。
    private bool Rule2(AttributeData attr, out string? error)
    {
        var target = $"{NotifyPropGenerator.attributeCtor}(string";
        var ctor = attr.AttributeConstructor!.ToString();
        if (variables.Value.Length > 1 && ctor.StartsWith(target))
        {
            error = $"[{attr.AttributeClass?.Name}] 同时标记多个字段时 {GetFieldNames()}，无法修改“名称”和 “类型”";
            return false;
        }
        error = null;
        return true;
    }

    private NotifyPropFieldCodeBuilder CreateFieldCodeBuilder(VariableDeclaratorSyntax variable)
    {
        var fieldName = variable.Identifier.ToString();
        var propName = GetInitialLower(FormatName(GetPropName(fieldName)), false);
        var typeName = ((IFieldSymbol)TargetData.Symbol).Type.GetTypeName();
        return new(this)
        {
            AttributeData = TargetData.Attributes[0],
            SyntaxNode = variable,
            FieldName = fieldName,
            PropertyName = propName,
            TypeName = typeName,
            Variable = variable,
        };
    }

    private string? GetPropName(string defaultName)
    {
        var attr = TargetData.Attributes[0];
        var target = $"{NotifyPropGenerator.attributeCtor}(string";
        if (!attr.AttributeConstructor!.ToString().StartsWith(target)) return defaultName;
        return attr.GetCtorArgumentValue(0);
    }
}
