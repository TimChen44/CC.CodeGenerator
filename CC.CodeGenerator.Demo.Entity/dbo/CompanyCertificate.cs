﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CC.CodeGenerator.Demo.Entity
{
    public partial class CompanyCertificate
    {
        [Key]
        public Guid CompanyCertificateId { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? Start { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? End { get; set; }
    }
}