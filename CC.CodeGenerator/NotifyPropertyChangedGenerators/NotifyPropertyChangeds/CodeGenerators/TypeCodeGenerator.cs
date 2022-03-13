#nullable enable
using Microsoft.CodeAnalysis.Text;

namespace CC.CodeGenerator.NotifyPropertyChangeds;

/// <summary>
/// 类型代码生成器
/// </summary>
internal class TypeShadowCode
{
    private readonly List<MemberShadowCode> members = new();
    private readonly INamedTypeSymbol typeDefinition;
    private readonly HashSet<string> names = new();
    private readonly List<string> errors = new();

    public TypeShadowCode(INamedTypeSymbol typeDefinition)
    {
        this.typeDefinition = typeDefinition;
        FillMemberNames();
    }

    public string SetPropertyMethodName { get; private set; } = "SetProperty";

    public string OnPropertyChangedMethodName { get; private set; } = "OnPropertyChanged";

    public void SetPropertyName(string? name)
    {
        if (Extends.IsEmpty(name)) return;
        SetPropertyMethodName = name!;
    }

    public void OnChangedName(string? name)
    {
        if (Extends.IsEmpty(name)) return;
        OnPropertyChangedMethodName = name!;
    }

    private void FillMemberNames() => typeDefinition
        .MemberNames.ToList().ForEach(x => names.Add(x));

    public void AddError(string error) => errors.Add(error);

    public string? AddProp(MemberShadowCode member)
    {
        //从特性创建 检查 属性 字段
        //从字段创建 检查 属性
        var res = Test(member.PropertyName, "属性");
        if (member.IsCheckFiledName) res ??= Test(member.FieldName, "字段");
        if (res is null) members.Add(member);
        return res;
    }

    private string? Test(string value, string msg)
    {
        if (names.Contains(value))
        {
            var res = $"准备生成的{msg}名:{value}, 已被占用!";
            errors.Add(res);
            return res;
        }

        names.Add(value);
        return default;
    }

    internal void CreateCode(GeneratorExecutionContext context)
    {
        var code =
            //添加空间名
            CreateNamespace()
            .AddError(errors)
            //添加类型
            .Add(CrateType)
            .Add(CreateHandler)

            .AddMember("#region props")
            //创建字段
            .Add(CreateFields)
            //创建属性
            .Add(CreateProps)
            .AddMember("#endregion");

        var name = typeDefinition.ToDisplayString().Replace("<", "{").Replace(">", "}");

        var csFile = $@"NotifyProp_{name}.cs";
        context.AddSource(csFile, SourceText.From(code.Build(), Encoding.UTF8));
    }

    /// <summary>
    /// 处理空间名
    /// </summary>
    private CodeBuilder CreateNamespace()
    {
        var res = new CodeBuilder();
        var value = typeDefinition
              .ToDisplayParts()
              .Where(x => x.Kind is SymbolDisplayPartKind.NamespaceName)
              .Select(x => x.ToString())
              .Aggregate((l, r) => $"{l}.{r}");
        return res.AddNamespace(value);
    }

    private CodeBuilder CrateType(CodeBuilder code)
    {
        var types = typeDefinition.ToDisplayParts().Where(x => x.Kind is
                     SymbolDisplayPartKind.ClassName or
                     SymbolDisplayPartKind.EnumName or
                     SymbolDisplayPartKind.InterfaceName or
                     SymbolDisplayPartKind.RecordClassName or
                     SymbolDisplayPartKind.RecordStructName or
                     SymbolDisplayPartKind.StructName).ToList();
        types.Remove(types.Last());
        types.ForEach(x => code.AddType(x.Symbol).TabPlus());
        return code.AddType(typeDefinition, "INotifyPropertyChanged");
    }

    //创建函数
    private CodeBuilder CreateHandler(CodeBuilder code)
    {
        return code
            .AddUsing("System.Collections.Generic")
            .AddUsing("System.ComponentModel")
            .AddUsing("System.Runtime.CompilerServices")
            .AddMember(@$"
#region ChangedHandler

public event PropertyChangedEventHandler? PropertyChanged;

protected virtual bool {SetPropertyMethodName}<T>(ref T storage, T value
    , [CallerMemberName] string? propertyName = null)
{{
    if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
    storage = value;
    {OnPropertyChangedMethodName}(new PropertyChangedEventArgs(propertyName));
    return true;
}}

protected virtual void {OnPropertyChangedMethodName}(PropertyChangedEventArgs args) =>
    PropertyChanged?.Invoke(this, args);

#endregion");
    }

    //创建字段
    private CodeBuilder CreateFields(CodeBuilder code)
    {
        if (errors.Count > 0) return code;
        members.Where(x => x.IsCreateField).ToList().ForEach(x => x.CreateField(code));
        return code;
    }

    //创建属性    
    private CodeBuilder CreateProps(CodeBuilder code)
    {
        if (errors.Count > 0) return code;
        members.ForEach(x => x.CreateProperty(code, SetPropertyMethodName));
        return code;
    }

}