#nullable enable
namespace CC.CodeGenerator.NotifyPropertyChangeds;
public class NotifyPropChangedReceiver : ReceiverBase<MemberDeclarationSyntax>
{
    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax or FieldDeclarationSyntax)
        {
            if (syntaxNode is MemberDeclarationSyntax mds && mds.AttributeLists.Count>0)
                nodes.Add((MemberDeclarationSyntax)syntaxNode);
        }      
    }
}
