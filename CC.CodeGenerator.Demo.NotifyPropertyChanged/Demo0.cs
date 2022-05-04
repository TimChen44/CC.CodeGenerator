namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

[Dto]
[AddNotifyPropertyChanged("Id", typeof(long), XmlSummary = "从类上创建属性")]
partial class Demo0
{
    [AddNotifyPropertyChanged(XmlSummary = "从字段创建属性")]
    private string _name;
}
