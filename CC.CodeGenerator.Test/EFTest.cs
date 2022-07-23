using CC.CodeGenerator.Demo.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace CC.CodeGenerator.Test
{
    [TestClass]
    public class EFTest
    {

        [TestMethod]
        public void Select()
        {
            var context = new DemoContext();

            var company = context.Company.Where(x => x.CompanyId == new Guid("d7abc1aa-ee1a-466e-97ab-5499d7a9c186"))
                 .Select(x => new CompanyDto(x)
                 {
                     Address = x.Address.ToAddressDto(),
                     Personnels = x.Personnel.ToPersonnelDtos()
                 }).FirstOrDefault();

            Assert.IsNotNull(company);
            Assert.IsNotNull(company.Address);
            Assert.IsNotNull(company.Personnels);
        }

        [TestMethod]
        public void CascadeSave()
        {
            var context = new DemoContext();
            var company = CreateCompanyDto();
            company.SaveGen(context);

            Assert.AreEqual(context.Company.Local.Count, 1);
            Assert.AreEqual(context.Address.Local.Count, 1);
            Assert.AreEqual(context.Personnel.Local.Count, 2);
        }

        [TestMethod]
        public void NonCascadingSave()
        {
            var context = new DemoContext();
            var company = CreateCompanyDto();
            company.SaveGen(context, false);
            company.Address.SaveGen(context, false);
            company.Personnels.ForEach(x => x.SaveGen(context, false));

            Assert.AreEqual(context.Company.Local.Count, 1);
            Assert.AreEqual(context.Address.Local.Count, 1);
            Assert.AreEqual(context.Personnel.Local.Count, 2);
        }


        private CompanyDto CreateCompanyDto()
        {
            var companyId = Guid.NewGuid();
            var company = new CompanyDto()
            {
                CompanyId = Guid.NewGuid(),
                Name = DateTime.Now.ToString(),
                Address = new AddressDto()
                {
                    AddressId = Guid.NewGuid(),
                    Name = DateTime.Now.ToString(),
                },
                Personnels = new List<PersonnelDto>()
                {
                    new PersonnelDto()
                    {
                        PersonnelId= Guid.NewGuid(),
                        CompanyId=companyId,
                          Name= DateTime.Now.ToString(),
                          Age=new Random().Next(100),
                    },
                     new PersonnelDto()
                    {
                        PersonnelId= Guid.NewGuid(),
                        CompanyId=companyId,
                        Name= DateTime.Now.ToString(),
                        Age=new Random().Next(100),
                    }
                }
            };
            return company;
        }

    }

    [Dto(nameof(DemoContext),typeof(Company))]
    [Mapping(typeof(Company))]
    public partial class CompanyDto
    {
        public Guid CompanyId { get; set; }

        public string Name { get; set; }

        [DtoForeignKey("AddressId")]
        public AddressDto Address { get; set; }

        public List<PersonnelDto> Personnels { get; set; }
    }

    [Dto(nameof(DemoContext),typeof(Address))]
    [Mapping(typeof(Address))]
    public partial class AddressDto
    {
        public Guid AddressId { get; set; }

        public string Name { get; set; }
    }

    [Dto(nameof(DemoContext),typeof(Personnel))]
    [Mapping(typeof(Personnel))]
    public partial class PersonnelDto
    {
        public Guid PersonnelId { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
    }


}
