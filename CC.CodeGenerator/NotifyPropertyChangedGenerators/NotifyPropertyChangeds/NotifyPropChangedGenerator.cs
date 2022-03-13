#nullable enable
using CC.CodeGenerator.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.Infos;

namespace CC.CodeGenerator;

[Generator]
public class NotifyPropChangedGenerator : GeneratorBase<NotifyPropChangedReceiver>
{
    public override void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_NotifyPropChanged
        DebuggerLaunch();
#endif
        base.Initialize(context);
    }

    protected override void Execute(GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol? attributeSymbol)
    {
        if (attributeSymbol is null) return;
        var codeManager = new NotifyPropChangedCodeManager();
        GetSyntaxReceiver(context)?.Nodes.ToList()
            .ForEach(member => CodeHandler(member, context, compilation, attributeSymbol, codeManager));
        codeManager.CreateCode(context);
    }

    private void CodeHandler(MemberDeclarationSyntax member
        , GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol attributeSymbol
        , NotifyPropChangedCodeManager codeManager)
    {
        try
        {
            MemberInfoBase? data = member switch
            {
                FieldDeclarationSyntax fds => new FieldInfo(fds),
                ClassDeclarationSyntax or RecordDeclarationSyntax => new TypeInfo(member),
                _ => default
            };

            data?.SetValue(context, compilation, attributeSymbol, codeManager).ParsingCode();
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(member, DiagnosticSeverity.Error, "ShadowCodeError", ex.Message);
        }
    }

    protected override (string fileName, string? code) GetAttributeCode(GeneratorExecutionContext context
        , out string attributeFullName)
{
        attributeFullName = MemberInfoBase.attributePath;
        var code = @"
#nullable enable
namespace CC.CodeGenerator
{
    /// <summary>
    /// 创建具有变更通知的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
    internal class AddNotifyPropertyChangedAttribute : Attribute
    {
        /// <summary>
        /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
        /// </summary>
        public AddNotifyPropertyChangedAttribute() { }

        /// <summary>
        /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
        /// </summary>
        /// <param name=""propertyName"">属性名称</param>
        public AddNotifyPropertyChangedAttribute(string propertyName) =>
            PropertyName = propertyName;

        /// <summary>
        /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
        /// </summary>
        /// <param name=""propertyName"">属性名称</param>
        /// <param name=""propertyType"">属性类型</param>
        public AddNotifyPropertyChangedAttribute(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string? PropertyName { get; }


        /// <summary>
        /// 属性类型
        /// </summary>
        public Type? PropertyType { get; }


        /// <summary>
        /// SetProperty方法的名称。
        /// <br/> 用于解决命名冲突。(仅用于类型)
        /// </summary>
        public string? SetPropertyMethodName { get; set; }

        /// <summary>
        /// OnPropertyChanged方法的名称。
        /// <br/> 用于解决命名冲突。(仅用于类型)
        /// </summary>
        public string? OnPropertyChangedMethodName { get; set; }

        /// <summary>
        /// 生成xml文档的字符串。
        /// </summary>
        public string? XmlSummary { get; set; }
    }   
}
";
        return ($"{MemberInfoBase.attributeName}.cs", code);
    }
}