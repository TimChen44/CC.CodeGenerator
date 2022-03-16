#pragma warning disable CS8632
using CC.CodeGenerato.NotifyPropertyChangeds.CodeBuilds;
using CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;

namespace CC.CodeGenerato.NotifyPropertyChangeds;
internal partial class NotifyPropertyCodeBuildManager : ICodeBuilder
{
    private string? _onPropertyChangedMethodName;
    private string? _setPropertyMethodName;

    /// <summary>
    /// 成员名称集合
    /// </summary>
    private readonly Dictionary<string, Location?> members = new();

    #region 待处理的成员
    public List<TargetValidationBase> TargetValidations { get; } = new();

    public NotifyPropertyCodeBuildManager AddMember(TargetValidationBase member)
    {
        TargetValidations.Add(member);
        return this;
    }

    #endregion

    #region 自定义函数名称

    public string OnPropertyChangedMethodName
    {
        get => TestNullOrEmpty(_onPropertyChangedMethodName, "OnPropertyChanged");
        set => _onPropertyChangedMethodName = value;
    }

    public string SetPropertyMethodName
    {
        get => TestNullOrEmpty(_setPropertyMethodName, "SetProperty");
        set => _setPropertyMethodName = value;
    }


    private string TestNullOrEmpty(string? value, string defaultValue)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value!;
    }

    #endregion

    public void Build()
    {
        var first = TargetValidations.First();

        //初始化现有的成员名称集合
        InitMemberName(first.ContainingType);

        //提取要创建的属性
        var buildItems = TargetValidations
            .SelectMany(x => x.CreateCodeBuilders())
            .Select(Test) //检查单项代码
            .ToList();

        //获取自定义函数名称
        SetHanderName(buildItems);

        //执行构建代码
        CreateCode(buildItems);
    }

    //检查单项代码
    private CodeBuilderBase Test(CodeBuilderBase codeBuilder)
    {
        //检测命名冲突
        codeBuilder.TestName(members);

        //检测调用规则
        codeBuilder.TestRule();

        return codeBuilder;
    }

    //获取自定义函数名称
    private void SetHanderName(IEnumerable<CodeBuilderBase> buildItems)
    {
        var items = buildItems.OfType<TypeCodeBuilder>().ToArray();
        SetPropertyMethodName = Find(x => x.SetPropertyMethodName)!;
        OnPropertyChangedMethodName = Find(x => x.OnPropertyChangedMethodName)!;

        string? Find(Func<TypeCodeBuilder, string?> getItem) =>
            items.Select(getItem).OfType<string>().LastOrDefault();
    }


    /// <summary>
    /// 初始化现有的成员名称
    /// </summary>
    private void InitMemberName(INamedTypeSymbol containingType)
    {
        foreach (var item in containingType.GetMembers())
            members[item.Name] = item.Locations.First();
    }

    private void CreateCode(List<CodeBuilderBase> buildItems)
    {
        var type = TargetValidations.First();

        var codeBuilder = new CodeBuilder()
            .AddUsing("System.Collections.Generic")
            .AddUsing("System.ComponentModel")
            .AddUsing("System.Runtime.CompilerServices")
            .AddTypeTree(type.ContainingType, "INotifyPropertyChanged")
            .AddCode(GetHandlerCode());
        buildItems.ForEach(x => x.CreateCode(codeBuilder, SetPropertyMethodName));

        var fileName = type.ContainingType.ToDisplayString().Replace("<", "{").Replace(">", "}");
        fileName = $"pc_{fileName}";


        type.NodeData.Context.AddSource(fileName, codeBuilder.ToString());

    }

    private string GetHandlerCode() => @$"
#region ChangedHandler

public event PropertyChangedEventHandler? PropertyChanged;

private bool {SetPropertyMethodName}<T>(ref T storage, T value , [CallerMemberName] string? propertyName = null)
{{
    if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
    storage = value;
    {OnPropertyChangedMethodName}(propertyName);
    return true;
}}

private void {OnPropertyChangedMethodName}(string? propertyName) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

#endregion";
}