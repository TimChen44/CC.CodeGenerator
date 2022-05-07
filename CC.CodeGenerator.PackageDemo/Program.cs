global using CC.CodeGenerator;
global using CC.CodeGenerator.Demo.Entity;
using CC.CodeGenerator.PackageDemo;
using System.Text;


#region MappingGenerator

//初始新的Dto
var people1Map = new People1Map() { PeopleId=Guid.NewGuid(),UserName="Tim" };
var people2Map = new People2Map();
var people3Map = new People3Map() { City="ShangHai"};

//复制到对象
people1Map.CopyTo(people2Map);

//从对象复制来
people1Map.CopyFrom(people3Map);

#endregion


#region DtoGenerator

var context = new DemoaContext();

//初始新的Dto
var firstDto = PeopleDto.NewGen();

//快速载入Dto
var secondDto = new PeopleDto() {City="ShangHai"  };

//快速从Dto复制
firstDto.CopyFormDto(secondDto);

//EF快速Select
var peopleEntityDtos = context.People.ToPeopleDtos();

//快速载入Dto
var peopleEntityDto = PeopleDto.LoadGen(context, Guid.NewGuid());

//快速复制到实体
var peopleEntity = context.People.FirstOrDefault();
peopleEntityDto.CopyToEntity(peopleEntity);

//Dto快速重新载入
peopleEntityDto.ReLoadGen(context);

//Dto快速保存
peopleEntityDto.City = "北京";
peopleEntityDto.SaveGen(context);

//Dto快速删除
peopleEntityDto.DeleteGen(context);

//最后保存操作
context.SaveChanges();

#endregion