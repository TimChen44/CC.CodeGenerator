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

        public DtoBuilder(ITypeSymbol typeSymbol, TypeData typeData)
        {
            TypeData = typeData;
            TypeSymbol = typeSymbol;

        }

        public void CreateCode(ClassCodeBuilder dtoBuilder, ClassCodeBuilder extBuilder)
        {
            if (TypeData.DtoAttr == null) return;

            CopyFormDto(dtoBuilder);
            CopyToEntity(dtoBuilder);

            if (string.IsNullOrWhiteSpace(TypeData.ContextName) == false && TypeData.EntitySymbol != null && TypeData.EntityKeyIds?.Count() > 0)
            {
                dtoBuilder.AddUsing("using Microsoft.EntityFrameworkCore;");
                New(dtoBuilder);
                Load(dtoBuilder);
                FirstQueryable(dtoBuilder);
                ReLoad(dtoBuilder);
                Save(dtoBuilder);
                Delete(dtoBuilder);

                EntitySelectExtension(extBuilder);
            }
        }

        // 从Dto赋值值到自己
        private void CopyFormDto(ClassCodeBuilder dtoBuilder)
        {
            var code = dtoBuilder.AssignCode("this", TypeData.DtoPropertyDatas, "dto", TypeData.DtoPropertyDatas, ";");
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
            if (TypeData.EntitySymbol == null) return;
            var code = dtoBuilder.AssignCode("entity", TypeData.EntityProperties, "this", TypeData.DtoPropertyDatas, ";");

            dtoBuilder.AddMethod(@$"    
    /// <summary>
    /// 自己的值复制到实体
    /// </summary>
    public virtual void CopyToEntity({TypeData.Name} entity)
    {{
{code}
    }}");

        }

        //新对象
        private void New(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyInits = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
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
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void Load(ClassCodeBuilder dtoBuilder)
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

            var code = @$"
    /// <summary>
    /// 载入已有实体
    /// </summary>
    /// <returns></returns>
    public static {TypeSymbol.Name}? LoadGen({TypeData.ContextName} context, {keyParameter})
    {{
        return context.{TypeData.EntitySymbol.Name}.Where(x => {keyCompare}).To{TypeSymbol.Name}s().FirstOrDefault();
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void FirstQueryable(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyCompares = new List<string>();
            foreach (var keyId in TypeData.EntityKeyIds)
            {
                keyCompares.Add($"x.{keyId.Name} == this.{keyId.Name}");
            }
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{TypeData.EntitySymbol.Name}> FirstQueryable({TypeData.ContextName} context)
    {{
        return context.{TypeData.EntitySymbol.Name}.Where(x => {keyCompare});
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
            dtoBuilder.AddMethod(code);
        }

        //Save 保存
        private void Save(ClassCodeBuilder dtoBuilder)
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
            foreach (var fkProp in TypeData.DtoPropertyDatas.Where(x => x.DtoForeignKeyAttr != null))
            {
                var attr = fkProp.DtoForeignKeyAttr;
                var foreignKey = attr.ConstructorArguments[0].Value;
                var allowNull = attr.ConstructorArguments[1].Value as bool?;

                if (allowNull == true)
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}?.{foreignKey};");
                else
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}.{foreignKey};");
            }

            var code = @$"
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
            dtoBuilder.AddMethod(code);
        }

        //Delete 删除
        private void Delete(ClassCodeBuilder dtoBuilder)
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

            var code = @$"
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
            dtoBuilder.AddMethod(code);
        }


        private void EntitySelectExtension(ClassCodeBuilder extBuilder)
        {
            var code = extBuilder.AssignCode("", TypeData.DtoPropertyDatas, "x", TypeData.EntityProperties, ",");

            extBuilder.AddMethod(@$"
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
}}");

        }

    }


}
