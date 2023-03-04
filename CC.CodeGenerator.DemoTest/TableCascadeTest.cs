namespace CC.CodeGenerator.DemoTest;

[TestClass]
public class TableCascadeTest
{
    /// <summary>
    /// 级联查询
    /// </summary>
    [TestMethod]
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
    /// 级联查询
    /// </summary>
    [TestMethod]
    public void SelectStandardSingle()
    {
        var context = new DemoContext();

        var personne = context.Personnel.Where(x => x.PersonnelId == new Guid("92c26f2e-1dda-4c0b-9279-a4a66560d4be"))
             .Select(x => new PersonnelDto(x)
             {
                 CompanyDto = x.Company.ToCompanyDto(),
                 AchievementsDtos = x.Achievements.ToAchievementsDtos(),
             }).FirstOrDefault();

        Assert.IsNotNull(personne);
        Assert.AreNotEqual(personne.PersonnelId, Guid.Empty);
        Assert.IsNotNull(personne.CompanyDto);
        Assert.IsNotNull(personne.AchievementsDtos);
    }

    /// <summary>
    /// 简单级联查询
    /// </summary>
    [TestMethod]
    public void SelectEasySingle()
    {
        var context = new DemoContext();

        var personne = PersonnelDto.LoadGen(context, new Guid("92c26f2e-1dda-4c0b-9279-a4a66560d4be"));

        Assert.IsNotNull(personne);
        Assert.AreNotEqual(personne.PersonnelId, Guid.Empty);
        Assert.IsNotNull(personne.CompanyDto);
        Assert.IsNotNull(personne.AchievementsDtos);
    }

    /// <summary>
    /// 级联查询多条
    /// </summary>
    [TestMethod]
    public void SelectList()
    {
        var context = new DemoContext();

        var personnes = context.Personnel
            .Where(x => x.IsJob == true && (x.Company.Address == "上海" || x.Company.Address == "北京"))
            .ToPersonnelDtos().ToList();

        Assert.IsTrue(personnes.Count > 0);
    }

    /// <summary>
    /// 级联操作
    /// </summary>
    [TestMethod]
    public void CascadeSave()
    {
        var context = new DemoContext();

        //模拟前端提交了新增或编辑的Dto
        var personnel = CreatePersonnelDto();
        //保存
        personnel.SaveGen(context);
        var saveResult = context.SaveChanges();
        Assert.IsTrue(saveResult > 0);

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
        //保存
        personnel.SaveGen(context);
        var updateResult = context.SaveChanges();
        Assert.IsTrue(updateResult > 0);

        //模拟删除Dto
        personnel.DeleteGen(context);
        var deleteResult = context.SaveChanges();
        Assert.IsTrue(deleteResult > 0);
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


}
