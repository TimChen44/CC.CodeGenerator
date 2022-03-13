namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

/*             
 *             
 * 从类创建属性            
 *             
 */
[AddNotifyPropertyChanged("Id", typeof(long))]
[AddNotifyPropertyChanged("Name", typeof(string), XmlSummary = "名称")]
internal partial class Demo1
{

    //验证是否生成
    void _()
    {
        // 自动生的属性
        Id = 123;
        Name = "456";

        // 自动生成的方法和事件
        var __ = $"{nameof(SetProperty)}";
        var ___ = $"{nameof(OnPropertyChanged)}";
        var ____ = $"{nameof(PropertyChanged)}";
    }
}
