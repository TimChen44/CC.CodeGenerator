#pragma warning disable CS8632 
using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using CC.CodeGenerator.NotifyPropertyChangeds.CodeBuilds;
using CC.CodeGenerator.NotifyPropertyChangeds.Nodes;
namespace CC.CodeGenerator.NotifyPropertyChangedGenerators.NotifyPropertyChangeds;
public partial class NotifyPropCodeBuildManager : CodeBuildManagerBase<NotifyPropNodeBase>
{
    private string? _onPropertyChangedMethodName;
    private string? _setPropertyMethodName;

    /// <summary>
    /// 成员名称集合
    /// </summary>
    private readonly Dictionary<string, Location?> members = new();


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

    public override void Build()
    {
        var first = Items.First();

        //初始化现有的成员名称集合
        InitMemberName(first.TargetData.ContainingType);

        //提取要创建的属性
        var buildItems = Items
            .SelectMany(x => x.CreateCodeBuilders())
            .Select(Test) //检查单项代码
            .ToList();

        //获取自定义函数名称
        SetHanderName(buildItems);

        //执行构建代码
        CreateCode(buildItems);
    }

    //检查单项代码
    private NotifyPropCodeBuilderBase Test(NotifyPropCodeBuilderBase codeBuilder)
    {
        //检测命名冲突
        codeBuilder.TestName(members);

        //检测调用规则
        codeBuilder.TestRule();

        return codeBuilder;
    }

    //获取自定义函数名称
    private void SetHanderName(IEnumerable<NotifyPropCodeBuilderBase> buildItems)
    {
        var items = buildItems.OfType<NotifyPropTypeCodeBuilder>().ToArray();
        SetPropertyMethodName = Find(x => x.SetPropertyMethodName)!;
        OnPropertyChangedMethodName = Find(x => x.OnPropertyChangedMethodName)!;

        string? Find(Func<NotifyPropTypeCodeBuilder, string?> getItem) =>
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

    private void CreateCode(List<NotifyPropCodeBuilderBase> buildItems)
    {
        var type = Items.First();
        var containingType = type.TargetData.ContainingType;
        var builds = buildItems.Where(x => x.IsBuild()).Select(x => x.PropertyName).ToArray();
        var props = string.Join(", ", builds);

        var codeBuilder = new CodeBuilder()
            .AddUsing("System.Collections.Generic")
            .AddUsing("System.ComponentModel")
            .AddUsing("System.Runtime.CompilerServices")
            .AddTypeTree(containingType, "INotifyPropertyChanged")
            .AddCode(GetHandlerCode())
            .AddLine()
            .AddLine()
            .AddLine($"// 生成 [{props}] {builds.Length}个属性");

        buildItems.ForEach(x => x.CreateCode(codeBuilder, SetPropertyMethodName));

        var fileName = containingType.ToDisplayString().Replace("<", "{").Replace(">", "}");
        type.ContextData.Context.AddSource(fileName, codeBuilder.ToString());
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
