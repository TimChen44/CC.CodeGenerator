using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.PackageDemo
{
    [Mapping(typeof(People2Map), typeof(People3Map))]
    public partial class People1Map
    {
        public Guid PeopleId { get; set; } 
        public string Name { get; set; }


        public string CityTitle { get; set; }

        public string Disply => $"{Name}";

    }

    public class People2Map
    {
        public Guid PeopleId { get; set; }
        public string Name { get; set; }
    }

    public class People3Map
    {
        public string CityTitle { get; set; }

    } 
}

