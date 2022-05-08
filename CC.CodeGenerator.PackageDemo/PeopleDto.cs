using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    }
}
