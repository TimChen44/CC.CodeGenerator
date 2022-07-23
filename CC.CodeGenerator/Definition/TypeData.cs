using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Definition
{
    public class TypeData
    {
        private readonly ITypeSymbol TypeSymbol;
        public string Name => TypeSymbol?.Name;

        //可以用于赋值的属性
        public List<PropertyData> PropertyAssignDatas { get; set; } = new List<PropertyData>();
        //不能用于赋值的引用类型
        public List<PropertyData> PropertyReferenceDatas { get; set; } = new List<PropertyData>();

        public AttributeData DtoAttr { get; set; }

        public AttributeData MappingAttr { get; set; }

        public TypeData(LoadTool loadTool, ITypeSymbol typeSymbol, AttributeData dtoAttr, AttributeData mappingAttr)
        {
            TypeSymbol = typeSymbol;
            DtoAttr = dtoAttr;
            MappingAttr = mappingAttr;

            //类中的属性，使用延迟初始化，如果没有对应的特性就免去此处的反射操作优化性能
            var props = typeSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList();
            foreach (var prop in props)
            {
                var propData = new PropertyData(loadTool, prop);

                if ((prop.Type.IsValueType == true || prop.Type?.MetadataName == "String") && prop.IsReadOnly == false)
                    PropertyAssignDatas.Add(propData);
                else if (prop.Type.IsReferenceType)
                    PropertyReferenceDatas.Add(propData);
            }
        }

    }
}
