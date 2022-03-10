using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

[AddNotifyPropertyChanged]
internal partial class Demo
{
    //使用“代码片段”(propchanged.snippet)生成
    public int MyProperty { get => _MyProperty; set => SetProperty(ref _MyProperty, value); }
    private int _MyProperty;

}
