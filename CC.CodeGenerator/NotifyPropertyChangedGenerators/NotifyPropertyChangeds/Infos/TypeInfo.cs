#nullable enable
using CC.CodeGenerator;
using CC.CodeGenerator.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.Infos;

internal class TypeInfo : MemberInfoBase<MemberDeclarationSyntax>
{
    public TypeInfo(MemberDeclarationSyntax member) : base(member) { }

    protected override SyntaxNode GetAttributeProvider() => Member;

    protected override void Run()
    {
        var type = GetTypeDefinition();

        //读取自定义的函数名
        ReadHandlerName(type);

        //规则检查
        if (!Rule(type)) return;

        //创建属性
        CreateProp(type);
    }


    #region 读取自定义函数名   
    private void ReadHandlerName(TypeShadowCode type)
    {
        var attr = AttributeDatas.LastOrDefault(IsHandlerName);
        if (attr is null) return;
        type.SetPropertyName(attr.GetNamedArgumentValue(handlerNames[0]));
        type.OnChangedName(attr.GetNamedArgumentValue(handlerNames[1]));
    }

    private bool IsHandlerName(AttributeData data) =>
       data.NamedArguments.Any(x => handlerNames.Contains(x.Key));

    #endregion

    #region 规则检查
    /// <summary>
    /// 规则1:意图不明确,如果需要创建属性, 必须提供PropertyType.
    /// </summary>
    private bool Rule(TypeShadowCode type)
    {
        var target = $"{attributeCtor}(string)";
        var errorNode = AttributeDatas
            .FirstOrDefault(x => x.AttributeConstructor?.ToDisplayString() == target);

        if (errorNode is not null)
        {
            var error = $"[{attributeName}] 意图不明确, 如果需要在类型上创建属性, 必须提供PropertyType参数.";
            return ReportDiagnostic("Type01", error,
                node: errorNode.GetSyntaxNode(),
                typeCode: type);
        }
        return true;
    }


    /// <summary>
    /// 规则1: 属性名至少包含一个字母。
    /// </summary>
    private bool Rule2(TypeShadowCode type, string? propName)
    {
        propName ??= "";
        if (GetPropName(propName) is { Length: > 0 }) return true;
        var error = "过滤字符{ '_' , '@' }后, 内容为空, 无法创建字段, 至少应包含一个字母.";
        ReportDiagnostic("Type02", error, node: Member, offset: 0, typeCode: type);
        return false;
    }
    #endregion

    #region 创建属性
    private void CreateProp(TypeShadowCode type)
    {
        var target = $"{attributeCtor}(string, System.Type)";
        AttributeDatas.Where(x => x.AttributeConstructor?.ToDisplayString() == target)
            .ToList().ForEach(x => CreateProp(type, x));
    }

    private void CreateProp(TypeShadowCode type, AttributeData data)
    {
        var propName = data.GetCtorArgumentValue(0);
        var memberType = data.ConstructorArguments[1].GetReferenceName();
        if (propName!.IsEmpty() || memberType!.IsEmpty()) return;
        //命名检查
        if (!Rule2(type, propName)) return;
        var error = type.AddProp(new()
        {
            FieldName = $"_{GetInitialLower(propName!)}",
            PropertyName = propName!,
            MemberType = memberType!,
            XmlSummary = data.GetNamedArgumentValue("XmlSummary"),
            IsCreateField = true,
        });
        TestNamingConflict(error, data);
    }

    #endregion
}