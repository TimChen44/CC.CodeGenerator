using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Builder
{
    public class MapBuilder
    {
        readonly TypeData TypeData;

        readonly ITypeSymbol TypeSymbol;

        public MapBuilder(ITypeSymbol typeSymbol, TypeData typeData)
        {
            TypeData = typeData;
            TypeSymbol = typeSymbol;
        }

        public void CreateCode(ClassCodeBuilder mapBuilder)
        {
            if (TypeData.MappingAttr == null) return;

            StringBuilder mappingBuilder = new StringBuilder();

     
            //获得目标类型
            List<ITypeSymbol> targetSymbols = TypeData.MappingAttr.ConstructorArguments.FirstOrDefault().Values.Select(x => x.Value as ITypeSymbol).Distinct().ToList();

            foreach (var targetSymbol in targetSymbols)
            {
                //获得目标属性
                var targetProperties = targetSymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList() ?? new List<IPropertySymbol>();
                var code = MappingCopy(mapBuilder,TypeSymbol, TypeData.MappingPropertyDatas, targetSymbol, targetProperties);
                mapBuilder.AddMethod(code);
            }

            //检查是否有默认构造，如果有就不用创建默认，否则创建默认构造
            var constructorDeclaration = (TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax)?.Members.FirstOrDefault(x => x.Kind() == SyntaxKind.ConstructorDeclaration) as ConstructorDeclarationSyntax;
            if (constructorDeclaration == null || constructorDeclaration.ParameterList.Parameters.Count != 0)
            {
                var defaultConstructor = $"    public {TypeSymbol.Name}() {{ }}";
                mapBuilder.AddConstructor(defaultConstructor);
            }
        }

        // 映射复制
        private string MappingCopy(ClassCodeBuilder mapBuilder, ITypeSymbol classSymbol, IEnumerable<PropertyData> mappingProperties, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            if (targetSymbol == null) return null;

            var codeCopyTo = mapBuilder.AssignCode("target", targetProperties, "this", mappingProperties, ";");
            var codeCopyFrom = mapBuilder.AssignCode("this", mappingProperties, "source", targetProperties, ";");

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