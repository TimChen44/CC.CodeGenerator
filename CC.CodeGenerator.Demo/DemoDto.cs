using System.ComponentModel.DataAnnotations;

namespace CC.CodeGenerator.Demo
{
    [Dto(DBContext = "DBContext", Entity = typeof(EntityDemo),KeyId = "MyProperty1"), Display()]

    public partial class DemoDto
    {
        public int MyProperty1 { get; set; }

        public int BB { get; set; }

        public int AAA { get; set; }

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

    }
}

