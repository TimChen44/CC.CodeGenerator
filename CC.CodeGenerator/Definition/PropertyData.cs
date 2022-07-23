using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Definition
{
    public class PropertyData
    {
        public IPropertySymbol Property { get; set; }
        public string Name => Property?.Name;
        public bool IsReadOnly => Property?.IsReadOnly ?? true;

        /// <summary>
        /// 是否允许赋值，就是可以在等号左边
        /// </summary>
        public bool AllowAssign { get; set; }

        public bool IsIgnore { get; set; }

        public AttributeData DtoForeignKeyAttr { get; set; }

        public PropertyData(LoadTool loadTool, IPropertySymbol prop)
        {
            Property = prop;

            //查找属性
            var attrs = Property.GetAttributes();
            //检查是否忽略
            IsIgnore = attrs.Any(x => x.AttributeClass.Equals(loadTool.DtoIgnoreAttrSymbol, SymbolEqualityComparer.Default));
            if (IsIgnore) return;

            //判断是否可以放在等号左边
            AllowAssign = (Property.Type.IsValueType == true || Property.Type?.MetadataName == "String") && Property.IsReadOnly == false;

            if (Property.Type.IsReferenceType)//只有引用类型才需要判断是否是外键
            {
                DtoForeignKeyAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.DtoForeignKeyAttrSymbol, SymbolEqualityComparer.Default));
            }
        }
    }
}
