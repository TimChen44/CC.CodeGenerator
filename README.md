# CC.CodeGenerator
利用.Net的Source Generator功能，生成开发过程中哪些无聊的代码。

# 使用方法

## 1. 安装

### 安装代码生成包以及支持包
```
Install-Package CC.CodeGenerator
```

```
Install-Package CC.NetCore
```

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
