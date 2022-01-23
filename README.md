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

# 效果演示

![GIF 2022-1-21 13-44-18](https://user-images.githubusercontent.com/7581981/150472966-345d633e-4731-437b-9a8f-691b09133a7c.gif)

