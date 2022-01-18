using System.ComponentModel.DataAnnotations;

namespace CC.CodeGenerator.Demo
{
    public partial class EntityDemo
    {
        public int Fint { get; set; }
        public long Flong { get; set; }



        public int AAA { get; set; }

        public int AAA1 { get; set; }

    }
    //[CC.CodeGenerator.Dto(DBContext = "DBContext")]
    [CC.CodeGenerator.Dto(DBContext = "DBContext", Entity = typeof(EntityDemo), KeyId = "MyProperty1")]
    public partial class DemoDto
    {
        public int Fint { get; set; }
        public long Flong { get; set; }
        public bool Fbool { get; set; }
        public decimal Fdecimal { get; set; }
        public float Ffloat { get; set; }
        public double Fdouble { get; set; }
        public DateTime FDateTime { get; set; }
        public string Fstring { get; set; }
        public int? FintN { get; set; }
        public long? FlongN { get; set; }
        public bool? FboolN { get; set; }
        public decimal? FdecimalN { get; set; }
        public float? FfloatN { get; set; }
        public double? FdoubleN { get; set; }
        public DateTime? FDateTimeN { get; set; }
        public string? FstringN { get; set; }

        public int MyProperty1 { get; set; }



        public void Mod()
        {

        }
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

