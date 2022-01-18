using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.CodeGenerator.Demo
{
    //[Dto(Entity = typeof(EntityDemo),KeyId = "MyProperty1"), Display()]
    [Dto(DBContext = "DBContext", Entity = typeof(EntityDemo), KeyId = "MyProperty1")]
    public partial record DemoDto
    {
        [Ignore]
        public int MyProperty1 { get; set; }

        [DisplayName("用户角色")]
        public Guid RoleId { get; set; }
        [DisplayName("系统名称")]
        public string SystemName { get; set; } 
        [DisplayName("角色名称")]
        public string Name { get; set; }
        [DisplayName("描述")]
        public string? Dept { get; set; }

        public IEnumerable<EntityDemo> Features { get; set; } = new List<EntityDemo>();

        public void Mod()
        {

        }

    }

    public partial class EntityDemo
    {
        public int MyProperty1 { get; set; }
        public int AAA { get; set; }

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TestAttribute : Attribute
    {
        public Type Entity { get; set; }


    }

    public class DBContext
    {
        public IQueryable<EntityDemo> EntityDemo { get; set; }
    }
}

