using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.PackageDemo
{
    [Mapping(typeof(People), typeof(City))]
    public partial class PeopleViewDto
    {
        public Guid PeopleId { get; set; }
        public string Name { get; set; }

        public string CityTitle { get; set; }

        public List<SkillViewDto> SkillViews { get; set; }
    }

    [Mapping(typeof(Skill))]
    public partial class SkillViewDto
    {

        public Guid SkillId { get; set; }
        public Guid PeopleId { get; set; }
        public string SkillName { get; set; }
    }

}
