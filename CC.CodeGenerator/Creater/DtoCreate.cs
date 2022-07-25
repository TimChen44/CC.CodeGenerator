using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CC.CodeGenerator.Builder
{
    public class DtoCreate
    {
        readonly TypeData TypeData;

        readonly ITypeSymbol TypeSymbol;

        public List<PropertyData> DtoPropertyDatas { get; set; } = new List<PropertyData>();
        public List<PropertyData> DtoForeignKeyPropertyDatas { get; set; } = new List<PropertyData>();
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


        public DtoCreate(ITypeSymbol typeSymbol, TypeData typeData)
        {
            TypeData = typeData;
            TypeSymbol = typeSymbol;

            //获得DBContext的名字
            ContextName = TypeData.DtoAttr.ConstructorArguments[0].Value?.ToString();
            //获得实体类型
            EntitySymbol = TypeData.DtoAttr.ConstructorArguments[1].Value as ITypeSymbol;
            //获得实体属性
            EntityProperties = EntitySymbol?.GetMembers().Where(x => x.Kind == SymbolKind.Property).Cast<IPropertySymbol>().ToList();
            //获得实体主键
            EntityKeyIds = EntityProperties?.Where(x => x.GetAttributes().Any(y => y.AttributeClass.ToDisplayString() == "System.ComponentModel.DataAnnotations.KeyAttribute")).ToList();

            DtoPropertyDatas = TypeData.PropertyAssignDatas.Where(x => x.DtoIgnoreAttr == null).ToList();

            DtoForeignKeyPropertyDatas = TypeData.PropertyReferenceDatas.Where(x => x.DtoForeignKeyAttr != null).ToList();

        }

        public void CreateCode(ClassCodeBuilder dtoBuilder, ClassCodeBuilder extBuilder)
        {
            if (TypeData.DtoAttr == null) return;

            CopyFormDto(dtoBuilder);

            if (string.IsNullOrWhiteSpace(ContextName) == false && EntitySymbol != null && EntityKeyIds?.Count() > 0)
            {
                dtoBuilder.AddUsing("using Microsoft.EntityFrameworkCore;");
                dtoBuilder.AddUsing($"using {EntitySymbol.ContainingNamespace.ToDisplayString()};");
                CopyToEntity(dtoBuilder);

                New(dtoBuilder);
                Load(dtoBuilder);
                FirstQueryable(dtoBuilder);
                ReLoad(dtoBuilder);
                Save(dtoBuilder);
                Delete(dtoBuilder);


                extBuilder.AddUsing("using Microsoft.EntityFrameworkCore;");
                extBuilder.AddUsing($"using {EntitySymbol.ContainingNamespace.ToDisplayString()};");

                var assignCode = extBuilder.AssignCode("", DtoPropertyDatas, "x", EntityProperties, ",");
                ToDtoExtension(extBuilder, assignCode);
                IQueryableToDtosExtension(extBuilder, assignCode);
                ICollectionToDtosExtension(extBuilder, assignCode);
            }
        }

        #region 数据库访问

        // 从Dto赋值值到自己
        private void CopyFormDto(ClassCodeBuilder dtoBuilder)
        {
            var code = dtoBuilder.AssignCode("this", DtoPropertyDatas, "dto", DtoPropertyDatas, ";");
            dtoBuilder.AddMethod(@$"
    /// <summary>
    /// 从Dto赋值值到自己
    /// </summary>
    public virtual void CopyFormDto({TypeData.Name} dto)
    {{
{code}
    }}");
        }

        // 自己的值复制到实体
        private void CopyToEntity(ClassCodeBuilder dtoBuilder)
        {
            if (EntitySymbol == null) return;
            var code = dtoBuilder.AssignCode("entity", EntityProperties, "this", DtoPropertyDatas, ";");

            dtoBuilder.AddMethod(@$"    
    /// <summary>
    /// 自己的值复制到实体
    /// </summary>
    public virtual void CopyToEntity({EntitySymbol.Name} entity)
    {{
{code}
    }}");

        }

        //新对象
        private void New(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyInits = new List<string>();
            foreach (var keyId in EntityKeyIds)
            {
                if (keyId.Type.ToString() != "System.Guid") return;//如果主键中包含非Guid的对象，那么就不要生成初始化代码
                keyInits.Add($"{keyId.Name} = Guid.NewGuid()");
            }
            var keyInit = keyInits.Aggregate((a, b) => a + ", " + b);

            var code = @$"
    /// <summary>
    /// 创建新实体[模拟工厂模式]
    /// </summary>
    /// <returns></returns>
    public static {TypeSymbol.Name} NewGen()
    {{
        return new {TypeSymbol.Name}() {{ {keyInit} }};
    }}

    /// <summary>
    /// 创建新实体并反馈Result
    /// </summary>
    /// <returns></returns>
    public static Result<{TypeSymbol.Name}> NewResultGen()
    {{
        return new Result<{TypeSymbol.Name}>(new {TypeSymbol.Name}() {{ {keyInit} }});
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void Load(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();
            foreach (var keyId in EntityKeyIds)
            {
                keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
                keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
            }
            var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 载入已有实体
    /// </summary>
    /// <returns></returns>
    public static {TypeSymbol.Name}? LoadGen({ContextName} context, {keyParameter})
    {{
        return context.{EntitySymbol.Name}.Where(x => {keyCompare}).To{TypeSymbol.Name}s().FirstOrDefault();
    }}

    /// <summary>
    /// 载入已有实体并反馈Result
    /// </summary>
    /// <returns></returns>
    public static Result<{TypeSymbol.Name}> LoadResultGen({ContextName} context, {keyParameter})
    {{
        var entity = context.{EntitySymbol.Name}.Where(x => {keyCompare}).To{TypeSymbol.Name}s().FirstOrDefault();
        if (entity==null) return new Result<{TypeSymbol.Name}>(""内容不存在"", false);
        else return new Result<{TypeSymbol.Name}>(entity);
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void FirstQueryable(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyCompares = new List<string>();
            foreach (var keyId in EntityKeyIds)
            {
                keyCompares.Add($"x.{keyId.Name} == this.{keyId.Name}");
            }
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{EntitySymbol.Name}> FirstQueryable({ContextName} context)
    {{
        return context.{EntitySymbol.Name}.Where(x => {keyCompare});
    }}";
            dtoBuilder.AddMethod(code);
        }

        //ReLoad 重新加载
        private void ReLoad(ClassCodeBuilder dtoBuilder)
        {
            var code = @$"
    /// <summary>
    /// 重新加载
    /// </summary>
    public Result ReLoadGen({ContextName} context)
    {{
        var dto = FirstQueryable(context).To{TypeData.Name}s().FirstOrDefault();
        if (dto == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        CopyFormDto(dto);
        return Result.OK;
    }}";
            dtoBuilder.AddMethod(code);
        }

        //Save 保存
        private void Save(ClassCodeBuilder dtoBuilder)
        {
            //赋值主键
            List<string> keyInits = new List<string>();
            foreach (var keyId in EntityKeyIds)
            {
                keyInits.Add($"{keyId.Name} = this.{keyId.Name}");
            }
            var keyInit = keyInits.Count() > 0 ? keyInits.Aggregate((a, b) => a + ", " + b) : "";

            //赋值外键
            StringBuilder fkAssignCode = new StringBuilder();
            foreach (var fkProp in DtoForeignKeyPropertyDatas)
            {
                var attr = fkProp.DtoForeignKeyAttr;
                var foreignKey = attr.ConstructorArguments[0].Value;
                var allowNull = attr.ConstructorArguments[1].Value as bool?;

                if (allowNull == true)
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}?.{foreignKey};");
                else
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}.{foreignKey};");
            }

            //子节点保存
            StringBuilder childSaveCode = new StringBuilder();
            foreach (var prop in TypeData.PropertyReferenceDatas)
            {
                var typeSymbol = prop.Property.Type;

                if (typeSymbol.OriginalDefinition.Name == "List")
                {//如果是列表就循环保存
                    var dtoSymbol = (typeSymbol as Microsoft.CodeAnalysis.INamedTypeSymbol)?.TypeArguments.FirstOrDefault();
                    if (dtoSymbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == "CC.CodeGenerator.DtoAttribute") == true)
                    {
                        childSaveCode.AppendLine($"            this.{prop.Name}?.ForEach(x => x.SaveGen(context));");
                    }
                }
                else if (prop.Property.Type.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == "CC.CodeGenerator.DtoAttribute") == true)
                {//如果是单个就独立保存
                    childSaveCode.AppendLine($"            this.{prop.Name}?.SaveGen(context);");
                }
            }

            var code = @$"
    /// <summary>
    /// 保存
    /// </summary>
    public {EntitySymbol.Name} SaveGen({ContextName} context, bool cascadeSave = true)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            entity = new {EntitySymbol.Name}() {{ {keyInit} }};
            context.Add(entity);
        }}
        CopyToEntity(entity);
{fkAssignCode}
        if (cascadeSave == true)
        {{
{childSaveCode}
        }}
        return entity;
    }}";
            dtoBuilder.AddMethod(code);
        }

        //Delete 删除
        private void Delete(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();
            foreach (var keyId in EntityKeyIds)
            {
                keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
                keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
            }
            var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 删除，基于Dto
    /// </summary>
    public bool DeleteGen({ContextName} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            return false;
        }}
        context.Remove(entity);
        return true;
    }}

    /// <summary>
    /// 删除，基于主键
    /// </summary>
    public static bool DeleteGen({ContextName} context, {keyParameter})
    {{
        var entity = context.{EntitySymbol.Name}.Where(x => {keyCompare}).FirstOrDefault();
        if (entity == null)
        {{
            return false;
        }}
        context.Remove(entity);
        return true;
    }}";
            dtoBuilder.AddMethod(code);
        }

        #endregion 

        #region 扩展函数

        private void ToDtoExtension(ClassCodeBuilder extBuilder, StringBuilder code)
        {
            extBuilder.AddMethod(@$"
    public static {TypeData.Name} To{TypeData.Name}(this {EntitySymbol.Name} x)
    {{
        return new {TypeData.Name}()
        {{
{code}
        }};
    }}");
        }

        private void IQueryableToDtosExtension(ClassCodeBuilder extBuilder, StringBuilder code)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// EntitySelect
    /// </summary>
    public static IQueryable<{TypeData.Name}> To{TypeData.Name}s(this IQueryable<{EntitySymbol.Name}> query)
    {{
        return query.Select(x => new {TypeData.Name}()
        {{
{code}
        }});
    }}
");
        }

        private void ICollectionToDtosExtension(ClassCodeBuilder extBuilder, StringBuilder code)
        {
            extBuilder.AddMethod(@$"
    public static List<{TypeData.Name}> To{TypeData.Name}s(this ICollection<{EntitySymbol.Name}> query)
    {{
        return query.Select(x => new {TypeData.Name}()
        {{
{code}
        }}).ToList();
    }}");
        }

        #endregion

    }

}
