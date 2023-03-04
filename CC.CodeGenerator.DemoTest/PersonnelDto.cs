using CC.CodeGenerator.DemoEntity;
using CC.CodeGenerator;

namespace CC.CodeGenerator.DemoTest
{

    [Dto(typeof(DemoContext), typeof(Personnel))]
    public partial class PersonnelDto
    {
        /// <summary>
        /// 员工
        /// </summary>
        [DtoKey]
        public Guid PersonnelId { get; set; }

        /// <summary>
        /// 企业
        /// </summary>
        public Guid CompanyId { get; set; }

        /// <summary>
        /// 姓名 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 是否在职
        /// </summary>
        public bool IsJob { get; set; }

        [DtoForeignKey("Company", "CompanyId", true)]
        public CompanyDto CompanyDto { get; set; }

        [DtoForeignKey("Achievements", "AchievementsId", true, true)]
        public List<AchievementsDto> AchievementsDtos { get; set; }
    }
}