using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Builder
{
    public class MapCreate
    {
        readonly TypeData TypeData;

        readonly ITypeSymbol TypeSymbol;

        public List<PropertyData> MappingPropertyDatas { get; set; } = new List<PropertyData>();

        public MapCreate(ITypeSymbol typeSymbol, TypeData typeData)
        {
            TypeData = typeData;
            TypeSymbol = typeSymbol;

            MappingPropertyDatas = TypeData.PropertyAssignDatas.Where(x => x.MappingIgnoreAttr == null).ToList();
        }

        public void CreateCode(ClassCodeBuilder mapBuilder)
        {
            if (TypeData.MappingAttr == null) return;

            //检查是否有默认构造，如果有就不用创建默认，否则创建默认构造
            var constructorDeclaration = (TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax)?.Members.FirstOrDefault(x => x.Kind() == SyntaxKind.ConstructorDeclaration) as ConstructorDeclarationSyntax;
            if (constructorDeclaration == null || constructorDeclaration.ParameterList.Parameters.Count != 0)
            {
                var defaultConstructor = $"    public {TypeSymbol.Name}() {{ }}";
                mapBuilder.AddConstructor(defaultConstructor);
            }

            //获得目标类型
            List<ITypeSymbol> targetSymbols = TypeData.MappingAttr.ConstructorArguments.FirstOrDefault().Values.Select(x => x.Value as ITypeSymbol).Distinct().ToList();

            foreach (var targetSymbol in targetSymbols)
            {
                //获得目标属性
                var targetProperties = targetSymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .ToList() ?? new List<IPropertySymbol>();

                Construction(mapBuilder, targetSymbol);
                CopyTo(mapBuilder, targetSymbol, targetProperties);
                CopyFrom(mapBuilder, targetSymbol, targetProperties);
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

        private void CopyTo(ClassCodeBuilder mapBuilder, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            //目标对象中包含MappingIgnore对象，就不进行覆盖
            var tProperties = targetProperties.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "CC.CodeGenerator.MappingIgnoreAttribute") == false);

            var codeCopyTo = mapBuilder.AssignCode("target", tProperties, "this", TypeData.PropertyAssignDatas, ";");

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

        private void CopyFrom(ClassCodeBuilder mapBuilder, ITypeSymbol targetSymbol, IEnumerable<IPropertySymbol> targetProperties)
        {
            var codeCopyFrom = mapBuilder.AssignCode("this", MappingPropertyDatas, "source", targetProperties, ";");

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