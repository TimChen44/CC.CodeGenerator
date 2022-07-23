using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Builder
{
    public class DtoBuilder
    {
        readonly TypeData TypeData;

        readonly ITypeSymbol TypeSymbol;

        public DtoBuilder(ITypeSymbol typeSymbol, string classType, TypeData typeData)
        {
            TypeData = typeData;
            TypeSymbol = typeSymbol;

        }


        public string CreateCode(ClassCodeBuilder dtoBuilder, ClassCodeBuilder extBuilder)
        {
            dtoBuilder.AddMethod(CopyFormDto());
            dtoBuilder.AddMethod(CopyToEntity());

            if (string.IsNullOrWhiteSpace(TypeData.ContextName) == false && TypeData.EntitySymbol != null && TypeData.EntityKeyIds?.Count() > 0)
            {
                dtoBuilder.AddUsing("using Microsoft.EntityFrameworkCore;");
                dtoBuilder.AddMethod(New());
                dtoBuilder.AddMethod(Load());
                dtoBuilder.AddMethod(FirstQueryable());
                dtoBuilder.AddMethod(ReLoad());
                dtoBuilder.AddMethod(Save());
                dtoBuilder.AddMethod(Delete());
            }


        }

        // 从Dto赋值值到自己
        private void CopyFormDto(ClassCodeBuilder dtoBuilder)
        {
            var code = dtoBuilder.AssignCode("this", TypeData.Properties, "dto", TypeData.Properties, ";");
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
        private string CopyToEntity()
        {
            if (TypeData.EntitySymbol == null) return null;
            var code = AssignCode("entity", TypeData.EntityProperties, "this", TypeData.Properties, ";");

            return @$"    
    /// <summary>
    /// 自己的值复制到实体
    /// </summary>
    public virtual void CopyToEntity({TypeData.Name} entity)
    {{
{code}
    }}";

        }

        //新对象
        private string New()
        {
            List<string> keyInits = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                if (keyId.Type.ToString() != "System.Guid") return "";//如果主键中包含非Guid的对象，那么就不要生成初始化代码
                keyInits.Add($"{keyId.Name} = Guid.NewGuid()");
            }
            var keyInit = keyInits.Aggregate((a, b) => a + ", " + b);

            return @$"
    /// <summary>
    /// 创建新实体[模拟工厂模式]
    /// </summary>
    /// <returns></returns>
    public static {TypeSymbol.Name} NewGen()
    {{
        return new {TypeSymbol.Name}() {{ {keyInit} }};
    }}";
        }

        private string Load()
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
                keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
            }
            var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            return @$"
    /// <summary>
    /// 载入已有实体
    /// </summary>
    /// <returns></returns>
    public static {TypeSymbol.Name}? LoadGen({TypeData.ContextName} context, {keyParameter})
    {{
        return context.{TypeData.EntitySymbol.Name}.Where(x => {keyCompare}).To{TypeSymbol.Name}s().FirstOrDefault();
    }}";
        }

        private string FirstQueryable()
        {
            List<string> keyCompares = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                keyCompares.Add($"x.{keyId.Name} == this.{keyId.Name}");
            }
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            return @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{TypeData.EntitySymbol.Name}> FirstQueryable({TypeData.ContextName} context)
    {{
        return context.{TypeData.EntitySymbol.Name}.Where(x => {keyCompare});
    }}";
        }

        //ReLoad 重新加载
        private string ReLoad()
        {
            return @$"
    /// <summary>
    /// 重新加载
    /// </summary>
    public Result ReLoadGen({TypeData.ContextName} context)
    {{
        var dto = FirstQueryable(context).To{TypeData.Name}s().FirstOrDefault();
        if (dto == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        CopyFormDto(dto);
        return Result.OK;
    }}";
        }

        //Save 保存
        private string Save()
        {
            //赋值主键
            List<string> keyInits = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                keyInits.Add($"{keyId.Name} = this.{keyId.Name}");
            }
            var keyInit = keyInits.Count() > 0 ? keyInits.Aggregate((a, b) => a + ", " + b) : "";

            //赋值外键
            StringBuilder fkAssignCode = new StringBuilder();
            foreach (var fkProp in TypeData.PropertyDatas.Where(x => x.DtoForeignKeyAttr != null))
            {
                var attr = fkProp.DtoForeignKeyAttr;
                var foreignKey = attr.ConstructorArguments[0].Value;
                var allowNull = attr.ConstructorArguments[1].Value as bool?;

                if (allowNull == true)
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}?.{foreignKey};");
                else
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}.{foreignKey};");
            }

            return @$"
    /// <summary>
    /// 保存
    /// </summary>
    public {TypeData.EntitySymbol.Name} SaveGen({TypeData.ContextName} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            entity = new {TypeData.EntitySymbol.Name}() {{ {keyInit} }};
            context.Add(entity);
        }}
        CopyToEntity(entity);
{fkAssignCode}
        return entity;
    }}";
        }

        //Delete 删除
        private string Delete()
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
                keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");
            }
            var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            return @$"
    /// <summary>
    /// 删除，基于Dto
    /// </summary>
    public Result DeleteGen({TypeData.ContextName} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        context.Remove(entity);
        return Result.OK;
    }}

    /// <summary>
    /// 删除，基于主键
    /// </summary>
    public static Result DeleteGen({TypeData.ContextName} context, {keyParameter})
    {{
        var entity = context.{TypeData.EntitySymbol.Name}.Where(x => {keyCompare}).FirstOrDefault();
        if (entity == null)
        {{
            return new Result(""内容不存在"", false);
        }}
        context.Remove(entity);
        return Result.OK;
    }}";
        }


        private string EntitySelectExtension()
        {
            var code = AssignCode("", TypeData.Properties, "x", TypeData.EntityProperties, ",");

            return @$"
public static class {TypeData.Name}Extension
{{
    /// <summary>
    /// EntitySelect
    /// </summary>
    public static IQueryable<{TypeData.Name}> To{TypeData.Name}s(this IQueryable<{TypeData.EntitySymbol.Name}> query)
    {{
        return query.Select(x => new {TypeData.Name}()
        {{
{code}
        }});
    }}
}}";

        }

    }


}
