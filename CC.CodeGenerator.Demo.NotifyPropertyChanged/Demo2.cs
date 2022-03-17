namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

/*
 * 
 *  从字段创建属性
 * 
 */
internal partial class Demo2
{
    [AddNotifyPropertyChanged]
    private long _id;

    [AddNotifyPropertyChanged("Name", XmlSummary = "XML文档内容")]
    private string? _xxxxName;

    [AddNotifyPropertyChanged]
    private int _pid, _sid;

    //验证是否生成
    void _()
    {
        // 自动生的属性
        Id = 123;
        Name = "456";
        Pid = 123;
        Sid = 456;

        // 自动生成的方法和事件
        var __ = $"{nameof(SetProperty)}";
        var ___ = $"{nameof(OnPropertyChanged)}";
        var ____ = $"{nameof(PropertyChanged)}";
    }
}
