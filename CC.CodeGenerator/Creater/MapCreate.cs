using CC.CodeGenerator.Definition;
using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Builder
{
    public class MapCreate
    {
        readonly ITypeSymbol TypeSymbol;

        public MapCreate(ITypeSymbol typeSymbol)
        {
            TypeSymbol = typeSymbol;
        }

        //在Map文件中生成Map代码
        public void CreateMapCode(ClassCodeBuilder mapBuilder, TypeData typeData)
        {
            if (typeData.MappingAttr == null) return;
            //Map的目标
            List<ITypeSymbol> targetSymbols = typeData.MappingAttr.ConstructorArguments.FirstOrDefault().Values.Select(x => x.Value as ITypeSymbol).Distinct().ToList();

            //Map所有字段
            var mappingPropertiesAll = typeData.PropertyAssignDatas;
            //Map排除后字段
            var mappingPropertiesIgnore = mappingPropertiesAll.Where(x => x.MappingIgnoreAttr == null).ToList();

            DefaultConstructor(mapBuilder);

            foreach (var targetSymbol in targetSymbols)
            {
                //获得所有字段
                var targetPropertiesAll = targetSymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .ToList() ?? new List<IPropertySymbol>();
                //目标排除后字段
                var targetPropertiesIgnore = targetPropertiesAll.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.MappingIgnoreAttribute") == false);

                Construction(mapBuilder, targetSymbol);
                CopyTo(mapBuilder, mappingPropertiesAll, targetSymbol, targetPropertiesIgnore);
                CopyFrom(mapBuilder, mappingPropertiesIgnore, targetSymbol, targetPropertiesAll);
            }

        }


        public void CreateDtoCode(ClassCodeBuilder dtoBuilder, TypeData typeData, ITypeSymbol targetSymbol)
        {
            //Map的目标
            List<ITypeSymbol> targetSymbols = new List<ITypeSymbol>() { targetSymbol };
            //Map的字段
            var dtoPropertiesAll = typeData.PropertyAssignDatas;
            var dtoPropertiesIgnore = typeData.PropertyAssignDatas.Where(x => x.DtoIgnoreAttr == null).ToList();

            var targetPropertiesAll = targetSymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property)
                   .Cast<IPropertySymbol>()
                   .ToList() ?? new List<IPropertySymbol>();
            //目标排除后字段
            var targetPropertiesIgnore = targetPropertiesAll.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.MappingIgnoreAttribute") == false);

            DefaultConstructor(dtoBuilder);
            Construction(dtoBuilder, targetSymbol);
            CopyTo(dtoBuilder, dtoPropertiesAll, targetSymbol, targetPropertiesIgnore);
            CopyFrom(dtoBuilder, dtoPropertiesIgnore, targetSymbol, targetPropertiesAll);
        }

       

        private void DefaultConstructor(ClassCodeBuilder codeBuilder)
        {
            //检查是否有默认构造，如果有就不用创建默认，否则创建默认构造
            var constructorDeclaration = (TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax)?.Members.FirstOrDefault(x => x.Kind() == SyntaxKind.ConstructorDeclaration) as ConstructorDeclarationSyntax;
            if (constructorDeclaration == null || constructorDeclaration.ParameterList.Parameters.Count != 0)
            {
                var defaultConstructor = $"    public {TypeSymbol.Name}() {{ }}";
                codeBuilder.AddConstructor(defaultConstructor);
            }
        }

            //构造复制
        private void Construction(ClassCodeBuilder mapBuilder, ITypeSymbol targetSymbol)
        {
            var code = $@"
    /// <summary>
    /// 基于源赋值初始化
    /// </summary>
    public {TypeSymbol.Name}({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
        CopyFrom(source);
    }}";
            mapBuilder.AddConstructor(code);
        }

        private void CopyTo(ClassCodeBuilder mapBuilder, IEnumerable<PropertyData> sourceProperties, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            var codeCopyTo = mapBuilder.AssignCode("target", targetProperties, "this", sourceProperties, ";");
            var code = $@"
    /// <summary>
    /// 将自己赋值到目标
    /// </summary>
    public {TypeSymbol.Name} CopyTo({targetSymbol.ContainingNamespace}.{targetSymbol.Name} target)
    {{
{codeCopyTo}
        return this;
    }}";
            mapBuilder.AddConstructor(code);
        }

        private void CopyFrom(ClassCodeBuilder mapBuilder, IEnumerable<PropertyData> sourceProperties, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            var codeCopyFrom = mapBuilder.AssignCode("this", sourceProperties, "source", targetProperties, ";");

            var code = $@"
    /// <summary>
    /// 从源赋值到自己
    /// </summary>
    public {TypeSymbol.Name} CopyFrom({targetSymbol.ContainingNamespace}.{targetSymbol.Name} source)
    {{
{codeCopyFrom}
        return this;
    }}";
            mapBuilder.AddConstructor(code);
        }

    }
}