#pragma warning disable CS8632
using CC.CodeGenerato.NotifyPropertyChangeds.CodeBuilds;

namespace CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;
internal partial class FieldTargetValidation : TargetValidationBase<FieldDeclarationSyntax>
{
    public FieldTargetValidation(NodeData nodeData
        , FieldDeclarationSyntax member)
        : base(nodeData, member) { }

    protected override SyntaxNode GetAttributeProvider() => Member.Declaration.Variables[0];

    private string GetFieldNames() => 
        string.Join(", ", Member.Declaration.Variables.Select(x => $"{x}"));

    public override IEnumerable<CodeBuilderBase> CreateCodeBuilders()
    {
        var attrs = TargetValidation.Attributes;

        //规则检测
        if (!Rule1(attrs, out var error)) return new[]
        {
            new FieldCodeBuilder(this)
            {
                AttributeData = attrs.Last(),
                SyntaxNode = Member,
            }.ReportError("field",error!)
        };
        return Member.Declaration.Variables.Select(CreateFieldCodeBuilder);
    }

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
        var target = $"{NotifyPropertyGenerator.attributeCtor}(string";
        var ctor = attr.AttributeConstructor!.ToString();
        if (Member.Declaration.Variables.Count > 1 && ctor.StartsWith(target))
        {
            error = $"[{attr.AttributeClass?.Name}] 同时标记多个字段时 {GetFieldNames()}，无法修改“名称”和 “类型”";
            return false;
        }
        error = null;
        return true;
    }

    private FieldCodeBuilder CreateFieldCodeBuilder(VariableDeclaratorSyntax variable)
    {
        var fieldName = variable.Identifier.ToString();
        var propName = GetInitialLower(FormatName(GetPropName(fieldName)), false);
        var typeName = ((IFieldSymbol)MemberSymbol).Type.GetTypeName();
        return new(this)
        {
            AttributeData = TargetValidation.Attributes[0],
            SyntaxNode = variable,
            FieldName = fieldName,
            PropertyName = propName,
            TypeName = typeName,
            Variable = variable,
        };
    }

    private string? GetPropName(string defaultName)
    {
        var attr = TargetValidation.Attributes[0];
        var target = $"{NotifyPropertyGenerator.attributeCtor}(string";
        if (!attr.AttributeConstructor!.ToString().StartsWith(target)) return defaultName;
        return attr.GetCtorArgumentValue(0);
    }
}