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

   
        public AttributeData DtoAttr { get; set; }
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

        public List<PropertyData> PropertyDatas { get; set; } = new List<PropertyData>();
        public List<IPropertySymbol> Properties { get; set; } = new List<IPropertySymbol>();

        public TypeData(LoadTool loadTool, ITypeSymbol typeSymbol)
        {
            LoadTool = loadTool;
            TypeSymbol = typeSymbol;

            //读取类的特性
            var attrs = typeSymbol.GetAttributes();
           
            //读取实体操作配置
            DtoAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.DtoAttSymbol, SymbolEqualityComparer.Default));
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
            }

            //读取映射配置
            MappingAttr = attrs.FirstOrDefault(x => x.AttributeClass.Equals(loadTool.MappingAttrSymbol, SymbolEqualityComparer.Default));


            //类中的属性，使用延迟初始化，如果没有对应的特性就免去此处的反射操作优化性能
            var props = typeSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList();
            foreach (var prop in props)
            {
                var propData = new PropertyData(loadTool, prop);
                if (propData.IsIgnore == true) continue;
                Properties.Add(prop);
                PropertyDatas.Add(propData);
            }

        }

    }
}
