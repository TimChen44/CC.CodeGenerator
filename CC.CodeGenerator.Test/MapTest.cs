using CC.CodeGenerator.Demo.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Test
{
    [TestClass]
    public class MapTest
    {


        [TestMethod]
        public void CopyTo()
        {
            var people1Map = new People1Map()
            {
                PeopleId = Guid.NewGuid(),
                Name = "Tim",
                Age = 10,
            };
            var people2Map = new People2Map();
            people1Map.CopyTo(people2Map);

            Assert.AreNotEqual(people1Map.PeopleId, people2Map.PeopleId);
            Assert.AreEqual(people1Map.Name, people2Map.Name);
            Assert.AreEqual(people1Map.Age, people2Map.Age);

            var people3Map = new People3Map() { CityTitle = "ShangHai" };
            people1Map.CopyTo(people3Map);

            Assert.AreEqual(people1Map.CityTitle, people3Map.CityTitle);
            Assert.AreNotEqual(people1Map.Disply, people3Map.Disply);
        }


        [TestMethod]
        public void CopyFrom()
        {
            var people1Map = new People1Map()
            {
                PeopleId = Guid.NewGuid(),
                Name = "Tim",
                Age = 10,
            };
            var people2Map = new People2Map()
            {
                PeopleId = Guid.NewGuid(),
                Name = "Chen",
                Age = 20,
            };

            people1Map.CopyFrom(people2Map);

            Assert.AreEqual(people1Map.PeopleId, people2Map.PeopleId);
            Assert.AreEqual(people1Map.Name, people2Map.Name);
            Assert.AreNotEqual(people1Map.Age, people2Map.Age);

            var people3Map = new People3Map() { CityTitle = "ShangHai" };
            people1Map.CopyFrom(people3Map);

            Assert.AreEqual(people1Map.CityTitle, people3Map.CityTitle);
            Assert.AreNotEqual(people1Map.Disply, people3Map.Disply);
        }

        [TestMethod]
        public void Construction()
        {
            var people2Map = new People2Map()
            {
                PeopleId = Guid.NewGuid(),
                Name = "Chen",
                Age = 20,
            };
            var people1Map = new People1Map(people2Map); 

            Assert.AreEqual(people1Map.PeopleId, people2Map.PeopleId);
            Assert.AreEqual(people1Map.Name, people2Map.Name);
            Assert.AreNotEqual(people1Map.Age, people2Map.Age);
        }


    }

    [Mapping(typeof(People2Map), typeof(People3Map))]
    public partial class People1Map
    {

        public Guid PeopleId { get; set; }
        public string Name { get; set; }
        [MappingIgnore]
        public int Age { get; set; }
        public string CityTitle { get; set; }
        public string Disply => $"{Name}";
    }

    public class People2Map
    {
        [MappingIgnore]
        public Guid PeopleId { get; set; }
        public string Name { get; set; }

        public int Age { get; set; }
    }

    public class People3Map
    {
        public string CityTitle { get; set; }
        public string Disply { get; set; }
    }


}
