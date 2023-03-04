using CC.CodeGenerator.Common.DtoStructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CC.CodeGenerator.Common.Reader
{
    public class SyntaxTreeReader
    {

        public SyntaxTreeReader()
        {
        }

        public DtoClass AnalysisTypeDeclarationSyntax(TypeDeclarationSyntax syntaxTree)
        {
            var genClass = GenClass.Create(syntaxTree);
            if (genClass == null) return null;

            return AnalysisClass(genClass);
        }


        /// <summary>
        /// 分析表达式树，获得DtoClass对象集合
        /// </summary>
        /// <param name="syntaxTree"></param>
        /// <returns></returns>
        public DtoClass AnalysisClass(GenClass genClass)
        {
            var classSyntax = genClass.ClassSyntax;

            Console.WriteLine($"Reader - {classSyntax.Identifier.Text}");

            //读取类名
            var dtoClass = new DtoClass();
            dtoClass.Name = classSyntax.Identifier.Text;

            //读取生成器属性
            if (genClass.DtoAttrSyntax != null)
            {
                dtoClass.DtoConfig = new DtoGeneratorConfig();
                dtoClass.DtoConfig.Context = genClass.DtoAttrSyntax.ArgumentList?.Arguments[0].ChildNodes().First().ChildNodes().First().ToFullString() ?? "/* 缺少Context配置 */";
                dtoClass.DtoConfig.Entity = genClass.DtoAttrSyntax.ArgumentList?.Arguments[1].ChildNodes().First().ChildNodes().First().ToFullString() ?? "/* 缺少Entity配置 */";
                dtoClass.DtoConfig.DtoNamespace = classSyntax.GetNamespace() ?? "CodeGenerator";
            }

            //读取构造函数信息
            dtoClass.DtoConfig.HasDefaultConstructor = classSyntax.ChildNodes().Any(x => x is ConstructorDeclarationSyntax s && s.ParameterList.ChildNodes().Count() == 0);

            //读取属性
            var propertySyntaxs = classSyntax.ChildNodes().Where(x => x.IsKind(SyntaxKind.PropertyDeclaration));
            foreach (PropertyDeclarationSyntax propertySyntax in propertySyntaxs)
            {
                //public bool? CBool { get; set; }

                if (propertySyntax.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString() == "DtoIgnore")) == true)
                    continue;//存在忽略特性就忽略属性

                if (propertySyntax.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString() == "DtoForeignKey")) == true)
                {
                    DtoForeignProperty foreProperty = new DtoForeignProperty();
                    ReadForeignPropertyInfo(propertySyntax, foreProperty);
                    if (foreProperty.RelationType == ERelationType.Single)
                        dtoClass.ParentDtos.Add(foreProperty);
                    else
                        dtoClass.SubDtos.Add(foreProperty);
                }
                else
                {//普通属性时的操作
                    DtoProperty dtoProperty = new DtoProperty();
                    ReadPropertyInfo(propertySyntax, dtoProperty);
                    if (dtoProperty.Type.IsDataType)
                        dtoClass.Properties.Add(dtoProperty);
                }
            }
            //设置主键
            dtoClass.Keys = dtoClass.Properties.Where(x => x.IsKey).ToList();

            return dtoClass;
        }

        //读取属性
        private void ReadPropertyInfo(PropertyDeclarationSyntax propertySyntax, DtoProperty dtoProperty)
        {
            //属性名字
            dtoProperty.Name = propertySyntax.Identifier.Text;

            //判断是否只读
            if (propertySyntax.ChildNodes().Any(x => x.IsKind(SyntaxKind.ArrowExpressionClause)))
            {
                dtoProperty.IsReadOnly = true;
            }
            if (propertySyntax.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.AccessorList))?.ChildNodes().Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration)) == false)
            {
                dtoProperty.IsReadOnly = true;
            }

            //检查是否是不可编辑的
            if (propertySyntax.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString() == "DtoEditDisable")) == true)
                dtoProperty.IsEditDisable = true;

            //检查是否可允许空
            IEnumerable<SyntaxNode> typeChildNodes;
            var nullSyntax = propertySyntax.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.NullableType));
            if (nullSyntax != null)
            {
                dtoProperty.AllowNull = true;
                typeChildNodes = nullSyntax.ChildNodes();
            }
            else
            {
                typeChildNodes = propertySyntax.ChildNodes();
            }

            //分析出类型
            var typeSyntax = typeChildNodes.FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierName) ||
                x.IsKind(SyntaxKind.PredefinedType) || x.IsKind(SyntaxKind.GenericName));

            if (typeSyntax.IsKind(SyntaxKind.PredefinedType))
            {//检查是否是普通类型
                dtoProperty.Type = new CSharpPropertyType(typeSyntax.ToFullString().Trim());
            }
            else if (typeSyntax.IsKind(SyntaxKind.IdentifierName))
            {//检查是否是引用类型
                var typeName = typeSyntax.ToFullString().Trim();
                dtoProperty.Type = new CSharpPropertyType(typeName);
            }
            else if (typeSyntax.IsKind(SyntaxKind.GenericName))
            {
                var typeName = typeSyntax.ChildNodes().First().ChildNodes().First().ToFullString();
                dtoProperty.Type = new CSharpPropertyType(typeName);
            }

            dtoProperty.IsKey = propertySyntax.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString() == "DtoKey")) == true;
        }

        private void ReadForeignPropertyInfo(PropertyDeclarationSyntax propertySyntax, DtoForeignProperty foreProperty)
        {
            ReadPropertyInfo(propertySyntax, foreProperty);

            var foreignKeyAttrSyntax = propertySyntax.AttributeLists.SelectMany(x => x.Attributes.Where(y => y.Name.ToString() == "DtoForeignKey")).First();

            var foreignKeyAttr = foreignKeyAttrSyntax.ArgumentList?.Arguments;
            foreProperty.ForeignTable = foreignKeyAttr?[0]?.Expression?.ToFullString()?
                .Replace("nameof(", "")?.Trim('"', ')', ' ') ?? "";
            foreProperty.ForeignKeyId = foreignKeyAttr?[1]?.Expression?.ToFullString()?
                .Replace("nameof(", "")?.Trim('"', ')', ' ') ?? "";

            if (foreignKeyAttr?.Count > 2)
            {
                foreProperty.AutoCascadeSave = foreignKeyAttr?[2].ChildNodes().Any(x => x.IsKind(SyntaxKind.TrueLiteralExpression)) ?? false;
            }
            if (foreignKeyAttr?.Count > 3)
            {
                foreProperty.AutoDeleteExcess = foreignKeyAttr?[3].ChildNodes().Any(x => x.IsKind(SyntaxKind.TrueLiteralExpression)) ?? false;
            }

            //外键名称先默认为属性名称
            foreProperty.ForeignKeyName = foreProperty.Name;

            //判断链接类型，此处简化了逻辑，假设一对多都会采用List<T>泛型链接，所以用GenericName来判断
            if (propertySyntax.GetFirstSyntaxNode(x => x.IsKind(SyntaxKind.GenericName)) == null)
            {
                foreProperty.RelationType = ERelationType.Single;
            }
            else
            {
                foreProperty.RelationType = ERelationType.Multiple;
            }
        }

        /// <summary>
        /// 符合生成代码的Class
        /// </summary>
        public record GenClass
        {
            public static GenClass Create(TypeDeclarationSyntax syntax)
            {
                if (syntax.AttributeLists.Count == 0) return null;
                var attrSyntaxs = syntax.AttributeLists.SelectMany(x => x.Attributes.Where(y => y.Name.ToString() == "Dto" || y.Name.ToString() == "Mapping")).ToList();
                if (attrSyntaxs.Count == 0) return null;
                var genClass = new GenClass()
                {
                    ClassSyntax = syntax,
                    AttrSyntaxs = attrSyntaxs,
                };
                return genClass;
            }


            /// <summary>
            /// 代码表达式树
            /// </summary>
            public TypeDeclarationSyntax ClassSyntax { get; set; }

            /// <summary>
            /// 特性集合
            /// </summary>
            public List<AttributeSyntax> AttrSyntaxs { get; set; } = new List<AttributeSyntax>();

            /// <summary>
            /// Dto特性
            /// </summary>
            public AttributeSyntax? DtoAttrSyntax => AttrSyntaxs.FirstOrDefault(x => x.Name.ToFullString() == "Dto") ?? null;

            /// <summary>
            /// Mapping特性
            /// </summary>
            public AttributeSyntax? MappingAttrSyntax => AttrSyntaxs.FirstOrDefault(x => x.Name.ToFullString() == "Mapping") ?? null;

        }

    }
}

