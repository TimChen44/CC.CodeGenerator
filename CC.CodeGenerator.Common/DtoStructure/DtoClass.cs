using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Common.DtoStructure
{
    public class DtoClass
    {
        public string Name { get; set; }

        //主键
        public DtoProperty Key => Keys?.FirstOrDefault();

        public List<DtoProperty> Keys { get; set; } = new List<DtoProperty>();

        //字段
        public List<DtoProperty> Properties { get; set; } = new List<DtoProperty>();

        //父对象
        public List<DtoForeignProperty> ParentDtos { get; set; } = new List<DtoForeignProperty>();
        //子对象
        public List<DtoForeignProperty> SubDtos { get; set; } = new List<DtoForeignProperty>();


        //生成器属性，这些属性决定了这个对象需要生成那些东西
        public DtoGeneratorConfig DtoConfig;

    }
}
