using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Definition
{
    public class PropertyData
    {
        public IPropertySymbol Property { get; set; }
        public string Name => Property?.Name;
 
        public AttributeData DtoIgnoreAttr { get; set; }

        public AttributeData DtoForeignKeyAttr { get; set; }

        public AttributeData MappingIgnoreAttr { get; set; }


        public PropertyData(LoadTool loadTool, IPropertySymbol prop)
        {
            Property = prop;

            //查找属性
            var attrs = Property.GetAttributes();

            //检查Dto是否忽略
            DtoIgnoreAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.DtoIgnoreAttrSymbol, SymbolEqualityComparer.Default));
            if (Property.Type.IsReferenceType)//只有引用类型才需要判断是否是外键
            {
                DtoForeignKeyAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.DtoForeignKeyAttrSymbol, SymbolEqualityComparer.Default));
            }

            //检查Mapping是否忽略
            MappingIgnoreAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.MappingIgnoreAttrSymbol, SymbolEqualityComparer.Default));

        }
    }
}
