
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Demo
{
    [Dto(Context = nameof(DemoaContext), Entity =typeof(People))]
    public partial class PeopleDto
    {
        /// <summary>
        /// 12
        /// </summary>
        public Guid PeopleId { get; set; }
        /// <summary>
        /// 34
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 56
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 78
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 90
        /// </summary>
        public int? Age { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
        public string Remark { get; set; }
    }
}
