using CC.CodeGenerator.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.Nodes;

namespace CC.CodeGenerator.NotifyPropertyChangedGenerators.NotifyPropertyChangeds;

[Generator]
public class NotifyPropGenerator : GeneratorBase
{
    private readonly Dictionary<string, NotifyPropCodeBuildManager> types = new();
    public const string attributeName = "AddNotifyPropertyChangedAttribute";
    public const string attributePath = $"CC.CodeGenerator.{attributeName}";
    public const string attributeCtor = $"{attributePath}.{attributeName}";

    public override void Initialize(GeneratorInitializationContext context)
    {
#if !DEBUG
        DebuggerLaunch();
#endif
        base.Initialize(context);
    }
    protected override void Run(GeneratorExecutionContext context)
    {
        types.Clear();
        base.Run(context);
    }

    protected override string AttributeFullName => attributePath;

    protected override ISyntaxReceiver GetSyntaxReceiver() => new NotifyPropReceiver();

    protected override void BuildCode() => types.Values.ToList().ForEach(x => x.Build());

    protected override void MergeType(IEnumerable<NodeBase> nodes) => nodes
        .ToList<NotifyPropNodeBase>().ForEach(MergeType);

    private void MergeType(NotifyPropNodeBase node)
    {
        var type = node.TargetData.ContainingType;
        if (type == null) return;

        var key = node.TargetData.ContainingType.ToString();
        if (!types.TryGetValue(key, out var buildManager))
            types[key] = buildManager = new();
        buildManager.AddNode(node);
    }
}