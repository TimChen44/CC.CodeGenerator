using CC.CodeGenerator.DemoEntity;
using CC.CodeGenerator;

namespace CC.CodeGenerator.DemoTest
{

    [Dto(typeof(DemoContext), typeof(Achievements))]
    public partial class AchievementsDto
    {
        [DtoKey]
        public Guid AchievementsId { get; set; }

        /// <summary>
        /// 员工
        /// </summary>
        public Guid PersonnelId { get; set; }

        public int? Year { get; set; }

        public string Level { get; set; }
    }
}