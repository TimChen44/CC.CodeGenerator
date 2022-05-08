# CC.CodeGenerator
利用.Net的Source Generator功能，生成开发过程中哪些无聊的代码。

### 优势
- 代码实时生成，无需额外操作，立即生效
- 开发过程中生成的代码，执行性能比运行时反射有更高的效率

### 效果演示

![GIF 2022-1-21 13-44-18](https://user-images.githubusercontent.com/7581981/150472966-345d633e-4731-437b-9a8f-691b09133a7c.gif)


# 使用方法

## 1. 安装

### 安装代码生成包以及支持包
```powershell
Install-Package CC.CodeGenerator
Install-Package CC.NetCore
```

> 因为VS的原因，在添加包引用或者升级包版本后，建议重启VS。

### Program.cs中添加全局引用
```csharp
global using CC.CodeGenerator;
```

## 2. 对象Mapping

Mapping
> 启用对象映射，可指定多个映射对象

MappingIgnore
> 忽略不需要的属性

```csharp
[Mapping(typeof(People2Map), typeof(People3Map))]
public partial class People1Map
{
    public Guid PeopleId { get; set; }
    public string UserName { get; set; }
    public string City { get; set; }
    [MappingIgnore]
    public string Disply => $"{UserName}";
}

public class People2Map
{
    public Guid PeopleId { get; set; }
    public string UserName { get; set; }
}

public class People3Map
{
    public string City { get; set; }

}
```

进行对象之间的赋值

```csharp
//初始新的Dto
var people1Map = new People1Map() { PeopleId=Guid.NewGuid(),UserName="Tim" };
var people2Map = new People2Map();
var people3Map = new People3Map() { City="ShangHai"};

//复制到对象
people1Map.CopyTo(people2Map);

//从对象复制来
people1Map.CopyFrom(people3Map);
```

## 3. 服务注册代码自动创建

Service
> 自动创建服务注册代码，让Program更加清洁
> - LifeCycle:自定义生命周期，默认Scoped

### Program.cs中标记注册位置
```csharp
var builder = WebApplication.CreateBuilder(args);

CC.CodeGenerator.AutoDI.AddServices(builder);//加入此行代码
```

### 服务中增加ServiceAttribute

```csharp
[Service(LifeCycle = ELifeCycle.Singleton)]
public class WeatherForecastService
```

## 4. 增强数据交换对象及简化EF

Dto
> 增强实体特性，提供对象赋值，默认增删改查代码
> - Context:上下文对象
> - Entity:映射的EF实体

Ignore
> 忽略不需要的属性

### 示例

```csharp
[Dto(Context=nameof(DemoaContext),Entity =typeof(People))]
public partial record PeopleDto
{
    public Guid PeopleId { get; set; }
    public string UserName { get; set; }
    public string City { get; set; }
    [DtoIgnore]
    public string Disply => $"{UserName}";
}
```

### 示例

```csharp
var context = new DemoaContext();

//创建Dto
var secondDto = new PeopleDto() { City = "ShangHai" };

//初始新的Dto
var firstDto = PeopleDto.NewGen();

//快速从Dto复制
firstDto.CopyFormDto(secondDto);

//EF快速Select
var peopleEntityDtos = context.People.Where(x=>x.City == "ShangHai").ToPeopleDtos();

//快速载入Dto
var peopleEntityDto = PeopleDto.LoadGen(context, new Guid(""));

//Dto复制到实体
var peopleEntity = context.People.FirstOrDefault();
peopleEntityDto.CopyToEntity(peopleEntity);

//Dto重新载入
peopleEntityDto.ReLoadGen(context);

//Dto快速保存
peopleEntityDto.City = "北京";
peopleEntityDto.SaveGen(context);

//Dto快速删除
peopleEntityDto.DeleteGen(context);

//最后保存操作
context.SaveChanges();
```

## 5. 自动实现INotifyPropertyChanged接口

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
