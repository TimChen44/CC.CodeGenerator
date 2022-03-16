using CC.CodeGenerato.NotifyPropertyChangeds;
using CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;

namespace CC.CodeGenerato;

[Generator]
public class NotifyPropertyGenerator : GeneratorBase<NotifyPropertyReceiver>
{
    public const string attributeName = "AddNotifyPropertyChangedAttribute";
    public const string attributePath = $"CC.CodeGenerator.{attributeName}";
    public const string attributeCtor = $"{attributePath}.{attributeName}";

    public override void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_NotifyPropChanged
        DebuggerLaunch();
#endif
        base.Initialize(context);
    }

    protected override void Run(GeneratorExecutionContext context)
    {
        //创建特性
        CreateAttributeCode(context, out var compilation, out var attributeSymbol);

        var param = new NodeData(context, compilation, attributeSymbol);
        var typeContainer = new NotifyPropertyTypeContainer();

        //查找标记了特性的成员
        var members = GetSyntaxReceiver(context)?.Nodes
            .Select(member => TargetValidationBase.TargetFactory(param, member))
            .Where(IsTarget)
            .ToList();

        //将成员合并到相同的类型中
        members?.ForEach(member => member!.MergeType(typeContainer));

        //开始构建代码
        typeContainer.Build();
    }
  

    //创建特性,并且加入到编译中
    private void CreateAttributeCode(GeneratorExecutionContext context
        , out Compilation compilation
        , out INamedTypeSymbol attributeSymbol)
    {
        var code = @"
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
        var attr = context.AddSourceAndParseText($"{attributeName}.cs", code);
        compilation = context.Compilation.AddSyntaxTrees(attr);
        attributeSymbol = compilation.GetTypeByMetadataName(attributePath)!;
    }

}