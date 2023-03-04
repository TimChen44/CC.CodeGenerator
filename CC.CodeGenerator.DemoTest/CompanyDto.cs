using CC.CodeGenerator.DemoEntity;

namespace CC.CodeGenerator.DemoTest
{

    [Dto(typeof(DemoContext), typeof(Company))]
    public partial class CompanyDto {
        public CompanyDto() { }
        /// <summary>
        /// 企业
        /// </summary>
        [DtoKey]
        public Guid CompanyId { get; set; }
         
        /// <summary>
        /// 名称
        /// </summary>
        [DtoEditDisable]
        public string Title { get; set; }      

        /// <summary>
        /// 地址
        /// </summary> 
        public string Address { get; set; }
    }

}