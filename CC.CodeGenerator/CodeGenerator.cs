﻿using CC.CodeGenerator.Builder;
using CC.CodeGenerator.Definition;
using System;
using System.Collections.Generic;
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

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is CodeSyntaxReceiver receiver)) return;

            //把DtoAttribute加入当前的编译中
            Compilation compilation = context.Compilation;

            LoadTool loadTool = new LoadTool(compilation);

            //创建Dto扩展
            foreach (TypeDeclarationSyntax typeSyntax in receiver.CandidateClasses)
            {
                //获得类的类型符号,如果无法获得，就跳出
                if (compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetDeclaredSymbol(typeSyntax) is not ITypeSymbol typeSymbol) return;

                var typeData = loadTool.CreateTypeData(typeSymbol);

                //扩展函数
                var extBuilder = new ExtBuilder(typeSymbol,"ext", typeData);
                context.AddSource(extBuilder.FileName, extBuilder.SourceText);

                //实体操作
                var dtoBuilder = new DtoBuilder(typeSymbol,"dto",typeData);
                context.AddSource(dtoBuilder.FileName, dtoBuilder.SourceText);

            }

        }
    }
}