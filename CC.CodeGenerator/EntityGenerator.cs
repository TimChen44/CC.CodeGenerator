//namespace CC.CodeGenerator;

//[Generator]
//public class EntityGenerator : ISourceGenerator
//{
//    public void Initialize(GeneratorInitializationContext context)
//    {
//#if DEBUG
//        //if (!Debugger.IsAttached)
//        //{
//        //    Debugger.Launch();
//        //}
//#endif

//        //注册一个语法修改通知
//        context.RegisterForSyntaxNotifications(() => new DtoSyntaxReceiver());
//    }

//    class DtoSyntaxReceiver : ISyntaxReceiver
//    {
//        //需要生成Dto操作代码的类
//        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

//        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//        {
//            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
//            {
//                CandidateClasses.Add((ClassDeclarationSyntax)syntaxNode);
//            }
//        }
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        if (!(context.SyntaxReceiver is DtoSyntaxReceiver receiver))
//        {
//            return;
//        }

//        //把DtoAttribute加入当前的编译中
//        Compilation compilation = context.Compilation;


//        //获得AutoOptionAttribute类符号
//        INamedTypeSymbol optionAttSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.AutoOptionAttribute");

//        foreach (TypeDeclarationSyntax classSyntax in receiver.CandidateClasses)
//        {
//            if (compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax) is not ITypeSymbol classSymbol) continue;

//            var optionDatas = classSymbol.GetAttributes().Where(x => x.AttributeClass.Equals(optionAttSymbol, SymbolEqualityComparer.Default)).ToList();

//            if (optionDatas.Count == 0) continue;

//            StringBuilder entityBuilder = new StringBuilder();

//            foreach (var optionData in optionDatas)
//            {
//                entityBuilder.AppendLine(CreateOption(context, optionData));
//                entityBuilder.AppendLine();
//            }

//            //组装代码
//            string entityCode = @$"using CC.Core;
//using System.ComponentModel;

//namespace {classSymbol.ContainingNamespace.ToDisplayString()};

//public partial class {classSymbol.Name}
//{{
//{entityBuilder}
//}}
//";
//            context.AddSource($@"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}.entity.g.cs", SourceText.From(entityCode, Encoding.UTF8));
//        }
//    }

//    //创建注入代码
//    public string CreateOption(GeneratorExecutionContext context, AttributeData optionData)
//    {
//        try
//        {
//            var fieldName = optionData.ConstructorArguments[0].Value.ToString();
//            var options = optionData.ConstructorArguments[1].Value.ToString();
//            if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(options)) return "";

//            /// 代码:存储:显示
//            /// Option1:1:选项1

//            var optionLines = options.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//            StringBuilder classCode = new StringBuilder();
//            StringBuilder optionCode = new StringBuilder();
//            StringBuilder filterCode = new StringBuilder();

//            foreach (var line in optionLines)
//            {
//                var optionItems = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();

//                if (optionItems.Count == 0) continue;
//                if (optionItems.Count == 1) optionItems.Add(optionItems[0]);
//                if (optionItems.Count == 2) optionItems.Add(optionItems[0]);

//                classCode.AppendLine($@"        [DisplayName(""{optionItems[2]}"")]
//        public static string {optionItems[0]} {{ get; set; }} = ""{optionItems[1]}"";");

//                optionCode.AppendLine($@"        new OptionCore(""{optionItems[1]}"",""{optionItems[2]}""),");

//                filterCode.AppendLine($@"        new OptionCore(""{optionItems[1]}"",""{optionItems[2]}""),");
//            }

//            string code = @$"
//    public class E{fieldName}
//    {{
//{classCode}
//    }}

//    public static List<OptionCore> E{fieldName}Option {{get; }} = new List<OptionCore>()
//    {{
//{optionCode}
//    }};

//    public static List<OptionCore> E{fieldName}Filter {{get; }} = new List<OptionCore>()
//    {{
//        new OptionCore("""",""全部""),
//{filterCode}
//    }};
//";
//            return code;
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine(ex.ToString());
//            return "";
//        }

//    }
//}

