# CC.CodeGenerator
利用.Net的Source Generator功能，生成开发过程中哪些无聊的代码。

### 优势
- 代码实时生成，无需额外操作，立即生效
- 开发过程中生成的代码，执行性能比运行时反射更有效率


### 视频介绍

https://www.bilibili.com/video/BV1Am4y1S7QT?share_source=copy_web
https://www.bilibili.com/video/BV1a3411Y7xb?share_source=copy_web
https://www.bilibili.com/video/BV14m4y1S7Ne?share_source=copy_web

# 使用方法

### 安装代码生成包以及支持包
```powershell
Install-Package CC.CodeGenerator
Install-Package CC.NetCore
```

> 因为VS的原因，在添加包引用或者升级包版本后，建议重启VS。

### Program.cs中添加全局引用
```csharp
global using CC.CodeGenerator;
global CC.Core
```

# 功能介绍

## EF功能增强

### 多表级联查询

> 以下三个单条方法功能和性能是等效的

```csharp
/// <summary>
/// 级联查询(原始写法)
/// </summary>
public void SelectStandardOrigin()
{
    var context = new DemoContext();

    var personne = context.Personnel.Where(x => x.PersonnelId == new Guid("92c26f2e-1dda-4c0b-9279-a4a66560d4be"))
        .Select(x => new PersonnelDto()
        {
            PersonnelId = x.PersonnelId,
            CompanyId = x.CompanyId,
            Name = x.Name,
            Gender = x.Gender,
            Birthday = x.Birthday,
            IsJob = x.IsJob,

            CompanyDto = new CompanyDto()
            {
                CompanyId = x.Company.CompanyId,
                Title = x.Company.Title,
                Address = x.Company.Address,
            },

            AchievementsDtos = x.Achievements.Select(x => new AchievementsDto()
            {
                AchievementsId = x.AchievementsId,
                PersonnelId = x.PersonnelId,
                Year = x.Year,
                Level = x.Level,
            }).ToList(),
        }).FirstOrDefault();
}

/// <summary>
/// 级联查询(精简写法)
/// </summary>
public void SelectStandardSingle()
{
    var context = new DemoContext();

    var personne = context.Personnel.Where(x => x.PersonnelId == new Guid("92c26f2e-1dda-4c0b-9279-a4a66560d4be"))
            .Select(x => new PersonnelDto(x)
            {
                CompanyDto = x.Company.ToCompanyDto(),
                AchievementsDtos = x.Achievements.ToAchievementsDtos(),
            }).FirstOrDefault();
}

/// <summary>
/// 级联查询(极简写法)
/// </summary>
public void SelectEasySingle()
{
    var context = new DemoContext();

    var personne = PersonnelDto.LoadGen(context, new Guid("92c26f2e-1dda-4c0b-9279-a4a66560d4be"));
}
```

> 检索数据直接输出多表数据

```csharp
/// <summary>
/// 级联查询多条
/// </summary>
public void SelectList()
{
    var context = new DemoContext();

    var personnes = context.Personnel
        .Where(x => x.IsJob == true && (x.Company.Address == "上海" || x.Company.Address == "北京"))
        .ToPersonnelDtos().ToList();
}
```

### 多表级联添加、修改、删除

```csharp
/// <summary>
/// 级联操作
/// </summary>
public void CascadeSave()
{
    var context = new DemoContext();

    //模拟前端提交了新增或编辑的Dto
    var personnel = CreatePersonnelDto();
    personnel.SaveGen(context);
    var saveResult = context.SaveChanges();

    //模拟修改了Dto进行保存
    personnel.IsJob = false;
    personnel.CompanyDto.Title = "公司改名";//编辑禁用时不会保存到数据库
    personnel.CompanyDto.Address = "北京";
    personnel.AchievementsDtos.Remove(personnel.AchievementsDtos.First(x => x.Year == 2020));
    personnel.AchievementsDtos.Add(new AchievementsDto()
    {
        AchievementsId = Guid.NewGuid(),
        PersonnelId = personnel.PersonnelId,
        Year = 2022,
        Level = "S_CascadeSave",
    });
    personnel.SaveGen(context);
    var updateResult = context.SaveChanges();

    //模拟删除Dto
    personnel.DeleteGen(context);
    var deleteResult = context.SaveChanges();
}


private PersonnelDto CreatePersonnelDto()
{
    var personnelId = new Guid("10000101-db1e-40dc-8ef4-65e95ff5698f");
    var personnel = new PersonnelDto()
    {
        PersonnelId = personnelId,
        Name = "超人_CascadeSave",
        Gender = "男",
        IsJob = true,
        Birthday = DateTime.Now,

        CompanyDto = new CompanyDto()
        {
            CompanyId = new Guid("10000201-db1e-40dc-8ef4-65e95ff5698f"),
            Title = "超越建筑公司_CascadeSave",
            Address = "上海",
        },
        AchievementsDtos = new List<AchievementsDto>()
        {
            new AchievementsDto()
            {
                AchievementsId=new Guid("10000301-db1e-40dc-8ef4-65e95ff5698f"),
                PersonnelId=personnelId,
                Year=2020,
                Level="A_CascadeSave"
            },
                new AchievementsDto()
            {
                AchievementsId= new Guid("10000302-db1e-40dc-8ef4-65e95ff5698f"),
                PersonnelId=personnelId,
                Year=2021,
                Level="B_CascadeSave"
            }
        }
    };
    return personnel;
}
```

### 单表级联增删改查

```csharp
/// <summary>
/// 单表增删改查
/// </summary>
[TestMethod]
public void SLRD()
{
    var dto = SaveGen();
    LoadGen(dto);
    ReLoadGen(dto);
    DeleteGen(dto);
}

/// <summary>
/// 构造CompanyDto对象，并保存
/// </summary>
/// <returns></returns>
private CompanyDto SaveGen()
{
    var context = new DemoContext();
    //保存
    var newDto = new CompanyDto()
    {
        CompanyId = Guid.NewGuid(),
        Title = "Tim",
        Address = DateTime.Now.ToString(),
    };
    newDto.SaveGen(context);
    var save = context.SaveChanges();
    return newDto;
}

/// <summary>
/// 使用主键从数据库载入CompanyDto对象
/// </summary>
/// <param name="dto"></param>
private void LoadGen(CompanyDto dto)
{
    var context = new DemoContext();
    var loadDto = CompanyDto.LoadGen(context, dto.CompanyId);

    var loadResultDto = CompanyDto.LoadResultGen(context, dto.CompanyId);

    var loadNullResultDto = CompanyDto.LoadResultGen(context, Guid.NewGuid());
}

/// <summary>
/// 从数据库中更新CompanyDto对象中的内容
/// </summary>
/// <param name="dto"></param>
private void ReLoadGen(CompanyDto dto)
{
    var context = new DemoContext();
    var reLoadDto = new CompanyDto() { CompanyId = dto.CompanyId };
    reLoadDto.ReLoadGen(context);
}

/// <summary>
/// 从数据库删除CompanyDto
/// </summary>
/// <param name="dto"></param>
private void DeleteGen(CompanyDto dto)
{
    var context = new DemoContext();
    dto.DeleteGen(context);
    var delete = context.SaveChanges();
    var loadDto = context.Company.FirstOrDefault(x => x.CompanyId == dto.CompanyId);
}
```

### 对象复制

```csharp
/// <summary>
/// 赋值：Dto=>Dto
/// </summary>
[TestMethod]
public void CopyFormDto()
{
    var s = new CompanyDto()
    {
        CompanyId = Guid.NewGuid(),
        Title = "Tim",
        Address = DateTime.Now.ToString(),
    };

    var t = new CompanyDto();
    t.CopyFormDto(s);
}

/// <summary>
/// 赋值：Dto=>Entity
/// </summary>
[TestMethod]
public void CopyToEntity()
{
    var s = new CompanyDto()
    {
        CompanyId = Guid.NewGuid(),
        Title = "Tim",
        Address = DateTime.Now.ToString(),
    };

    var t = new Company();
    s.CopyTo(t);
}

/// <summary>
/// 创建新的CompanyDto
/// </summary>
[TestMethod]
public void NewCompanyDtoGen()
{
    var dto = CompanyDto.NewGen();
    var dtoResult = CompanyDto.NewResultGen();
}

```

### Dto结构

```csharp
[Dto(typeof(DemoContext), typeof(Company))]
public partial class CompanyDto {
    public CompanyDto() { }
    /// <summary>
    /// 企业
    /// </summary>
    [DtoKey]
    public Guid CompanyId { get; set; }
         
    /// <summary>
    /// 名称
    /// </summary>
    [DtoEditDisable]
    public string Title { get; set; }      

    /// <summary>
    /// 地址
    /// </summary> 
    public string Address { get; set; }
}

[Dto(typeof(DemoContext), typeof(Personnel))]
public partial class PersonnelDto
{
    /// <summary>
    /// 员工
    /// </summary>
    [DtoKey]
    public Guid PersonnelId { get; set; }

    /// <summary>
    /// 企业
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// 姓名 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public string Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 是否在职
    /// </summary>
    public bool IsJob { get; set; }

    [DtoForeignKey("Company", "CompanyId", true)]
    public CompanyDto CompanyDto { get; set; }

    [DtoForeignKey("Achievements", "AchievementsId", true, true)]
    public List<AchievementsDto> AchievementsDtos { get; set; }
}

[Dto(typeof(DemoContext), typeof(Achievements))]
public partial class AchievementsDto
{
    [DtoKey]
    public Guid AchievementsId { get; set; }

    /// <summary>
    /// 员工
    /// </summary>
    public Guid PersonnelId { get; set; }

    public int? Year { get; set; }

    public string Level { get; set; }
}
```

### EF实体结构

```csharp
public partial class Company
{
    /// <summary>
    /// 企业
    /// </summary>
    [Key]
    public Guid CompanyId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [StringLength(50)]
    public string Title { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Address { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<Personnel> Personnel { get; } = new List<Personnel>();
}

public partial class Personnel
{
    /// <summary>
    /// 员工
    /// </summary>
    [Key]
    public Guid PersonnelId { get; set; }

    /// <summary>
    /// 企业
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [StringLength(50)]
    public string Gender { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    [Column(TypeName = "date")]
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 是否在职
    /// </summary>
    public bool IsJob { get; set; }

    [InverseProperty("Personnel")]
    public virtual ICollection<Achievements> Achievements { get; } = new List<Achievements>();

    [ForeignKey("CompanyId")]
    [InverseProperty("Personnel")]
    public virtual Company Company { get; set; }
}

public partial class Achievements
{
    [Key]
    public Guid AchievementsId { get; set; }

    /// <summary>
    /// 员工
    /// </summary>
    public Guid PersonnelId { get; set; }

    public int? Year { get; set; }

    [StringLength(50)]
    public string Level { get; set; }

    [ForeignKey("PersonnelId")]
    [InverseProperty("Achievements")]
    public virtual Personnel Personnel { get; set; }
}
```

## 服务自动注册

服务类添加Service特性
```csharp
[Service]
public class ServicesScoped
{
}

[Service(LifeCycle = ELifeCycle.Transient)]
public class ServicesTransient
{
}
```

Program类中增加代码
```csharp
CC.CodeGenerator.AutoDI.AddServices(builder);
```
