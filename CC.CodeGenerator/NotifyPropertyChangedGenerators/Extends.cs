#nullable enable
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Reflection;

namespace CC.CodeGenerator;

internal static class Extends
{

    #region SyntaxNode

    /// <summary>
    /// 获取与声明关联的符号
    /// </summary>
    public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation) =>
               compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

    #endregion

    #region GeneratorExecutionContext

    public static void ReportDiagnostic(this GeneratorExecutionContext context
        , SyntaxNode syntaxNode
        , DiagnosticSeverity diagnosticSeverity
        , string id, string error, string? helpLinkUri = null, int offset = 1)
    {
        var diagnosticDescriptor = new DiagnosticDescriptor
        (
            id,
            "ShadowCode",
            messageFormat: "{0}",
            category: "ShadowCode",
            defaultSeverity: diagnosticSeverity,
            isEnabledByDefault: true,
            helpLinkUri: helpLinkUri
        );

        var location = GetLocation(syntaxNode, offset);
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, error);
        context.ReportDiagnostic(diagnostic);
    }

    private static Location GetLocation(SyntaxNode syntaxNode, int offset)
    {
        var span = syntaxNode.Span;
        var textSpan = TextSpan.FromBounds(span.Start - offset, span.End);
        return Location.Create(syntaxNode.SyntaxTree, textSpan);
    }

    #endregion

    #region ITypeSymbol

    /// <summary>
    /// 返回指定类型的特性集合
    /// </summary>
    public static AttributeData[] GetTargetAttributes(this ISymbol typeSymbol, INamedTypeSymbol target) =>
        typeSymbol.GetAttributes()
        .Where(x => x.AttributeDataEquals(target)).ToArray();

    private static bool AttributeDataEquals(this AttributeData attributeData, INamedTypeSymbol target) =>
        attributeData.AttributeClass?
        .Equals(target, SymbolEqualityComparer.Default) ?? false;

    public static bool IsReferenceType(this TypedConstant typedConstant) => 
        (typedConstant.Value as ITypeSymbol)?.BaseType?.Name != "ValueType";

    public static string GetReferenceName(this TypedConstant typedConstant)
    {
        var value = typedConstant.Value!.ToString();
        return IsReferenceType(typedConstant) ? $"{value}?" : value;
    }

    #endregion

    #region AttributeData

    public static SyntaxNode GetSyntaxNode(this AttributeData data) =>
        data.ApplicationSyntaxReference!.GetSyntax();

    /// <summary>
    /// 返回特性中指定属性名的值
    /// </summary>
    /// <param name="target">属性名称</param>
    public static string? GetNamedArgumentValue(this AttributeData data, string target) =>
        data.NamedArguments.FirstOrDefault(x => x.Key == target).Value.Value?.ToString();

    /// <summary>
    /// 返回特性中指定参数位置的值
    /// </summary>
    /// <param name="target">属性名称</param>
    public static string? GetCtorArgumentValue(this AttributeData data, int index)
    {
        var args = data.ConstructorArguments;
        if (index + 1 > args.Length) return default;
        return data.ConstructorArguments[index].Value?.ToString();
    }

    #endregion

    #region Resource
    /// <summary>
    /// 返回指定名称的资源
    /// </summary>
    public static string GetResource(this Assembly assembly, string name)
    {
        var stream = assembly.GetManifestResourceStream(name);
        var sReader = new StreamReader(stream);
        return sReader.ReadToEnd();
    }
    #endregion

    #region 字符串

    /// <summary>
    /// 是否为空或无内容字符串
    /// </summary>
    public static bool IsEmpty(this string? v) => v is not { Length: > 0 };

    public static IEnumerable<string> GetLines(this string value)
    {
        using var sr = new StringReader(value);
        string line;
        while ((line = sr.ReadLine()) is not null)
        {
            yield return line;
        }
    }
    #endregion
}