#pragma warning disable CS8632
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Reflection;

namespace CC.CodeGenerato;

internal static class Extends
{

    #region SyntaxNode

    /// <summary>
    /// 获取与声明关联的符号
    /// </summary>
    public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation) =>
               compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

    /// <summary>
    /// 语法引用的符号(用于调用表达式)
    /// </summary>
    public static ISymbol? GetSymbol(this SyntaxNode node, Compilation compilation) =>
        compilation.GetSemanticModel(node.SyntaxTree).GetSymbolInfo(node).Symbol;

    /// <summary>
    /// 语法引用的符号(用于调用表达式)
    /// </summary>
    public static T? GetSymbol<T>(this SyntaxNode node, Compilation compilation)
        where T : class, ISymbol => node.GetSymbol(compilation) as T;

    /// <summary>
    /// 生成、条件编译符号是否存在
    /// </summary>
    public static string? FindPreprocessorSymbol(this SyntaxNode node, Func<string, bool> where) =>
        node.SyntaxTree.Options
        .PreprocessorSymbolNames
        .FirstOrDefault(where);

    #endregion

    #region GeneratorExecutionContext

    public static bool ReportError(this GeneratorExecutionContext context, DiagnosticData data, bool result = false)
    {
        data.ReportDiagnostic(context);
        return result;
    }


    public static SyntaxTree AddSourceAndParseText(this GeneratorExecutionContext context
        , string fileName, string code)
    {
        var sourceText = SourceText.From(code, Encoding.UTF8);
        context.AddSource(fileName, sourceText);
        return CSharpSyntaxTree.ParseText(sourceText);
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

    public static string GetTypeName(this ISymbol symbol)
    {
        var type = (ITypeSymbol)symbol;
        var res = type.ToString();
        return res.EndsWith("?") || type.IsValueType ? res : $"{res}?";
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