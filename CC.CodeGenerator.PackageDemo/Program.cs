global using CC.CodeGenerator;
global using CC.CodeGenerator.Demo.Entity;
using CC.CodeGenerator.PackageDemo;

var context=new DemoaContext();

var peoples=PeopleEditDto
    .SelectGen(context.People.Where(x => x.UserName.StartsWith("Latanya") )).ToList();

var p = peoples.FirstOrDefault();
p.City = "北京";
p.SaveGen(context);
context.SaveChanges(); 
