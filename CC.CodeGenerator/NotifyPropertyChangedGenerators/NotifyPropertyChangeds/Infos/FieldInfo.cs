#nullable enable
using CC.CodeGenerator;
using CC.CodeGenerator.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.Infos;

internal class FieldInfo : MemberInfoBase<FieldDeclarationSyntax>
{
    public FieldInfo(FieldDeclarationSyntax member) : base(member) { }

    public int FiledCount => Member.Declaration.Variables.Count;

    protected override SyntaxNode GetAttributeProvider() => Member.Declaration.Variables[0];

    /// <summary>
    /// 返回字段集合
    /// </summary>
    public IEnumerable<IFieldSymbol> GetFieldSymbols() => Member.Declaration.Variables
            .Select(x => x.GetDeclaredSymbol(Compilation))
            .Where(x => x is not null)
            .Cast<IFieldSymbol>()
            .ToList();

    protected override void Run()
    {
        var type = GetTypeDefinition();

        //规则检查
        if (!Rule(type)) return;

        //创建属性
        var attr = AttributeDatas[0];
        GetFieldSymbols()
            .Select((x, i) => (value: x, index: i))
            .ToList().ForEach(x => CreateProp(type, attr, x.value, x.index));
    }

    #region 检查规则

    private bool Rule(TypeShadowCode type)
    {
        var runs = new[] { Rule1, Rule2, Rule3, Rule4 };
        foreach (var item in runs)
        {
            if (!item(type)) return false;
        }
        return true;
    }

    /// <summary>
    /// 字段规则1:特性在字段上只能标记一次, 多次标记会导致歧义.
    /// </summary>
    private bool Rule1(TypeShadowCode type)
    {
        if (AttributeDatas.Length > 1)
        {
            var field = AttributeDatas.Last().GetSyntaxNode();
            var error = $"[{attributeName}] 特性在字段上只能标记一次, 多次标记会导致歧义.";
            return ReportDiagnostic("Filed01", error, node: field, typeCode: type);
        }
        return true;
    }

    /// <summary>
    /// 字段规则2:特性同时标记多个字段时，无法自定义属性名称和类型.
    /// </summary>
    private bool Rule2(TypeShadowCode type)
    {
        if (FiledCount is 1) return true;
        var attr = AttributeDatas.First();
        var target = $"{attributeCtor}(string";
        var ctor = attr.AttributeConstructor?.ToString() ?? "";
        if (ctor.StartsWith(target))
        {
            var error = $"{attributeName} 特性同时标记多个字段时，无法自定义属性的“名称”和“类型”.";
            var field = attr.GetSyntaxNode();
            return ReportDiagnostic("Filed02", error, node: field, typeCode: type);
        }
        return true;
    }

    /// <summary>
    /// 字段规则3:在字段上自定义属性类型无效.
    /// </summary>
    private bool Rule3(TypeShadowCode type)
    {
        var attr = AttributeDatas[0];
        var target = $"{attributeCtor}(string, System.Type)";
        var ctor = attr.AttributeConstructor?.ToString() ?? "";
        if (ctor.StartsWith(target))
        {
            var error = $"{attributeName} 在字段上无法自定义属性类型.";
            var field = attr.GetSyntaxNode();
            return ReportDiagnostic("Filed03", error, node: field, typeCode: type);
        }
        return true;
    }

    /// <summary>
    /// 字段规则4:在字段上自定义MethodName无效.SetPropertyMethodName 和 OnPropertyChangedMethodName 只能用在类型上.
    /// </summary>
    private bool Rule4(TypeShadowCode type)
    {
        var attr = AttributeDatas[0];
        var namedArgument = attr.NamedArguments.FirstOrDefault(x => handlerNames.Contains(x.Key)).Key;
        if (namedArgument is { Length: > 0 })
        {
            var error = $"{namedArgument} 只能在类型上使用.";
            var field = attr.GetSyntaxNode();
            return ReportDiagnostic("Filed04", error, node: field, typeCode: type);
        }
        return true;
    }

    /// <summary>
    ///  字段规则5: 字段命名不复合要求.
    /// </summary>
    /// <returns></returns>
    private bool Rule5(TypeShadowCode type, string? propName, int index)
    {
        if (propName is not null) return true;
        var error = "过滤字符{ '_' , '@' }后, 内容为空, 无法创建属性, 至少应包含一个字母.";
        var node = Member.Declaration.Variables[index];
        ReportDiagnostic("Filed05", error, node: node, offset: 0, typeCode: type);
        return false;
    }
    #endregion

    #region 创建属性
    private void CreateProp(TypeShadowCode type, AttributeData attr
        , IFieldSymbol fieldSymbol, int index)
    {
        var fieldName = fieldSymbol.Name;
        var propName = GetPropName(attr.GetCtorArgumentValue(0) ?? fieldName);

        //检查命名
        if (!Rule5(type, propName, index)) return;
        var xmldoc = attr.GetNamedArgumentValue("XmlSummary");

        var error = type.AddProp(new()
        {
            FieldName = fieldName,
            MemberType = fieldSymbol.Type.ToString(),
            PropertyName = GetInitialLower(propName!, false),
            Source = attr,
            XmlSummary = xmldoc,
            //从字段创建时，不需要检查字段的命名重复！
            IsCheckFiledName = false
        });

        TestNamingConflict(error, attr);
    }
    #endregion
}