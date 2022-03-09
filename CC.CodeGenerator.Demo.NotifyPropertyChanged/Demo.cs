using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Demo.NotifyPropertyChanged;

[AddNotifyPropertyChanged]
internal partial class Demo
{
    public int MyProperty { get => _MyProperty; set => SetProperty(ref _MyProperty, value); }
    private int _MyProperty;

}
