//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace CC.CodeGenerator;

//[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
//public class MappingAttribute : Attribute
//{
//    public MappingAttribute(params Type[] targets)
//    {
//        Targets = targets;
//    }

//    /// <summary>
//    /// 目标类型，当前对象与目标对象进行Mapping
//    /// </summary>
//    public Type[] Targets { get; set; }

//}


//[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
//public class MappingIgnoreAttribute : Attribute
//{
//    public MappingIgnoreAttribute()
//    {
//    }
//}