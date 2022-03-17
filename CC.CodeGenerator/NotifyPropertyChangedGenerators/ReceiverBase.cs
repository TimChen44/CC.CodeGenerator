namespace CC.CodeGenerator;
public abstract class ReceiverBase : ISyntaxReceiver
{

    protected readonly List<NodeBase> nodes = new();

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);

    protected bool TestAttributeCount(MemberDeclarationSyntax member) => member.AttributeLists.Count > 0;

    public IEnumerable<NodeBase> Nodes => nodes;  
}
