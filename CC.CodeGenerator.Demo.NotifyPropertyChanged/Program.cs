using CC.CodeGenerator.Demo.NotifyPropertyChanged;

var data = new Demo();
data.PropertyChanged += (s, e) => Console.WriteLine($"属性{e.PropertyName}被修改");
data.MyProperty = 456;