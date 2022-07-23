﻿using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CC.CodeGenerator
{
    /// <summary>
    /// 类代码构造
    /// </summary>
    public class ClassCodeBuilder
    {
        public ITypeSymbol TypeSymbol { get; }

        private List<string> Usings { get; set; } = new List<string>();

        private List<string> Methods { get; set; } = new List<string>();

        private List<string> Constructors { get; set; } = new List<string>();


        public ClassCodeBuilder(ITypeSymbol typeSymbol, string classType)
        {
            TypeSymbol = typeSymbol;
            ClassName = classType;
        }

        public void AddUsing(string code)
        {
            if (Usings.Contains(code.Trim())) return;
            Usings.Add(code);
        }
        public void AddConstructor(string code)
        {
            Constructors.Add(code);
        }


        public void AddMethod(string code)
        {
            Methods.Add(code);
        }

        public override string ToString()
        {
            //类的类型
            var typeName = TypeSymbol.IsRecord ? "record" : "class";
            var usingsCode = Usings.Count > 0 ? Usings.Aggregate((a, b) => a + "\r\n" + b) : "";
            var methodsCode = Methods.Count > 0 ? Methods.Aggregate((a, b) => a + "\r\n\r\n" + b) : "";
            var constructorCode = Constructors.Count > 0 ? Constructors.Aggregate((a, b) => a + "\r\n\r\n" + b) : "";

            //组装代码
            string dtoCode = @$"
using CC.Core;
{usingsCode}

namespace {TypeSymbol.ContainingNamespace.ToDisplayString()};

public partial {(IsStatic ? "static" : "")} {typeName} {TypeSymbol.Name}
{{
{constructorCode}

{methodsCode}
}}
";
            return dtoCode;
        }

        //是否静态
        public bool IsStatic { get; set; } = false;

        //名字
        public string ClassName { get; }

        /// <summary>
        /// 写入代码
        /// </summary>
        /// <param name="context"></param>
        public void WriteCode(GeneratorExecutionContext context)
        {
            if (Usings.Count > 0 && Methods.Count > 0 && Constructors.Count > 0)
            {
                context.AddSource($"{TypeSymbol?.ContainingNamespace.ToDisplayString()}.{TypeSymbol?.Name}.{ClassName}.g.cs",
                    SourceText.From(this.ToString(), Encoding.UTF8));
            }
        }

        #region 公用方法

        public StringBuilder AssignCode(string leftName, IEnumerable<PropertyData> leftProps,
            string rightName, IEnumerable<PropertyData> rightProps, string separate)
        {
            return AssignCode(leftName, leftProps.Select(x => x.Property), rightName, rightProps.Select(x => x.Property), separate);
        }

        public StringBuilder AssignCode(string leftName, IEnumerable<PropertyData> leftProps,
            string rightName, IEnumerable<IPropertySymbol> rightProps, string separate)
        {
            return AssignCode(leftName, leftProps.Select(x => x.Property), rightName, rightProps, separate);
        }

        public StringBuilder AssignCode(string leftName, IEnumerable<IPropertySymbol> leftProps,
            string rightName, IEnumerable<PropertyData> rightProps, string separate)
        {
            return AssignCode(leftName, leftProps, rightName, rightProps.Select(x => x.Property), separate);
        }

        /// <summary>
        /// 赋值代码
        /// </summary>
        public StringBuilder AssignCode(string leftName, IEnumerable<IPropertySymbol> leftProps,
            string rightName, IEnumerable<IPropertySymbol> rightProps, string separate)
        {
            var code = new StringBuilder();
            foreach (var leftProp in leftProps)
            {
                if (leftProp.IsReadOnly) continue;
                var rightProp = rightProps.FirstOrDefault(x => x.Name == leftProp.Name);
                if (rightProp == null) continue;
                code.AppendLine($"        {(string.IsNullOrWhiteSpace(leftName) ? "" : $"{leftName}.")}{leftProp.Name} = {rightName}.{rightProp.Name}{separate}");
            }
            return code;
        }

        #endregion
    }
}
