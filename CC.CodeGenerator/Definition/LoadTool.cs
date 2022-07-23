using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Definition
{
    public class LoadTool
    {
        public INamedTypeSymbol DtoAttSymbol { get; set; }
        public INamedTypeSymbol DtoIgnoreAttrSymbol { get; set; }
        public INamedTypeSymbol DtoForeignKeyAttrSymbol { get; set; }

        public INamedTypeSymbol MappingAttrSymbol { get; set; }
        public INamedTypeSymbol MappingIgnoreAttrSymbol { get; set; }

        public LoadTool(Compilation compilation)
        {
            //获得DtoAttribute类符号
            DtoAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoAttribute");
            //获得DtoIgnoreAttribute类符号
            DtoIgnoreAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoIgnoreAttribute");
            //获得DtoForeignKeyAttribute类符号
            DtoForeignKeyAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.DtoForeignKeyAttribute");


            //获得MappingAttribute类符号
            MappingAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.MappingAttribute");
            //获得MappingIgnoreAttribute类符号
            MappingIgnoreAttrSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.MappingIgnoreAttribute");
            
        }

        public TypeData CreateTypeData(ITypeSymbol typeSymbol)
        {
            return new TypeData(this, typeSymbol);
        }

    }
}
