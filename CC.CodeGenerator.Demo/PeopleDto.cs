﻿
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Demo
{
    [Dto(Context = nameof(DemoaContext))]
    public partial class PeopleEditDto
    {
        public Guid PeopleId { get; set; }

        public string UserName { get; set; }

        public string City { get; set; }
    }
}
