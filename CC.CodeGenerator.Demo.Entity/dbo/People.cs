﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CC.CodeGenerator.Demo.Entity
{
    [Index(nameof(Age), Name = "Age")]
    public partial class People
    {
        /// <summary>
        /// 12
        /// </summary>
        [Key]
        public Guid PeopleId { get; set; }
        /// <summary>
        /// 34
        /// </summary>
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        /// <summary>
        /// 56
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        /// <summary>
        /// 78
        /// </summary>
        [StringLength(5)]
        public string Sex { get; set; }
        /// <summary>
        /// 90
        /// </summary>
        public int? Age { get; set; }
        [StringLength(50)]
        public string Country { get; set; }
        [StringLength(50)]
        public string City { get; set; }
        [StringLength(4000)]
        public string Address { get; set; }
        [StringLength(50)]
        public string Email { get; set; }
        [StringLength(50)]
        public string Phone { get; set; }
        public string Remark { get; set; }
    }
}