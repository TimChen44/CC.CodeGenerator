using CC.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.PackageDemo
{
    [Dto(Context = nameof(DemoContext), Entity = typeof(People))]
    public partial record PeopleDto
    {
        public Guid PeopleId { get; set; }
        public string Name { get; set; }

        public int? Age { get; set; }

        public string Disply => $"{Name}";

        [DtoForeignKey("CityId",true)]
        public CityDto City { get; set; }     
    }

    [Dto(Context = nameof(DemoContext), Entity = typeof(City))]
    public partial class CityDto
    {
        /// <summary> 
        /// 城市
        /// </summary>
        public string CityId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string CityTitle { get; set; }

    }
}
