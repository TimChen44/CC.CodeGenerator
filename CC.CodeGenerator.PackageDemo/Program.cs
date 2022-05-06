global using CC.CodeGenerator;
global using CC.CodeGenerator.Demo.Entity;
using CC.CodeGenerator.PackageDemo;

var context=new DemoaContext();

//初始新的Dto
var peopleFirstDto = PeopleDto.NewGen();

//快速载入Dto
var peopleSecondDto = new PeopleDto() {City="ShangHai" };

//快速从Dto复制
peopleFirstDto.CopyFormDto(peopleSecondDto);

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

//保存操作
context.SaveChanges();