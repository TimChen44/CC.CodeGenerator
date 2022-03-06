using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CC.CodeGenerator;

internal static class Extends
{

    /// <summary>
    /// 是否存在指定类型的Attribute
    /// </summary>
    public static bool IsExistsAttribute(this ITypeSymbol typeSymbol
        , INamedTypeSymbol target
        , out AttributeData attributeData)
    {
        attributeData = typeSymbol.GetAttributes().FirstOrDefault(x => Equals(x, target));
        return attributeData != null;
    }

    /// <summary>
    /// 是否存在指定Display的Attribute
    /// </summary>
    public static bool IsExistsDisplayAttribute(this ISymbol symbol, string target) =>
        symbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == target);

    private static bool Equals(this AttributeData attributeData, INamedTypeSymbol target) =>
        attributeData.AttributeClass.Equals(target, SymbolEqualityComparer.Default);


    /// <summary>
    /// 返回类的类型名称字符串"class" : "record"
    /// </summary>
    public static string GetTypeString(this ITypeSymbol type) => type.IsRecord ? "record" : "class";

    /// <summary>
    /// 获取与声明语法节点关联的符号。
    /// </summary>
    public static INamedTypeSymbol GetDeclaredSymbol(this Compilation compilation
        , TypeDeclarationSyntax typeDeclarationSyntax)
    {
        return compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree)
            .GetDeclaredSymbol(typeDeclarationSyntax) as INamedTypeSymbol;
    }
}

