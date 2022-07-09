#pragma warning disable CS8632 
using Microsoft.CodeAnalysis.CSharp;

namespace CC.CodeGenerator.NotifyPropertyChangedGenerators;
public class CodeBuilder
{
    private readonly HashSet<string> partialMatching = new() { "class", "record", "struct", "interface", };
    private readonly List<string> usings = new();
    private readonly List<string?> starts = new();
    private readonly Stack<string> ends = new();
    private string tabString = "";
    private int _tabCount = 0;

    public int TabCount
    {
        get => _tabCount;
        private set
        {
            if (_tabCount == value) return;
            _tabCount = value;
            tabString = new('\t', _tabCount);
        }
    }

    /// <summary>
    /// 添加Using
    /// </summary>
    public CodeBuilder AddUsing(string value)
    {
        usings.Add($"using {value};");
        return this;
    }

    /// <summary>
    /// 添加一个缩进
    /// </summary>
    public CodeBuilder AddTab(Action<CodeBuilder> children)
    {
        TabCount += 1;
        children(this);
        TabCount -= 1;
        return this; ;
    }

    /// <summary>
    /// 添加代码(分行:{缩进}{insertValue}{行内容})
    /// </summary>
    /// <param name="code">代码内容，自动分行</param>
    /// <param name="insertValue">在每行前面插入的内容</param>
    public CodeBuilder AddCode(string? code, string insertValue = "")
    {
        if (code != null)
        {
            foreach (var item in code.GetLines())
                starts.Add($"{tabString}{insertValue}{item}");
        }
        return this;
    }

    /// <summary>
    /// 仅用于添加单行
    /// </summary>
    public CodeBuilder AddLine(string? value = null)
    {
        starts.Add($"{tabString}{value}");
        return this;
    }

    public CodeBuilder AddScope(string? s = "{", string? e = "}", bool addTab = true)
    {
        starts.Add($"{tabString}{s}");
        ends.Push($"{tabString}{e}");
        if (addTab) TabCount += 1;
        return this;
    }

    /// <summary>
    /// 插入类型树
    /// </summary>
    public CodeBuilder AddTypeTree(INamedTypeSymbol typeSymbol, params string[] inherits)
    {
        var (namespaces, types) = GetNodes(typeSymbol);
        var last = types.Last().Symbol!.ToString();
        var namespaceStr = string.Join(".", namespaces.Select(x => x.ToString()));
        AddCode($"namespace {namespaceStr}").AddScope();

        foreach (var item in types)
        {
            var key = item.Symbol!.ToString();
            var keyword = GetKeyword(item);
            var className = key.Split('.').Last();
            var inherit = GetInherit(() => key == last, inherits);

            starts.Add($"{tabString}{keyword} {className} {inherit}");
            AddScope();
        }
        return this;
    }

    private string GetInherit(Func<bool> where, string[] inherits)
    {
        if (!where()) return "";
        return $" : {string.Join(", ", inherits)}";
    }

    private string GetKeyword(SymbolDisplayPart symbol)
    {
        var items = symbol.Symbol!
              .DeclaringSyntaxReferences[0]
              .GetSyntax()
              .ChildTokens()
              .Where(x => x.IsKeyword())
              .Select(x => x.ToString())
              .ToList();
        if (!items.Any(x => x == "partial"))
            items = GetKeywords(ref items);
        return string.Join(" ", items);
    }

    private List<string> GetKeywords(ref List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (partialMatching.Contains(list[i]))
            {
                list[i] = $"partial {list[i]}";
                return list;
            }
        }
        list.Insert(0, "partial");
        return list;
    }

    private (SymbolDisplayPart[] namespaces, SymbolDisplayPart[] types) GetNodes(INamedTypeSymbol typeSymbol)
    {
        var namespaces = new List<SymbolDisplayPart>();
        var types = new List<SymbolDisplayPart>();
        foreach (var node in typeSymbol.ToDisplayParts())
        {
            switch (node.Kind)
            {
                case SymbolDisplayPartKind.NamespaceName:
                    namespaces.Add(node);
                    break;

                case SymbolDisplayPartKind.ClassName:
                case SymbolDisplayPartKind.InterfaceName:
                case SymbolDisplayPartKind.RecordClassName:
                case SymbolDisplayPartKind.RecordStructName:
                case SymbolDisplayPartKind.StructName:
                    types.Add(node);
                    break;
                default:
                    break;
            }
        }
        return (namespaces.ToArray(), types.ToArray());
    }

    public override string ToString() => GetCode();

    public string GetCode()
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetDisableWarnings());
        usings.ForEach(x => sb.AppendLine($"using {x};".Replace(";;",";").Replace("using using ", "using ")));
        starts.ForEach(x => sb.AppendLine(x));
        ends.ToList().ForEach(x => sb.AppendLine(x));
        return sb.ToString();
    }

    private string GetDisableWarnings() => @$"// 生成 : {DateTime.Now}
#region warnings
#pragma warning disable IDE0079
#pragma warning disable IDE0090
#pragma warning disable IDE0044
#pragma warning disable IDE0051
#pragma warning disable IDE0060
#pragma warning disable CS8618
#pragma warning disable CS8612
#pragma warning disable CS8625
#pragma warning disable CS8632
#pragma warning disable CS8603
#pragma warning disable CS8601
#pragma warning disable CA1822
#pragma warning disable CS0169
#pragma warning disable CS0414
#endregion
";
}