using CC.CodeGenerator.NotifyPropertyChangeds;
using CC.CodeGenerator.NotifyPropertyChangeds.NotifyPropValidations;
namespace CC.CodeGenerator;

[Generator]
public class NotifyPropGenerator : GeneratorBase
{
    public const string attributeName = "AddNotifyPropertyChangedAttribute";
    public const string attributePath = $"CC.CodeGenerator.{attributeName}";
    public const string attributeCtor = $"{attributePath}.{attributeName}";
    private readonly NotifyPropTypeContainer typeContainer = new();

    public override void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_NotifyPropChanged
        DebuggerLaunch();
#endif
        base.Initialize(context);
    }

    protected override ISyntaxReceiver GetSyntaxReceiver() => new NotifyPropReceiver();

    protected override void Run(GeneratorExecutionContext context)
    {
        typeContainer.Clear();
        base.Run(context);
    }

    protected override void CreateAttributeCode(GeneratorExecutionContext context
        , out Compilation compilation
        , out INamedTypeSymbol attributeSymbol)
    {
        var attr = context.Compilation.GetTypeByMetadataName(attributePath);
        if (attr is not null)
        {
            compilation = context.Compilation;
            attributeSymbol = attr;
            return;
        }
        var code = @"
#pragma warning disable IDE0079
#pragma warning disable CS8632
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
        var newAttr = context.AddSourceAndParseText($"{attributeName}.cs", code);
        compilation = context.Compilation.AddSyntaxTrees(newAttr);
        attributeSymbol = compilation.GetTypeByMetadataName(attributePath)!;
    }

    protected override void MergeType(IEnumerable<NodeBase> nodes)
    {
        foreach (NotifyPropNodeBase item in nodes)
        {
            typeContainer.GetItem(item.TargetData.ContainingType).AddMember(item);
        }
    }

    protected override void BuildCode() => typeContainer.Build();
}