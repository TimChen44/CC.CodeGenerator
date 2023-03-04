using CC.CodeGenerator.Common;
using CC.CodeGenerator.Common.DtoStructure;
using CC.CodeGenerator.Common.Reader;
using CC.CodeGenerator.Definition;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CC.CodeGenerator
{
    [Generator]
    public class CodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif

            //注册一个语法修改通知
            context.RegisterForSyntaxNotifications(() => new CodeSyntaxReceiver());
        }

        class CodeSyntaxReceiver : ISyntaxReceiver
        {
            //需要生成Dto操作代码的类
            public List<TypeDeclarationSyntax> CandidateClasses { get; } = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if ((syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
                    || (syntaxNode is RecordDeclarationSyntax rds && rds.AttributeLists.Count > 0)
                    )//有特性的类都进行候选，将来可以筛选出只有需要的特性的类
                {
                    CandidateClasses.Add((TypeDeclarationSyntax)syntaxNode);
                }
            }
        }


        private static readonly DiagnosticDescriptor GeneratorError = new DiagnosticDescriptor(id: "CC001",
                                                                                              title: "Dto扩展代码生成失败",
                                                                                              messageFormat: "生成Dto扩展代码发生异常 '{0}'.",
                                                                                              category: "CodeGenerator",
                                                                                              DiagnosticSeverity.Error,
                                                                                              isEnabledByDefault: true);

        public void Execute(GeneratorExecutionContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif
            try
            {
                if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(GeneratorError, Location.None, "缺少 Microsoft.EntityFrameworkCore"));
                }

                if (!(context.SyntaxReceiver is CodeSyntaxReceiver receiver)) return;
                //创建Dto扩展
                foreach (TypeDeclarationSyntax typeSyntax in receiver.CandidateClasses)
                {
                    SyntaxTreeReader reader = new SyntaxTreeReader();
                    var dtoClass = reader.AnalysisTypeDeclarationSyntax(typeSyntax);
                    if (dtoClass == null) continue;
                    DtoCodeGen ctoCodeGen = new DtoCodeGen(dtoClass);
                    var genCode = ctoCodeGen.GenCode();
                    context.AddSource($"{dtoClass.DtoConfig.DtoNamespace}.{dtoClass.Name}.g.cs", SourceText.From(genCode, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(GeneratorError, Location.None, ex.ToString()));
            }

        }
    }
}
