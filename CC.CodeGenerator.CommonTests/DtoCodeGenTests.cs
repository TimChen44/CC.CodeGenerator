//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using CC.CodeGenerator.Common;
//using System;
//using System.Collections.Generic;
//using System.Text;

//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis;
//using CC.CodeGenerator.Common.Reader;
//using CC.CodeGenerator.Common.DtoStructure;

//namespace CC.CodeGenerator.Common.Tests
//{
//    [TestClass()]
//    public class DtoCodeGenTests
//    {
//        [TestMethod]
//        public void SyntaxTreeReader_Analysis_CompanyDtoTest()
//        {
//            var dtoClasses = SyntaxTreeReader_AnalysisTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\CompanyDto.cs");

//            Assert.AreEqual(dtoClasses?.Count, 1);
//        }

//        [TestMethod]
//        public void SyntaxTreeReader_Analysis_PersonnelDtoTest()
//        {
//            var dtoClasses = SyntaxTreeReader_AnalysisTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\PersonnelDto.cs");
//            Assert.AreEqual(dtoClasses?.Count, 1);
//        }

//        [TestMethod]
//        public void SyntaxTreeReader_Analysis_AchievementsDtoTest()
//        {
//            var dtoClasses = SyntaxTreeReader_AnalysisTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\AchievementsDto.cs");
//            Assert.AreEqual(dtoClasses?.Count, 1);
//        }

//        //分析代码
//        private List<DtoClass> SyntaxTreeReader_AnalysisTest(string dtoFileName)
//        {
//            var code = System.IO.File.ReadAllText(dtoFileName);
//            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
//            SyntaxTreeReader reader = new SyntaxTreeReader();
//            return reader.AnalysisTypeDeclarationSyntax(tree.GetRoot());
//        }

//        [TestMethod]
//        public void DtoCodeGen_GenCode_CompanyDtoTest()
//        {
//            DtoCodeGen_GenCodeTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\CompanyDto.cs");
//            Assert.IsTrue(true);
//        }

//        [TestMethod]
//        public void DtoCodeGen_GenCode_PersonnelDtoTest()
//        {
//            DtoCodeGen_GenCodeTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\PersonnelDto.cs");
//            Assert.IsTrue(true);
//        }

//        [TestMethod]
//        public void DtoCodeGen_GenCode_AchievementsDtoTest()
//        {
//            DtoCodeGen_GenCodeTest(@"..\..\..\..\CC.DevelopmentKit.DemoTest\AchievementsDto.cs");
//            Assert.IsTrue(true);
//        }

//        //生成代码
//        private void DtoCodeGen_GenCodeTest(string dtoFileName)
//        {
//            var dtoClasses = SyntaxTreeReader_AnalysisTest(dtoFileName);

//            foreach (var dtoClass in dtoClasses)
//            {
//                DtoCodeGen ctoCodeGen = new DtoCodeGen(dtoClass);
//                var genCode = ctoCodeGen.GenCode();

//                if (genCode.Contains("生成代码发生错误")) Assert.Fail();

//                var outFileName = "";
//                if (dtoClasses.Count == 1)
//                {
//                    outFileName = Path.Combine(Path.GetDirectoryName(dtoFileName), $"{Path.GetFileNameWithoutExtension(dtoFileName)}.g.cs");
//                }
//                else
//                {
//                    outFileName = Path.Combine(Path.GetDirectoryName(dtoFileName), $"{Path.GetFileNameWithoutExtension(dtoFileName)}.{dtoClass.Name}.g.cs");
//                }
//                if (System.IO.File.Exists(outFileName) == false || System.IO.File.ReadAllText(outFileName) != genCode)
//                    File.WriteAllText(outFileName, genCode);
//            }
//        }
//    }
//}