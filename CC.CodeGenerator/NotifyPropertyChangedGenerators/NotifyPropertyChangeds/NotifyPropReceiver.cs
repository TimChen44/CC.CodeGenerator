using CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropValidations;
namespace CC.CodeGenerator.NotifyPropertyChangeds;
public class NotifyPropReceiver : ReceiverBase
{
    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case ClassDeclarationSyntax cds:
                AddNode(cds, () => new NotifyPropTypeNode() { SyntaxNode = syntaxNode });
                break;
            case RecordDeclarationSyntax rds:
                AddNode(rds, () => new NotifyPropTypeNode() { SyntaxNode = syntaxNode });
                break;
            case FieldDeclarationSyntax fds:
                AddNode(fds, () => new NotifyPropFieldNode() { SyntaxNode = syntaxNode });
                break;
            default:
                break;
        }
    }

    private void AddNode(MemberDeclarationSyntax member, Func<NodeBase> func)
    {
        if (TestAttributeCount(member)) nodes.Add(func());
    }

}
