global using CC.CodeGenerator;
global using CC.CodeGenerator.Demo.Entity;
using CC.CodeGenerator.PackageDemo;

var context=new DemoaContext();


var peoples= PeopleEditDto.SelectGen(context.People.Where(x => x.UserName.StartsWith("Latanya"))).ToList();

var me= peoples.FirstOrDefault();
me.City = "上海";
me.SaveGen(context);
context.SaveChanges();