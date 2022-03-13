#nullable enable
using CC.CodeGenerator.Receivers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace CC.CodeGenerator
{
    public abstract class GeneratorBase : ISourceGenerator
    {
        public virtual void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(GetSyntaxReceiver);
        }

        protected void DebuggerLaunch()
        {
            if (!Debugger.IsAttached) Debugger.Launch();
        }

        /// <summary>
        /// 返回SyntaxNode(默认情况下返回带有特性的class或recor)
        /// </summary>
        protected virtual ISyntaxReceiver GetSyntaxReceiver() => new ReceiverDefault();

        public virtual void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var compilation = context.Compilation;

                //将特性添加到当前的编译中
                var attr = CreateAttribute(context, out var attributeFullName);
                if (attr is not null)
                    compilation = compilation.AddSyntaxTrees(attr);

                var attSymbol = compilation.GetTypeByMetadataName(attributeFullName);
                Execute(context, compilation, attSymbol);
            }
            catch (Exception ex)
            {
                CreateErrorFile(context, ex);              
            }           
        }

        private void CreateErrorFile(GeneratorExecutionContext context, Exception ex)
        {
            var error = $@"
/*
{ex}
*/
";
            var fileName = $"_Error{GetType().Name}.cs";
            context.AddSource(fileName, error);
        }

        protected abstract void Execute(GeneratorExecutionContext context
            , Compilation compilation, INamedTypeSymbol? attributeSymbol);

        /// <summary>
        /// 创建特性
        /// </summary>
        protected virtual SyntaxTree? CreateAttribute(GeneratorExecutionContext context, out string attributeFullName)
        {
            var (fileName, code) = GetAttributeCode(context, out attributeFullName);
            if (code is not { Length: > 0 }) return default;
            var sourceText = SourceText.From(code, Encoding.UTF8);
            context.AddSource(fileName, sourceText);
            return CSharpSyntaxTree.ParseText(sourceText);
        }

        /// <summary>
        /// 返回特性的代码
        /// </summary>
        /// <returns>code 代码<br/>fileName 文件名</returns>
        protected abstract (string fileName, string? code) GetAttributeCode(GeneratorExecutionContext context, out string attributeFullName);
    }

    public abstract class GeneratorBase<T> : GeneratorBase where T : class, ISyntaxReceiver, new()
    {
        protected override ISyntaxReceiver GetSyntaxReceiver() => new T();

        protected T? GetSyntaxReceiver(GeneratorExecutionContext context) => context.SyntaxReceiver as T;
    }

}
