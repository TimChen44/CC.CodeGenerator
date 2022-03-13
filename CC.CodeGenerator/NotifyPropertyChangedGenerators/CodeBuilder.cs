#nullable enable
using Microsoft.CodeAnalysis.CSharp;

namespace CC.CodeGenerator;
internal class CodeBuilder
{
    private readonly HashSet<string> partialMatching = new()
    {
        "class",
        "record",
        "struct",
        "interface",
    };
    private readonly List<string> usings = new();
    private readonly Queue<string> queue = new();
    private readonly Stack<string> stack = new();
    private readonly List<string> errors = new();
    private string tabString = "";

    public CodeBuilder() { }

    public int TabCount { get; private set; }

    private void RefreshTabString() => tabString = new('\t', TabCount);

    public CodeBuilder AddLine()
    {
        AddQueueCode("");
        return this;
    }

    public CodeBuilder TabPlus()
    {
        TabCount += 1;
        RefreshTabString();
        return this;
    }

    public CodeBuilder TabMinus()
    {
        TabCount -= 1;
        RefreshTabString();
        return this;
    }

    private void AddQueueCode(string code) => queue.Enqueue(code);

    private void AddStackCode(string code) => stack.Push(code);

    private void SetEnd() => AddStackCode($"{tabString}}}");

    private void AppendLine(StringBuilder sb, IEnumerable<string> items)
    {
        foreach (var item in items) sb.AppendLine(item);
    }

    public string Build()
    {
        var sb = new StringBuilder();
        WriteError(sb);
        usings.ForEach(x => sb.AppendLine(x));
        AppendLine(sb, queue);
        AppendLine(sb, stack);
        var res = sb.ToString();
        return res;
    }

    public override string ToString() => Build();

    /// <summary>
    /// 添加Using
    /// </summary>
    public CodeBuilder AddUsing(string value)
    {
        usings.Add($"using {value};");
        return this;
    }

    /// <summary>
    /// 添加空间名
    /// </summary>
    public CodeBuilder AddNamespace(string value)
    {
        AddQueueCode($"{tabString}namespace {value}\r\n{tabString}{{");
        SetEnd();
        TabPlus();
        return this;
    }

    internal CodeBuilder AddError(IEnumerable<string> errors)
    {
        this.errors.AddRange(errors);
        return this;
    }

    public CodeBuilder AddType(ISymbol? symbol) =>
        symbol is INamedTypeSymbol namedTypeSymbol ? AddType(namedTypeSymbol) : this;

    public CodeBuilder AddType(INamedTypeSymbol symbol, params string[] inherits)
    {
        var sb = new StringBuilder();
        WriteType(symbol, sb);
        WriteTypeParameters(symbol, sb);
        WriteInherit(sb, inherits);
        AddQueueCode($"{tabString}{sb}\r\n{tabString}{{");
        SetEnd();
        return this;
    }

    private void WriteType(INamedTypeSymbol symbol, StringBuilder sb)
    {
        var keys = GetKeywords(symbol);
        var isPartial = keys.Any(x => x == "partial");
        foreach (var item in keys)
        {
            if (partialMatching.Contains(item) && !isPartial)
                sb.Append("partial ");
            sb.Append(item).Append(' ');
        }
        sb.Append(symbol.Name);
    }

    private void WriteTypeParameters(INamedTypeSymbol symbol, StringBuilder sb)
    {
        if (symbol.TypeParameters.Length is 0) return;
        sb.Append("<");
        var items = symbol.TypeParameters.Select(x => x.Name);
        sb.Append(string.Join(", ", items));
        sb.Append(">");
    }

    private void WriteInherit(StringBuilder sb, string[] inherits)
    {
        if (inherits is not { Length: > 0 }) return;
        sb.Append(" : ");
        sb.Append(string.Join(", ", inherits));
    }

    private string[] GetKeywords(INamedTypeSymbol symbol) => symbol
        .DeclaringSyntaxReferences[0]
        .GetSyntax()
        .ChildTokens()
        .Where(x => x.IsKeyword())
        .Select(x => x.ToString())
        .ToArray();

    private void WriteError(StringBuilder sb)
    {    
        if (errors.Count is 0) return;
        sb.AppendLine("// 生成时发生错误 !!!!!!!!!!!!!!!");
        errors.SelectMany(x => x.GetLines()).ToList().ForEach(x =>
        {
            if (x.Length > 0) sb.Append("// ");
            sb.AppendLine(x);
        });
    }

    public CodeBuilder Add(Func<CodeBuilder, CodeBuilder> func) => func(this);

    /// <summary>
    /// 添加成员代码(并且追加一行)
    /// </summary>
    public CodeBuilder AddMember(string code, bool addLine = true)
    {
        TabPlus();
        AddQueue(code);
        if (addLine) AddLine();
        TabMinus();
        return this;
    }

    private void AddQueue(string code)
    {
        foreach (var item in code.GetLines())
        {
            if (item.Length > 0) AddQueueCode($"{tabString}{item}");
            else AddQueueCode(item);
        }
    }
}