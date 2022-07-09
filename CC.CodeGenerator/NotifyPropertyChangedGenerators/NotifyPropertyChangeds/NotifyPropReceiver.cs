using CC.CodeGenerator.NotifyPropertyChangeds.Nodes;
namespace CC.CodeGenerator.NotifyPropertyChangedGenerators.NotifyPropertyChangeds;
public class NotifyPropReceiver : ReceiverBase
{
    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case ClassDeclarationSyntax or RecordDeclarationSyntax:
                AddNode(new NotifyPropTypeNode() { SyntaxNode = syntaxNode });
                break;
            case FieldDeclarationSyntax:
                AddNode(new NotifyPropFieldNode() { SyntaxNode = syntaxNode });
                break;
            default: 
                break;
        }
    }
}
