using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common.DtoStructure
{
    public class CSharpPropertyType : PropertyType
    {
        public CSharpPropertyType(string? typeString)
        {
            OriginalName = typeString;
            OriginalSource = EOriginalSource.CSharp;

            switch (typeString)
            {
                case "string":
                case "Guid":
                case "int":
                case "long":
                case "bool":
                case "DateTime":
                case "decimal":
                case "double":
                    Name = typeString;
                    IsDataType = true;
                    break;
                default:
                    Name = typeString;
                    IsDataType = false;
                    break;
            }
        }
    }

    public class MSSQLPropertyType : PropertyType
    {
        public MSSQLPropertyType(string typeString)
        {
            OriginalName = typeString;
            OriginalSource = EOriginalSource.MSSQL;

            Name = typeString.ToLower() switch
            {
                "varchar" => "string",
                "nvarchar" => "string",
                "char" => "string",
                "nchar" => "string",
                "uniqueidentifier" => "Guid",
                "int" => "int",
                "bigint" => "long",
                "bit" => "bool",
                "datetime" => "DateTime",
                "date" => "DateTime",
                "decimal" => "decimal",
                "float" => "double",
                _ => typeString,
            };
        }
    }


    public abstract class PropertyType
    {
        /// <summary>
        /// 类型:以C#类型为模板
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 是否是数据类型，如果是string，bool，guid等数据类型时为True
        /// </summary>
        public bool IsDataType { get; set; }

        /// <summary>
        /// 原始类型
        /// </summary>
        public string? OriginalName { get; set; }

        /// <summary>
        /// 原始来源
        /// </summary>
        public EOriginalSource OriginalSource { get; set; }

        public string CSharpName => Name;

        public string MSSQLName
        {
            get
            {
                if (OriginalSource == EOriginalSource.MSSQL) return OriginalName;
                return Name switch
                {
                    "string" => "nvarchar",
                    "Guid" => "uniqueidentifier",
                    "int" => "int",
                    "long" => "bigint",
                    "bool" => "bit",
                    "DateTime" => "datetime",
                    "decimal" => "decimal",
                    "double" => "float",
                    _ => Name,
                };
            }
        }

    }
    public enum EOriginalSource
    {
        MSSQL,
        CSharp,
    }
}
