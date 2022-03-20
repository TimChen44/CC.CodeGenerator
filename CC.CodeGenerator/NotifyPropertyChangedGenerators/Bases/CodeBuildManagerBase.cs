namespace CC.CodeGenerator;

public abstract partial class CodeBuildManagerBase : ICodeBuilder
{
    public abstract void Build();
    public abstract void AddNode(NodeBase item);
}

public abstract partial class CodeBuildManagerBase<T> : CodeBuildManagerBase
    where T : NodeBase
{
    private readonly List<T> items = new();
    public IEnumerable<T> Items => items;
    public override void AddNode(NodeBase item) => items.Add((T)item);
}
