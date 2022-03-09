using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace CC.CodeGenerator.NotifyPropertyChangedGenerators;

public abstract class ReceiverBase<T> : ISyntaxReceiver
    where T : SyntaxNode
{
    /// <summary>
    /// 需要生成Dto操作代码的成员节点
    /// </summary>
    public List<T> Nodes { get; } = new();

    /// <summary>
    /// 元素列表为空
    /// </summary>
    public virtual bool IsEmpty => Nodes .Count == 0;

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);

    /// <summary>
    /// 是否为Class 或者 Record 类型
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <param name="typeDeclaration">返回type定义</param>
    public static bool IsClassOrRecord(SyntaxNode syntaxNode
        , out TypeDeclarationSyntax typeDeclaration)
    {
        if (syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax)
        {
            typeDeclaration = (TypeDeclarationSyntax)syntaxNode;
            return true;
        }
        typeDeclaration = null;
        return false;
    }
}
