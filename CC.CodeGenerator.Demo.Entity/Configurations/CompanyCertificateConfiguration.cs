﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using CC.CodeGenerator.Demo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace CC.CodeGenerator.Demo.Entity.Configurations
{
    public partial class CompanyCertificateConfiguration : IEntityTypeConfiguration<CompanyCertificate>
    {
        public void Configure(EntityTypeBuilder<CompanyCertificate> entity)
        {
            entity.Property(e => e.CompanyCertificateId).ValueGeneratedNever();

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<CompanyCertificate> entity);
    }
}
