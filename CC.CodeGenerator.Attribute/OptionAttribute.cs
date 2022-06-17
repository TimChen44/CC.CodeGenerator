using System;

namespace CC.CodeGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AutoOptionAttribute : Attribute
{
    /// <summary>
    /// 自动创建选项
    /// </summary>
    /// <param name="fieldName">选项字段名</param>
    /// <param name="options">
    /// 可选项目,使用“代码:存储:显示”格式，采用换行或“;”分割，示例：
    /// 1:Option1:选项1
    /// 2:Option2:选项2
    /// </param>
    public AutoOptionAttribute(string fieldName, string options)
    {
        FieldName = fieldName;
        Options = options;
    }

    public string FieldName { get; set; }

    public string Options { get; set; }


    ///// <summary>
    ///// "全部"选项中文名
    ///// </summary>
    //public string AllText { get; set; }

    ///// <summary>
    ///// "无"选项中文名
    ///// </summary>
    //public string NothingText { get; set; }
}