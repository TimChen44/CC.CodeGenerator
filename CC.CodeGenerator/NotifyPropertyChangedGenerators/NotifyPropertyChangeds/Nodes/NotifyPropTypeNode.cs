#pragma warning disable CS8632 
using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using CC.CodeGenerator.NotifyPropertyChangedGenerators.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.CodeBuilds;

namespace CC.CodeGenerator.NotifyPropertyChangeds.Nodes;
/// <summary>
/// 验证类型上的特性
/// </summary>
internal partial class NotifyPropTypeNode : NotifyPropNodeBase
{
    internal override IEnumerable<NotifyPropCodeBuilderBase> CreateCodeBuilders() => 
        TargetData.Attributes.Select(CreateTypeCodeBuilder);

    private NotifyPropTypeCodeBuilder CreateTypeCodeBuilder(AttributeData attribute)
    {
        var ctor = attribute.AttributeConstructor!;
        var propName = GetPropName(ctor, attribute);
        var fieldName = GetInitialLower(propName);
        if (fieldName is not null) fieldName = $"_{fieldName}";
        var typeName = GetPropType(ctor, attribute);
        return new(this)
        {
            AttributeData = attribute,
            SyntaxNode = SyntaxNode,
            PropertyName = propName,
            FieldName = fieldName,
            TypeName = typeName,
            ParameterCount = ctor.Parameters.Length,
        };
    }

    private string? GetPropName(IMethodSymbol method, AttributeData attribute)
    {
        var target = $"{NotifyPropGenerator.attributeCtor}(string";
        if (!method.ToString().StartsWith(target)) return default;
        var res = attribute.GetCtorArgumentValue(0)!;
        return FormatName(res);
    }

    private string? GetPropType(IMethodSymbol method, AttributeData attribute)
    {
        var target = $"{NotifyPropGenerator.attributeCtor}(string, System.Type)";
        if (!method.ToString().StartsWith(target)) return default;
        var res = (ITypeSymbol)attribute.ConstructorArguments[1].Value!;
        return res.GetTypeName();
    }
}
