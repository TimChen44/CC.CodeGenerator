namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

/*      
 *      使用 "代码片段" 创建属性
 *      参考项目中的 propchanged.snippet 文件
*/

[AddNotifyPropertyChanged]
public partial class Demo4
{
    //"代码片段"创建
    public int MyProperty { get => _MyProperty; set => SetProperty(ref _MyProperty, value); }
    private int _MyProperty;

}
