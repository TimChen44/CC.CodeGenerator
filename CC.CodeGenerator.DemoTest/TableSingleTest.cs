namespace CC.CodeGenerator.DemoTest
{
    [TestClass]
    public class TableSingleTest
    {
        /// <summary>
        /// 单表增删改查
        /// </summary>
        [TestMethod]
        public void SLRD()
        {
            var dto = SaveGen();
            LoadGen(dto);
            ReLoadGen(dto);
            DeleteGen(dto);
        }

        /// <summary>
        /// 构造CompanyDto对象，并保存
        /// </summary>
        /// <returns></returns>
        private CompanyDto SaveGen()
        {
            var context = new DemoContext();
            //保存
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

        /// <summary>
        /// 使用主键从数据库载入CompanyDto对象
        /// </summary>
        /// <param name="dto"></param>
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

        /// <summary>
        /// 从数据库中更新CompanyDto对象中的内容
        /// </summary>
        /// <param name="dto"></param>
        private void ReLoadGen(CompanyDto dto)
        {
            var context = new DemoContext();
            var reLoadDto = new CompanyDto() { CompanyId = dto.CompanyId };
            reLoadDto.ReLoadGen(context);
            AreEqualDto(dto, reLoadDto);
        }

        /// <summary>
        /// 从数据库删除CompanyDto
        /// </summary>
        /// <param name="dto"></param>
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
}
