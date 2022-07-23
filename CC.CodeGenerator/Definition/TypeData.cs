using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Definition
{
    public class TypeData
    {
        private readonly LoadTool LoadTool;

        private readonly ITypeSymbol TypeSymbol;
        public string Name => TypeSymbol?.Name;
        public List<PropertyData> PropertyDatas { get; set; } = new List<PropertyData>();


        public AttributeData DtoAttr { get; set; }
        public List<PropertyData> DtoPropertyDatas { get; set; } = new List<PropertyData>();
        /// <summary>
        /// 上下文名称
        /// </summary>
        public string ContextName { get; set; }
        /// <summary>
        /// 实体符号
        /// </summary>
        public ITypeSymbol EntitySymbol { get; set; }
        /// <summary>
        /// 实体属性
        /// </summary>
        public List<IPropertySymbol> EntityProperties { get; set; }
        /// <summary>
        /// 实体主键
        /// </summary>
        public List<IPropertySymbol> EntityKeyIds { get; set; }


        public AttributeData MappingAttr { get; set; }
        public List<PropertyData> MappingPropertyDatas { get; set; } = new List<PropertyData>();


        public TypeData(LoadTool loadTool, ITypeSymbol typeSymbol, AttributeData dtoAttr, AttributeData mappingAttr)
        {
            LoadTool = loadTool;
            TypeSymbol = typeSymbol;
            DtoAttr = dtoAttr;
            MappingAttr = mappingAttr;

            //类中的属性，使用延迟初始化，如果没有对应的特性就免去此处的反射操作优化性能
            var props = typeSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList();
            foreach (var prop in props)
            {
                var propData = new PropertyData(loadTool, prop);
                PropertyDatas.Add(propData);
            }

            if (DtoAttr != null)
            {
                //获得DBContext的名字
                ContextName = DtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Context").Value.Value?.ToString();
                //获得实体类型
                EntitySymbol = DtoAttr.NamedArguments.FirstOrDefault(x => x.Key == "Entity").Value.Value as ITypeSymbol;
                //获得实体属性
                EntityProperties = EntitySymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList();
                //获得实体主键
                EntityKeyIds = EntityProperties?.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "System.ComponentModel.DataAnnotations.KeyAttribute")).ToList();

                DtoPropertyDatas = PropertyDatas.Where(x => x.DtoIgnoreAttr == null).ToList();
            }

            if (MappingAttr != null)
            {
                MappingPropertyDatas = PropertyDatas.Where(x => x.MappingIgnoreAttr == null).ToList();
            }
        }

    }
}
