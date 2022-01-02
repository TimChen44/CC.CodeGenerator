using System.ComponentModel.DataAnnotations;

namespace CC.CodeGenerator.Demo
{
    [Dto(DBContext="1234567890"), Display()]
    public partial class DemoDto
    {
        public int MyProperty1 { get; set; }

        public void Mod()
        {

        }
    }
}

