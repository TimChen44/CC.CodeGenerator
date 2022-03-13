#nullable enable
namespace CC.CodeGenerator.NotifyPropertyChangeds;
/// <summary>
/// 类型分组
/// </summary>
internal class NotifyPropChangedCodeManager
{
    private readonly Dictionary<string, TypeShadowCode> typeDefinitions = new();

    internal void CreateCode(GeneratorExecutionContext context) => 
        typeDefinitions.Values.ToList().ForEach(x => x.CreateCode(context));



    public TypeShadowCode GetTypeDefinition(INamedTypeSymbol typeDefinition)
    {
        var name = typeDefinition.ToString();
        if (!typeDefinitions.TryGetValue(name, out var res))
        {
            typeDefinitions[name] = res = new TypeShadowCode(typeDefinition);
        }
        return res;
    }
}
