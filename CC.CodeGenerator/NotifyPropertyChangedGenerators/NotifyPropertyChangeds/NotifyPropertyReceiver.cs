#pragma warning disable CS8632
namespace CC.CodeGenerato.NotifyPropertyChangeds;
public class NotifyPropertyReceiver : ReceiverBase<MemberDeclarationSyntax>
{
    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        MemberDeclarationSyntax? member = syntaxNode switch
        {
            ClassDeclarationSyntax classSyntax => classSyntax,
            RecordDeclarationSyntax recordSyntax => recordSyntax,
            FieldDeclarationSyntax fieldSyntax => fieldSyntax,
            _ => default
        };
        if (member?.AttributeLists.Count > 0) nodes.Add(member);
    }
}
