using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.PackageDemo
{
    [Dto(Context=nameof(DemoaContext),Entity =typeof(People))]
    public partial record PeopleEditDto
    {
        public Guid PeopleId { get; set; }
        public string UserName { get; set; }

        public string City { get; set; }

        [Ignore]
        public string Disply => $"{UserName}";
    }



}
