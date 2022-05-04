namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

/*
 * 
 *  修改自动生成的方法名称
 * 
 */

[AddNotifyPropertyChanged(SetPropertyMethodName ="SetXX", OnPropertyChangedMethodName ="OnXX")]
[AddNotifyPropertyChanged("Id", typeof(int))]
public partial class Demo3
{
    [AddNotifyPropertyChanged]
    private string? _name = "";


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
        var __ = $"{nameof(SetXX)}";
        var ___ = $"{nameof(OnXX)}";
        var ____ = $"{nameof(PropertyChanged)}";
    }
}
