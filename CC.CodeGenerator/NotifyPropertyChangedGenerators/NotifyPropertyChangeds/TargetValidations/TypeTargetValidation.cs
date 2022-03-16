#pragma warning disable CS8632
using CC.CodeGenerato.NotifyPropertyChangeds.CodeBuilds;
namespace CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;
internal partial class TypeTargetValidation : TargetValidationBase<MemberDeclarationSyntax>
{
    public TypeTargetValidation(NodeData nodeData, MemberDeclarationSyntax member)
        : base(nodeData, member)
    {
    }

    public override IEnumerable<CodeBuilderBase> CreateCodeBuilders() =>
        TargetValidation.Attributes
        .Select(CreateTypeCodeBuilder);

    private TypeCodeBuilder CreateTypeCodeBuilder(AttributeData attribute)
    {
        var ctor = attribute.AttributeConstructor!;
        var propName = GetPropName(ctor, attribute);
        var fieldName = GetInitialLower(propName);
        if (fieldName is not null) fieldName = $"_{fieldName}";
        var typeName = GetPropType(ctor, attribute);
        return new(this)
        {
            AttributeData = attribute,
            SyntaxNode = Member,
            PropertyName = propName,
            FieldName = fieldName,
            TypeName = typeName,
            ParameterCount = ctor.Parameters.Length,
        };
    }

    private string? GetPropName(IMethodSymbol method, AttributeData attribute)
    {
        var target = $"{NotifyPropertyGenerator.attributeCtor}(string";
        if (!method.ToString().StartsWith(target)) return default;
        var res = attribute.GetCtorArgumentValue(0)!;
        return GetInitialLower(FormatName(res), false);
    }

    private string? GetPropType(IMethodSymbol method, AttributeData attribute)
    {
        var target = $"{NotifyPropertyGenerator.attributeCtor}(string, System.Type)";
        if (!method.ToString().StartsWith(target)) return default;
        var res = (ITypeSymbol)attribute.ConstructorArguments[1].Value!;
        return res.GetTypeName();
    }
}
