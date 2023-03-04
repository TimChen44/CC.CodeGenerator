namespace CC.CodeGenerator.DemoTest;

[TestClass]
public class DtoTest
{
    [TestMethod]
    public void CopyFormDto()
    {
        var s = new CompanyDto()
        {
            CompanyId = Guid.NewGuid(),
            Title = "Tim",
            Address = DateTime.Now.ToString(),
        };

        var t = new CompanyDto();
        t.CopyFormDto(s);

        AreEqualDto(s, t);
    }

    [TestMethod]
    public void CopyToEntity()
    {
        var s = new CompanyDto()
        {
            CompanyId = Guid.NewGuid(),
            Title = "Tim",
            Address = DateTime.Now.ToString(),
        };

        var t = new Company();
        s.CopyTo(t);

        Assert.AreEqual(s.CompanyId, t.CompanyId);
        Assert.AreEqual(s.Title, t.Title);
        Assert.AreEqual(s.Address, t.Address);
    }

    [TestMethod]
    public void NewGen()
    {
        var dto = CompanyDto.NewGen();
        Assert.IsNotNull(dto);
        Assert.AreNotEqual(dto.CompanyId, Guid.Empty);

        var dtoResult = CompanyDto.NewResultGen();
        Assert.IsNotNull(dtoResult);
        Assert.AreNotEqual(dtoResult?.Data?.CompanyId, Guid.Empty);
    }

    [TestMethod]
    public void SLRD()
    {
        var dto = SaveGen();
        LoadGen(dto);
        ReLoadGen(dto);
        DeleteGen(dto);
    }

    private CompanyDto SaveGen()
    {
        var context = new DemoContext();
        //±£´æ
        var newDto = new CompanyDto()
        {
            CompanyId = Guid.NewGuid(),
            Title = "Tim",
            Address = DateTime.Now.ToString(),
        };
        newDto.SaveGen(context);
        var save = context.SaveChanges();
        Assert.AreEqual(save, 1);
        return newDto;
    }

    private void LoadGen(CompanyDto dto)
    {
        var context = new DemoContext();
        var loadDto = CompanyDto.LoadGen(context, dto.CompanyId);
        AreEqualDto(dto, loadDto);
        var loadResultDto = CompanyDto.LoadResultGen(context, dto.CompanyId);
        AreEqualDto(dto, loadResultDto.Data);
        var loadNullResultDto = CompanyDto.LoadResultGen(context, Guid.NewGuid());
        Assert.AreEqual(loadNullResultDto.IsOK, false);
    }

    private void ReLoadGen(CompanyDto dto)
    {
        var context = new DemoContext();
        var reLoadDto = new CompanyDto() { CompanyId = dto.CompanyId };
        reLoadDto.ReLoadGen(context);
        AreEqualDto(dto, reLoadDto);
    }

    private void DeleteGen(CompanyDto dto)
    {
        var context = new DemoContext();
        dto.DeleteGen(context);
        var delete = context.SaveChanges();
        Assert.AreEqual(delete, 1);
        var loadDto = context.Company.FirstOrDefault(x => x.CompanyId == dto.CompanyId);
        Assert.IsNull(loadDto);
    }

    private void AreEqualDto(CompanyDto s, CompanyDto t)
    {
        Assert.AreEqual(s.CompanyId, t.CompanyId);
        Assert.AreEqual(s.Title, t.Title);
        Assert.AreEqual(s.Address, t.Address);
    }
}



