using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common.DtoStructure
{
    public class DtoGeneratorConfig
    {
        /// <summary>
        /// Dto的命名空间
        /// </summary>
        public string DtoNamespace { get; set; }

        /// <summary>
        /// Dto对象类型
        /// </summary>
        public string DtoType { get; set; } = "class";

        /// <summary>
        /// 实体命名空间。默认为DtoNamespace
        /// </summary>
        public string EntityNamespace { get; set; }

        public string EntityNamespaceString => string.IsNullOrEmpty(EntityNamespace) ? "" : $"{EntityNamespace}.";

        /// <summary>
        /// EF Core上下文名字
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// EF 实体类型
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// 存在无参构造函数，如果存在生成代码时不生成默认构造函数
        /// </summary>
        public bool HasDefaultConstructor { get; set; }
    }


}
