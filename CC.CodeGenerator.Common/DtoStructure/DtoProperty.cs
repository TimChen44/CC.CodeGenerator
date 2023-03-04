using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common.DtoStructure
{
    public class DtoProperty
    {
        public PropertyType Type { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 是否允许空
        /// </summary>
        public bool AllowNull { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 只读属性
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 禁止编辑
        /// </summary>
        public bool IsEditDisable { get; set; }
    }


}
