using CC.CodeGenerator.Demo.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CC.CodeGenerator.Test
{
    [TestClass]
    public class DtoTest
    {
        [TestMethod]
        public void CopyFormDto()
        {
            var t = new CompanyCertificateDto();
            var s = new CompanyCertificateDto()
            {
                CompanyCertificateId = Guid.NewGuid(),
                Name = "Tim",
                End = DateTime.Now,
            };
            t.CopyFormDto(s);

            Assert.AreEqual(s.CompanyCertificateId, t.CompanyCertificateId);
            Assert.AreEqual(s.Name, t.Name);
            Assert.AreNotEqual(s.End, t.End);
            Assert.AreEqual(t.End, null);
        }
    }


    [Dto(Context = nameof(DemoContext), Entity = typeof(CompanyCertificate))]
    public partial class CompanyCertificateDto
    {
        public Guid CompanyCertificateId { get; set; } 

        public string Name { get; set; } 

        public DateTime? Start { get; set; }

        [DtoIgnore]
        public DateTime? End { get; set; }
    }

}