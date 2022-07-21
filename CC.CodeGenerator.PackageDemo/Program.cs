global using CC.CodeGenerator.Demo.Entity;
using CC.CodeGenerator.PackageDemo;

var context = new DemoContext();

var p1 = PeopleDto.NewGen();
var p2 = PeopleDto.NewGen();
p1.SaveGen(context);
p2.SaveGen(context);



#region MappingGenerator

//初始新的Dto
var people1Map = new People1Map() { PeopleId = Guid.NewGuid(), Name = "Tim" };
var people2Map = new People2Map();
var people3Map = new People3Map() { CityTitle = "ShangHai" };

//复制到对象
people1Map.CopyTo(people2Map);

//从对象复制来
people1Map.CopyFrom(people3Map);

//从别的对象初始化
var people4Map = new People1Map(people2Map);

#endregion



#region EFSelect

//简化赋值
var PeopleViewDtos1 = context.People
    .Select(x => new PeopleViewDto(x)
        {
            CityTitle = x.City.CityTitle,
            SkillViews = x.Skill.Select(y => new SkillViewDto()
            {
                PeopleId=y.PeopleId,
                SkillId=y.SkillId,
                SkillName=y.SkillName,
            }).ToList()
        })
    .ToList();

//通过级联CopyFrom函数可以从多个实体获得数据
var PeopleViewDtos2 = context.People
    .Select(x => new PeopleViewDto(x)
        {
            SkillViews = x.Skill.Select(y => new SkillViewDto(y)).ToList()
        }
    .CopyFrom(x.City))
    .ToList();

#endregion

#region DtoGenerator

//创建Dto
var secondDto = new PeopleDto() { Age = 20 };


//初始新的Dto
var firstDto = PeopleDto.NewGen();

//快速从Dto复制
firstDto.CopyFormDto(secondDto);

//EF快速Select
var peopleEntityDtos = context.People.Where(x => x.Age == 20).ToPeopleDtos().ToList();

//快速载入Dto
var peopleEntityDto = PeopleDto.LoadGen(context, new Guid("25fcf1e5-a47c-432a-b2c6-25a2a09a5e01"));

//Dto复制到实体
var peopleEntity = context.People.FirstOrDefault();
peopleEntityDto.CopyToEntity(peopleEntity);

//Dto重新载入
peopleEntityDto.ReLoadGen(context);

//Dto快速保存
peopleEntityDto.Age = 10;
peopleEntityDto.SaveGen(context);

//Dto快速删除
peopleEntityDto.DeleteGen(context);

//主键快速删除
PeopleDto.DeleteGen(context, new Guid("25fcf1e5-a47c-432a-b2c6-25a2a09a5e01"));

//最后保存操作
//context.SaveChanges();

#endregion

Console.Read();