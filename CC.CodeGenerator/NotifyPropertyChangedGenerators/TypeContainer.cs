#pragma warning disable CS8632
namespace CC.CodeGenerato;
internal class TypeContainer<T> : ICodeBuilder where T : ICodeBuilder, new()
{
    private readonly Dictionary<string, T> items = new();

    public T GetItem(INamedTypeSymbol namedType)
    {
        var name = namedType.Name;
        if (!items.TryGetValue(name, out var res))
            items[name] = res = new T();
        return res;
    }

    public void Build()
    {
        foreach (var item in items.Values) item.Build();
    }
}
