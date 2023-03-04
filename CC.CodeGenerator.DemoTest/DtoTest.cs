namespace CC.CodeGenerator.DemoTest;

[TestClass]
public class DtoTest
{
    /// <summary>
    /// 赋值：Dto=>Dto
    /// </summary>
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

    /// <summary>
    /// 赋值：Dto=>Entity
    /// </summary>
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

    /// <summary>
    /// 创建新的CompanyDto
    /// </summary>
    [TestMethod]
    public void NewCompanyDtoGen()
    {
        var dto = CompanyDto.NewGen();
        Assert.IsNotNull(dto);
        Assert.AreNotEqual(dto.CompanyId, Guid.Empty);

        var dtoResult = CompanyDto.NewResultGen();
        Assert.IsNotNull(dtoResult);
        Assert.AreNotEqual(dtoResult?.Data?.CompanyId, Guid.Empty);
    }

    /// <summary>
    /// 创建新的PersonnelDto
    /// //TODO: 级联KeyId初始化未完成
    /// </summary>
    //[TestMethod]
    //public void NewGen()
    //{
    //    var dto = PersonnelDto.NewGen();
    //    Assert.IsNotNull(dto);
    //    Assert.AreNotEqual(dto.CompanyId, Guid.Empty);
    //    Assert.AreNotEqual(dto?.CompanyDto.CompanyId, Guid.Empty);
    //    Assert.AreNotEqual(dto?.AchievementsDtos, null);
    //}

    private void AreEqualDto(CompanyDto s, CompanyDto t)
    {
        Assert.AreEqual(s.CompanyId, t.CompanyId);
        Assert.AreEqual(s.Title, t.Title);
        Assert.AreEqual(s.Address, t.Address);
    }
}



