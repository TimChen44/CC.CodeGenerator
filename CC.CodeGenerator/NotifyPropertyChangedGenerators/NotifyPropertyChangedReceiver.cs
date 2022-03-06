using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CC.CodeGenerator;

internal class NotifyPropertyChangedReceiver : ReceiverBase<TypeDeclarationSyntax>
{   
    public override void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (IsClassOrRecord(syntaxNode, out var typeDefault)) 
            Nodes.Add(typeDefault);
    }
}
