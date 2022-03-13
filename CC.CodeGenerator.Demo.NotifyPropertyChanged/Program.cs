global using CC.CodeGenerator;
using CC.CodeGenerator.Demo.NotifyPropertyChanged;
using System.ComponentModel;

var test = new Demo1() ;
((INotifyPropertyChanged)test).PropertyChanged += (s, e) => 
    Console.WriteLine($"{e.PropertyName}被修改");
test.Id = 123;

Console.WriteLine("完成");