using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator.Common.DtoStructure
{
    public class DtoForeignProperty : DtoProperty
    {
        /// <summary>
        /// 外键表 
        /// </summary>
        public string ForeignTable { get; set; }
        /// <summary>
        /// 外键ID
        /// </summary>
        public string ForeignKeyId { get; set; }
        /// <summary>
        /// 外键名字
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// 自动级联保存
        /// </summary>
        public bool AutoCascadeSave { get; set; }

        /// <summary>
        /// 自动级联删除列表中多余的数据
        /// </summary>
        public bool AutoDeleteExcess { get; set; }

        /// <summary>
        /// 外键链接方式,单个（父表），多个（子表）
        /// </summary>
        public ERelationType RelationType { get; set; }
    }

    public enum ERelationType
    {
        Single,
        Multiple,
    }
}
