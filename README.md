# CC.CodeGenerator
利用.Net的Source Generator功能，生成开发过程中哪些无聊的代码。

# 使用方法

## 1. 安装

### 安装代码生成包以及支持包
```powershell
Install-Package CC.CodeGenerator
```

```powershell
Install-Package CC.NetCore
```

> 因为VS的原因，在添加包引用或者升级包版本后，建议重启VS。

### Program.cs中添加全局引用
```csharp
global using CC.CodeGenerator;
```

## 2. 对象Mapping

### 创建Class或record文件，并加入Dto特性

Ignore:忽略不需要的属性
```csharp
[Dto()]
public partial class PeopleEditDto
{
    public Guid PeopleId { get; set; }

    public string UserName { get; set; }

    public string City { get; set; }

    [Ignore]
    public string Display => $"{UserName} {City}";
}
```

### 效果演示

![GIF 2022-1-21 13-44-18](https://user-images.githubusercontent.com/7581981/150472966-345d633e-4731-437b-9a8f-691b09133a7c.gif)


## 3. 支持EF简化单表操作

### Program.cs中加入EF实体引用
```csharp
global using CC.CodeGenerator.Demo.Entity;
```

### 特性中增加参数
Context:上下文对象

Entity:映射的EF实体
```csharp
[Dto(Context =nameof(DemoaContext),Entity =typeof(People))]
public partial class PeopleEditDto
```

### 查询和保存示例

```csharp
var context=new DemoaContext();
var peoples= PeopleEditDto.SelectGen(context.People.Where(x => x.UserName.StartsWith("Latanya"))).ToList();

var me= peoples.FirstOrDefault();
me.City = "上海";
me.SaveGen(context);
context.SaveChanges();
```

## 3. 服务注册代码自动创建

### Program.cs中标记注册位置
```csharp
var builder = WebApplication.CreateBuilder(args);

CC.CodeGenerator.AutoDI.AddServices(builder);//加入此行代码
```

### 服务中增加ServiceAttribute
LifeCycle:自定义生命周期，默认Scoped
```csharp
[Service(LifeCycle = ELifeCycle.Singleton)]
public class WeatherForecastService
```

## 4. 自动实现INotifyPropertyChanged接口

```csharp
[AddNotifyPropertyChanged("Id", typeof(long), XmlSummary = "从类上创建属性")]
partial class Demo0
{
    [AddNotifyPropertyChanged(XmlSummary = "从字段创建属性")]
    private string _name;
}
```
生成代码如下
```csharp
partial class Demo0  : INotifyPropertyChanged
{
    
    #region 接口相关
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private bool SetProperty<T>(ref T storage, T value , [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    #endregion
    
    #region 生成的属性和字段

    private long _id;
    /// <summary>
    /// 从类上创建属性
    /// </summary>
    public long Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    /// <summary>
    /// 从字段创建属性
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    #endregion
}
```
