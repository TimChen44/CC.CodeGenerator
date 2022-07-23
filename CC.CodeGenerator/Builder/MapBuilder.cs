using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Builder
{
    public class MapBuilder : ClassCodeBuilder
    {
        public TypeData TypeData { get; }

        public MapBuilder(ITypeSymbol typeSymbol, string classType, TypeData typeData) : base(typeSymbol, classType)
        {
            TypeData = typeData;

        }

        // 映射复制
        private string MappingCopy(ITypeSymbol classSymbol, IEnumerable<IPropertySymbol> mappingProperties, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            if (targetSymbol == null) return null;

            var codeCopyTo = AssignCode("target", targetProperties, "this", mappingProperties, ";");
            var codeCopyFrom = AssignCode("this", mappingProperties, "source", targetProperties, ";");

            return @$"
    /// <summary>
    /// 基于源赋值初始化
    /// </summary>
    public {classSymbol.Name}({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
        CopyFrom(source);
    }}

    /// <summary>
    /// 将自己赋值到目标
    /// </summary>
    public {classSymbol.Name} CopyTo({targetSymbol.ContainingNamespace}.{targetSymbol.Name} target)
    {{
{codeCopyTo}
        return this;
    }}

    /// <summary>
    /// 从源赋值到自己
    /// </summary>
    public {classSymbol.Name} CopyFrom({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
{codeCopyFrom}
        return this;
    }}";



        }
    }


}