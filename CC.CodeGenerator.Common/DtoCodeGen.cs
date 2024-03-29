﻿using CC.CodeGenerator.Common.DtoStructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common
{
    public class DtoCodeGen
    {
        DtoClass DtoClass;
        DtoGeneratorConfig DtoConfig => DtoClass.DtoConfig;

        public DtoCodeGen(DtoClass dtoClass)
        {
            DtoClass = dtoClass;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoClass"></param>
        /// <param name="entityNameSpace"></param>
        /// <returns></returns>
        public string GenCode()
        {
            if (string.IsNullOrEmpty(DtoConfig.Context) == true || string.IsNullOrEmpty(DtoConfig.Entity) == true || DtoClass.Key == null)
                return "/* 缺少主键定义，请使用Key特性标记主键字段 */";

            try
            {
                var dtoBuilder = new ClassCodeBuilder();

                //赋值和复制
                var mapBuilder = new ClassCodeBuilder();

                DefaultConstructor(mapBuilder);
                CopyConstruction(mapBuilder);
                CopyTo(mapBuilder);
                CopyFrom(mapBuilder);
                CopyFormDto(mapBuilder);

                //数据库操作
                dtoBuilder.AddUsing("using Microsoft.EntityFrameworkCore;");

dtoBuilder.AddUsing(@"using CC.CodeGenerator.DemoEntity;
using CC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;");



                New(dtoBuilder);
                Load(dtoBuilder);
                FirstQueryable(dtoBuilder);
                ReLoad(dtoBuilder);
                Save(dtoBuilder);
                Delete(dtoBuilder);
                StaticDelete(dtoBuilder);

                //赋值扩展
                var assignBuilder = new ClassCodeBuilder();
                var assignCode = AssignCode("", DtoClass.Properties, "x", DtoClass.Properties, ",");
                ToDtoExtension(assignBuilder, assignCode);
                IQueryableToDtosExtension(assignBuilder, assignCode);
                ICollectionToDtosExtension(assignBuilder, assignCode);

                //集合操作扩展
                var collectionBuilder = new ClassCodeBuilder();
                DtoSaveGenExtension(collectionBuilder);
                DtoDeleteGenExtension(collectionBuilder);
                DtoDeleteExcessGenExtension(collectionBuilder);

                //组装代码
                string usingsCode = dtoBuilder.Usings.Union(assignBuilder.Usings).Distinct().Aggregate((a, b) => a + "\r\n" + b);

                string dtoCode = @$"// <auto-generated> 
// 此文件由CC.CodeGenerator自动生成，请不要修改。
// 生成时间：{DateTime.Now.ToString()}
// 技术支持：https://chintsso.feishu.cn/wiki/wikcndw8CeKBnPbO5KVgVnFdBBc
// </auto-generated>
{usingsCode}
namespace {DtoClass.DtoConfig.DtoNamespace};

public partial {DtoConfig.DtoType} {DtoClass.Name}
{{
    #region 赋值操作
{mapBuilder.BuildConstructors()}
{mapBuilder.BuildMethods()}
    #endregion

    #region 数据库操作
{dtoBuilder.BuildMethods()}
    #endregion
}}

public static class {DtoClass.Name}Extension
{{
    #region 赋值操作
{assignBuilder.BuildMethods()}
    #endregion

    #region 集合数据库操作
{collectionBuilder.BuildMethods()}
    #endregion
}}
";
                return dtoCode;
            }
            catch (Exception ex)
            {
                return @$"// <auto-generated> 
// 此文件由CC.CodeGenerator自动生成，请不要修改。
// 生成时间：{DateTime.Now.ToString()}
// 技术支持：https://chintsso.feishu.cn/wiki/wikcndw8CeKBnPbO5KVgVnFdBBc
// </auto-generated>
/* 
生成代码发生错误，错误详情。
{ex.ToString()}
*/
";
            }
        }

        #region 赋值和复制

        //构造函数
        private void DefaultConstructor(ClassCodeBuilder codeBuilder)
        {
            if (DtoConfig.HasDefaultConstructor == true) return;
            var defaultConstructor = $"    public {DtoClass.Name}() {{ }}";
            codeBuilder.AddConstructor(defaultConstructor);
        }

        //构造复制
        private void CopyConstruction(ClassCodeBuilder mapBuilder)
        {
            var code = $@"
    /// <summary>
    /// 基于源赋值初始化
    /// </summary>
    public {DtoClass.Name}({DtoClass.DtoConfig.EntityNamespaceString}{DtoClass.DtoConfig.Entity} source)
    {{
        CopyFrom(source);
    }}";
            mapBuilder.AddConstructor(code);
        }

        //
        private void CopyTo(ClassCodeBuilder mapBuilder)
        {
            var codeCopyTo = AssignCode(DtoClass.Properties, "target", "this", ";");
            var code = $@"
    /// <summary>
    /// 将自己赋值到目标：Dto=>Entity
    /// </summary>
    public virtual {DtoClass.Name} CopyTo({DtoClass.DtoConfig.EntityNamespaceString}{DtoClass.DtoConfig.Entity} target)
    {{
{codeCopyTo}
        return this;
    }}";
            mapBuilder.AddConstructor(code);
        }

        //从实体复制到Dto
        private void CopyFrom(ClassCodeBuilder mapBuilder)
        {
            var codeCopyFrom = AssignCode("this", DtoClass.Properties, "source", DtoClass.Properties, ";");

            var code = $@"
    /// <summary>
    /// 从源赋值到自己：Entiy=>Dto
    /// </summary>
    public virtual {DtoClass.Name} CopyFrom({DtoClass.DtoConfig.EntityNamespaceString}{DtoClass.DtoConfig.Entity} source)
    {{
{codeCopyFrom}
        return this;
    }}";
            mapBuilder.AddConstructor(code);
        }

        // 从Dto赋值值到自己
        private void CopyFormDto(ClassCodeBuilder dtoBuilder)
        {
            var code = AssignCode("this", DtoClass.Properties, "dto", DtoClass.Properties, ";");
            dtoBuilder.AddMethod(@$"
    /// <summary>
    /// 从Dto赋值值到自己：Dto=>Dto
    /// </summary>
    public virtual {DtoClass.Name} CopyFormDto({DtoClass.Name} dto)
    {{
{code}
        return this;
    }}");
        }

        /// <summary>
        /// 赋值代码
        /// </summary>
        public StringBuilder AssignCode(string leftName, List<DtoProperty> leftProperties,
            string rightName, List<DtoProperty> rightProperties, string separate)
        {
            var code = new StringBuilder();
            foreach (var leftProp in leftProperties)
            {
                if (leftProp.IsReadOnly) continue;
                var rightProp = rightProperties.FirstOrDefault(x => x.Name == leftProp.Name);
                if (rightProp == null) continue;
                code.AppendLine($"        {(string.IsNullOrWhiteSpace(leftName) ? "" : $"{leftName}.")}{leftProp.Name} = {rightName}.{rightProp.Name}{separate}");
            }
            return code;
        }

        /// <summary>
        /// 从Dto赋值到Entity的代码
        /// 加入了不可编辑的逻辑
        /// </summary>
        public StringBuilder AssignCode(List<DtoProperty> properties, string leftName, string rightName, string separate)
        {
            var code = new StringBuilder();
            foreach (var prop in properties)
            {
                if (prop.IsReadOnly) continue;
                code.AppendLine($"        {(string.IsNullOrWhiteSpace(leftName) ? "" : $"{leftName}.")}{prop.Name} = {rightName}.{prop.Name}{separate}");
            }
            return code;
        }

        /// <summary>
        /// 从Dto赋值到Entity的代码
        /// 加入了不可编辑的逻辑
        /// </summary>
        public StringBuilder AssignCodeDot2EntityNew(List<DtoProperty> properties)
        {
            var code = new StringBuilder();
            foreach (var prop in properties)
            {
                if (prop.IsReadOnly) continue;
                if (prop.IsEditDisable == false) continue;
                code.AppendLine($"            entity.{prop.Name} = this.{prop.Name};");
            }
            return code;
        }

        /// <summary>
        /// 从Dto赋值到Entity的代码
        /// 加入了不可编辑的逻辑
        /// </summary>
        public StringBuilder AssignCodeDot2EntityEdit(List<DtoProperty> properties)
        {
            var code = new StringBuilder();
            foreach (var prop in properties)
            {
                if (prop.IsReadOnly) continue;
                if (prop.IsEditDisable == true) continue;
                code.AppendLine($"        entity.{prop.Name} = this.{prop.Name};");
            }
            return code;
        }

        #endregion

        #region 数据库访问

        //新对象
        private void New(ClassCodeBuilder dtoBuilder)
        {
            //List<string> keyInits = new List<string>();
            //foreach (var key in DtoClass.Keys)
            //{
            //    if (key.Type.Name != "Guid") return;//如果主键中包含非Guid的对象，那么就不要生成初始化代码
            //    keyInits.Add($"{key.Name} = Guid.NewGuid()");
            //}
            //var keyInit = keyInits.Aggregate((a, b) => a + ", \r\n" + b);

            var keyInit = DtoClass.Keys.Where(x => x.Type.Name == "Guid")
                 .Select(x => $"            {x.Name} = Guid.NewGuid(),")
                 .Aggregate("\r\n");

            var parentInit = DtoClass.ParentDtos.Select(x => $"            {x.Name} = {x.Type.Name}.NewGen(),")
                .Aggregate("\r\n");
            var subInit = DtoClass.SubDtos.Select(x => $"            {x.Name} = new List<{x.Type.Name}>(), ")
                .Aggregate("\r\n");

            //TODO: 级联KeyId初始化存尚存一些问题
            //StringBuilder pkeyAssign = new StringBuilder();
            //StringBuilder pkeyAssign = new StringBuilder();

            //foreach (var item in DtoClass.SubDtos)
            //{
            //    if (DtoClass.ParentDtos.Any(x=>x.Name== item.Name)==false ) continue;
            //    pkeyAssign.AppendLine($@"        dto.{item.Name}" = )

            //}


            var code = @$"
    /// <summary>
    /// 创建新实体[模拟工厂模式]
    /// </summary>
    /// <returns></returns>
    public static {DtoClass.Name} NewGen()
    {{
        var dto = new {DtoClass.Name}() 
        {{ 
{keyInit}
{parentInit}
{subInit}
        }};
        return dto;
        
    }}

    /// <summary>
    /// 创建新实体并反馈Result
    /// </summary>
    /// <returns></returns>
    public static Result<{DtoClass.Name}> NewResultGen()
    {{
        return new Result<{DtoClass.Name}>(NewGen());
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void Load(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();
            foreach (var keyId in DtoClass.Keys)
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
    public static {DtoClass.Name}? LoadGen({DtoConfig.Context} context, {keyParameter})
    {{
        return context.{DtoConfig.Entity}.Where(x => {keyCompare}).To{DtoClass.Name}s().FirstOrDefault();
    }}

    /// <summary>
    /// 载入已有实体并反馈Result
    /// </summary>
    /// <returns></returns>
    public static Result<{DtoClass.Name}> LoadResultGen({DtoConfig.Context} context, {keyParameter})
    {{
        var entity = context.{DtoConfig.Entity}.Where(x => {keyCompare}).To{DtoClass.Name}s().FirstOrDefault();
        if (entity == null) return new Result<{DtoClass.Name}>(""内容不存在"", false);
        else return new Result<{DtoClass.Name}>(entity);
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void FirstQueryable(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyCompares = new List<string>();
            foreach (var keyId in DtoClass.Keys)
            {
                keyCompares.Add($"x.{keyId.Name} == this.{keyId.Name}");
            }
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 主键检索
    /// </summary>
    public IQueryable<{DtoConfig.Entity}> FirstQueryable({DtoConfig.Context} context)
    {{
        return context.{DtoConfig.Entity}.Where(x => {keyCompare});
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
    public Result ReLoadGen({DtoConfig.Context} context)
    {{
        var dto = FirstQueryable(context).To{DtoClass.Name}s().FirstOrDefault();
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
            foreach (var keyId in DtoClass.Keys)
            {
                keyInits.Add($"{keyId.Name} = this.{keyId.Name}");
            }
            var keyInit = keyInits.Count() > 0 ? keyInits.Aggregate((a, b) => a + ", " + b) : "";

            //新增时赋值编辑禁用的字段
            var assignCodeNew = AssignCodeDot2EntityNew(DtoClass.Properties);
            var assignCodeEdit = AssignCodeDot2EntityEdit(DtoClass.Properties);

            //赋值外键
            StringBuilder fkAssignCode = new StringBuilder();
            foreach (var fkProp in DtoClass.ParentDtos)
            {
                var foreignKey = fkProp.ForeignKeyId;
                var allowNull = fkProp.AutoCascadeSave;

                if (fkProp.AllowNull)
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}?.{foreignKey};");
                else
                    fkAssignCode.AppendLine($"        entity.{foreignKey} = this.{fkProp.Name}.{foreignKey};");
            }

            //父节点
            StringBuilder parentSaveCode = new StringBuilder();
            foreach (var prop in DtoClass.ParentDtos)
            {
                if (prop.AutoCascadeSave == true)
                {
                    parentSaveCode.AppendLine($"        this.{prop.Name}?.SaveGen(context);");
                }
            }

            //子节点保存
            StringBuilder subSaveCode = new StringBuilder();
            foreach (var prop in DtoClass.SubDtos)
            {
                if (prop.AutoCascadeSave == true)
                {
                    subSaveCode.AppendLine($"        this.{prop.Name}?.DtoSaveGen(context);");
                }
            }

            //子节点移除多余的数据
            StringBuilder subDeleteExcessCode = new StringBuilder();
            foreach (var prop in DtoClass.SubDtos)
            {
                if (prop.AutoDeleteExcess == true)
                {
                    subDeleteExcessCode.AppendLine(@$"        this.{prop.Name}?.DtoDeleteExcessGen(context, q => q.Where(x => x.{DtoClass.Key.Name} == this.{DtoClass.Key.Name}));");
                }
            }

            var code = @$"
    /// <summary>
    /// 保存
    /// </summary>
    public {DtoConfig.Entity} SaveGen({DtoConfig.Context} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            entity = new {DtoConfig.Entity}() {{ {keyInit} }};
{assignCodeNew}
            context.Add(entity);
        }}
{assignCodeEdit}

{fkAssignCode}
{parentSaveCode}{subSaveCode}{subDeleteExcessCode}
        return entity;
    }}";
            dtoBuilder.AddMethod(code);
        }

        //Delete 删除
        private void Delete(ClassCodeBuilder dtoBuilder)
        {
            //父节点
            StringBuilder parentDeleteCode = new StringBuilder();
            foreach (var prop in DtoClass.ParentDtos)
            {
                if (prop.AutoCascadeSave == true)
                {
                    parentDeleteCode.AppendLine($"        this.{prop.Name}?.DeleteGen(context);");
                }
            }

            //子节点保存
            StringBuilder subDeleteCode = new StringBuilder();
            foreach (var prop in DtoClass.SubDtos)
            {
                if (prop.AutoCascadeSave == true)
                {
                    subDeleteCode.AppendLine($"        this.{prop.Name}?.DtoDeleteGen(context);");
                }
            }

            var code = @$"
    /// <summary>
    /// 删除，基于Dto
    /// </summary>
    public bool DeleteGen({DtoConfig.Context} context)
    {{
        var entity = FirstQueryable(context).FirstOrDefault();
        if (entity == null)
        {{
            return false;
        }}

{subDeleteCode}
        context.Remove(entity);

{parentDeleteCode}
        return true;
    }}";
            dtoBuilder.AddMethod(code);
        }

        private void StaticDelete(ClassCodeBuilder dtoBuilder)
        {
            List<string> keyParameters = new List<string>();
            List<string> keyCompares = new List<string>();

            List<string> keyListParameters = new List<string>();
            List<string> keyListCompares = new List<string>();
            foreach (var keyId in DtoClass.Keys)
            {
                keyParameters.Add($"{keyId.Type.Name} {keyId.Name}");
                keyCompares.Add($"x.{keyId.Name} == {keyId.Name}");

                keyListParameters.Add($"List<{keyId.Type.Name}> {keyId.Name}s");
                keyListCompares.Add($"{keyId.Name}s.Contains(x.{keyId.Name})");
            }
            var keyParameter = keyParameters.Aggregate((a, b) => a + ", " + b);
            var keyCompare = keyCompares.Aggregate((a, b) => a + " && " + b);
            var keyListParameter = keyListParameters.Aggregate((a, b) => a + ", " + b);
            var keyListCompare = keyListCompares.Aggregate((a, b) => a + " && " + b);

            var code = @$"
    /// <summary>
    /// 删除，基于主键
    /// </summary>
    public static bool DeleteGen({DtoConfig.Context} context, {keyParameter})
    {{
        var entity = context.{DtoConfig.Entity}.Where(x => {keyCompare}).FirstOrDefault();
        if (entity == null)
        {{
            return false;
        }}
        context.Remove(entity);
        return true;
    }}

    /// <summary>
    /// 单个删除（高性能，不跟踪）
    /// </summary>
    public static bool ExecuteDeleteGen({DtoConfig.Context} context, {keyParameter})
    {{
        var change = context.{DtoConfig.Entity}.Where(x => {keyCompare}).ExecuteDelete();
        return change > 0;
    }}

    /// <summary>
    /// 批量删除（高性能，不跟踪）
    /// </summary>
    public static bool ExecuteDeleteGen({DtoConfig.Context} context, {keyListParameter})
    {{
        var change = context.{DtoConfig.Entity}.Where(x => {keyListCompare}).ExecuteDelete();
        return change > 0;
    }}
";
            dtoBuilder.AddMethod(code);
        }

        #endregion

        #region 扩展函数

        /// <summary>
        /// Dto=>Entity
        /// </summary>
        private void ToDtoExtension(ClassCodeBuilder extBuilder, StringBuilder assignCode)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// {DtoClass.Name} => {DtoConfig.Entity}
    /// Dto => Entity
    /// </summary>
    public static {DtoClass.Name} To{DtoClass.Name}(this {DtoConfig.Entity} x)
    {{
        return new {DtoClass.Name}(x)
        {{
{AssignCodeForeignKey()}
        }};
    }}");
        }

        /// <summary>
        /// IQueryable=>Dtos
        /// </summary>
        private void IQueryableToDtosExtension(ClassCodeBuilder extBuilder, StringBuilder assignCode)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// Entity => Dto
    /// {DtoConfig.Entity} => {DtoClass.Name}
    /// </summary>
    public static IQueryable<{DtoClass.Name}> To{DtoClass.Name}s(this IQueryable<{DtoConfig.Entity}> query)
    {{
        return query.Select(x => new {DtoClass.Name}(x)
        {{
{AssignCodeForeignKey()}
        }});
    }}
");

        }

        /// <summary>
        /// ICollection=>Dtos
        /// </summary>
        private void ICollectionToDtosExtension(ClassCodeBuilder extBuilder, StringBuilder assignCode)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// Entity => Dto
    /// {DtoConfig.Entity} => {DtoClass.Name}
    /// </summary>
    public static List<{DtoClass.Name}> To{DtoClass.Name}s(this ICollection<{DtoConfig.Entity}> query)
    {{
        return query.Select(x => new {DtoClass.Name}(x)
        {{
{AssignCodeForeignKey()}
        }}).ToList();
    }}");
        }

        //外联的Dto的赋值
        public string AssignCodeForeignKey()
        {
            StringBuilder assignCode = new StringBuilder();
            foreach (var prop in DtoClass.ParentDtos)
            {
                assignCode.AppendLine($"            {prop.Name} = x.{prop.ForeignTable}.To{prop.ForeignTable}Dto(),");
            }

            foreach (var prop in DtoClass.SubDtos)
            {
                assignCode.AppendLine($"            {prop.Name} = x.{prop.ForeignTable}.To{prop.ForeignTable}Dtos(),");
            }
            return assignCode.ToString();
        }


        private void DtoSaveGenExtension(ClassCodeBuilder extBuilder)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// Dto集合保存
    /// </summary>
    public static void DtoSaveGen(this List<{DtoClass.Name}> dtos, {DtoConfig.Context} context)
    {{
        dtos.ForEach(x => x.SaveGen(context));
    }}");
        }

        private void DtoDeleteGenExtension(ClassCodeBuilder extBuilder)
        {
            extBuilder.AddMethod(@$"
    /// <summary>
    /// Dto集合删除
    /// </summary>
    public static void DtoDeleteGen(this List<{DtoClass.Name}> dtos, {DtoConfig.Context} context)
    {{
        dtos.ForEach(x => x.DeleteGen(context));
    }}");
        }

        private void DtoDeleteExcessGenExtension(ClassCodeBuilder extBuilder)
        {//TODO:暂时不支持多主键
            extBuilder.AddMethod(@$"
    /// <summary>
    /// Dto集成以外的数据删除
    /// </summary>
    public static void DtoDeleteExcessGen(this List<{DtoClass.Name}> dtos, {DtoConfig.Context} context, Func<IQueryable<{DtoConfig.Entity}>, IQueryable<{DtoConfig.Entity}>> query)
    {{//暂时不支持多主键
        var ids=dtos.Select(y => y.{DtoClass.Key.Name}).ToList();
        var deleteQuery = query(context.{DtoConfig.Entity}).Where(x => ids.Contains(x.{DtoClass.Key.Name}) == false);
        context.{DtoConfig.Entity}.RemoveRange(deleteQuery);
    }}");
        }

        #endregion

    }
}
