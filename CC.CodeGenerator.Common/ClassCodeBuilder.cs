using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common
{
    public class ClassCodeBuilder
    {
        public List<string> Usings { get; set; } = new List<string>();
        public void AddUsing(string code)
        {
            if (Usings.Contains(code.Trim())) return;
            Usings.Add(code.Trim());
        }
        public string BuildUsing()
        {
            if (Usings.Count == 0) return "";
            return Usings.Aggregate((a, b) => a + "\r\n" + b);
        }

        public List<string> Constructors { get; set; } = new List<string>();
        public void AddConstructor(string code)
        {
            Constructors.Add(code);
        }
        public string BuildConstructors()
        {
            if (Constructors.Count == 0) return "";
            return Constructors.Aggregate((a, b) => a + "\r\n" + b);
        }

        public List<string> Methods { get; set; } = new List<string>();
        public void AddMethod(string code)
        {
            Methods.Add(code);
        }
        public string BuildMethods()
        {
            if (Methods.Count == 0) return "";
            return Methods.Aggregate((a, b) => a + "\r\n" + b);
        }
    }
}
