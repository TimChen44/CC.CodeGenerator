using CC.CodeGenerator.Demo.Entity;

namespace CC.CodeGenerator.Test
{
    [TestClass]
    public class DtoTest
    {
        [TestMethod]
        public void CopyFormDto()
        {
            var s = new CompanyCertificateDto()
            {
                CompanyCertificateId = Guid.NewGuid(),
                Name = "Tim",
                Start = DateTime.Now,
                End = DateTime.Now,
            };

            var t = new CompanyCertificateDto();
            t.CopyFormDto(s);

            AreEqualDto(s, t);
        }

        [TestMethod]
        public void CopyToEntity()
        {
            var s = new CompanyCertificateDto()
            {
                CompanyCertificateId = Guid.NewGuid(),
                Name = "Tim",
                Start = DateTime.Now,
                End = DateTime.Now,
            };

            var t = new CompanyCertificate();
            s.CopyToEntity(t);

            Assert.AreEqual(s.CompanyCertificateId, t.CompanyCertificateId);
            Assert.AreEqual(s.Name, t.Name);
            Assert.AreEqual(s.Start, t.Start);
            Assert.AreNotEqual(s.End, t.End);
            Assert.AreEqual(t.End, null);
        }

        [TestMethod]
        public void NewGen()
        {
            var dto = CompanyCertificateDto.NewGen();
            Assert.IsNotNull(dto);
            Assert.AreNotEqual(dto.CompanyCertificateId, Guid.Empty);
        }

        [TestMethod]
        public void LoadGen()
        {
            var dto = SaveGen();
            LoadGen(dto);
            ReLoadGen(dto);
            DeleteGen(dto);
        }

        private CompanyCertificateDto SaveGen()
        {
            var context = new DemoContext();
            //±£´æ
            var newDto = new CompanyCertificateDto()
            {
                CompanyCertificateId = Guid.NewGuid(),
                Name = "Tim",
                Start = DateTime.Now,
                End = DateTime.Now,
            };
            newDto.SaveGen(context);
            var save = context.SaveChanges();
            Assert.AreEqual(save, 1);
            return newDto;
        }

        private void LoadGen(CompanyCertificateDto dto)
        {
            var context = new DemoContext();
            var loadDto = CompanyCertificateDto.LoadGen(context, dto.CompanyCertificateId);
            AreEqualDto(dto, loadDto);
        }

        private void ReLoadGen(CompanyCertificateDto dto)
        {
            var context = new DemoContext();
            var reLoadDto = new CompanyCertificateDto() { CompanyCertificateId = dto.CompanyCertificateId };
            reLoadDto.ReLoadGen(context);
            AreEqualDto(dto, reLoadDto);
        }

        private void DeleteGen(CompanyCertificateDto dto)
        {
            var context = new DemoContext();
            dto.DeleteGen(context);
            var delete = context.SaveChanges();
            Assert.AreEqual(delete, 1);
            var loadDto = context.CompanyCertificate.FirstOrDefault(x => x.CompanyCertificateId == dto.CompanyCertificateId);
            Assert.IsNull(loadDto);
        }

        private void AreEqualDto(CompanyCertificateDto s, CompanyCertificateDto t)
        {
            Assert.AreEqual(s.CompanyCertificateId, t.CompanyCertificateId);
            Assert.AreEqual(s.Name, t.Name);
            Assert.AreEqual(s.Start?.ToString(), t.Start?.ToString());
            Assert.AreNotEqual(s.End?.ToString(), t.End?.ToString());
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