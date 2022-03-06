using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CC.CodeGenerator;

/// <summary>
/// 负责添加属性变更通知
/// </summary>
[Generator]
public class NotifyPropertyChangedGenerator : GeneratorBase
{
    protected override ISyntaxReceiver GetSyntaxReceiver() => new NotifyPropertyChangedReceiver();

    public override void Execute(GeneratorExecutionContext context)
    {
        //生成notifyPropertyAttribute
        var notifyPropAtt = CreateNotifyPropertyChangedAttribute(context);
        if (context.SyntaxReceiver is not NotifyPropertyChangedReceiver receiver) return;    

        //把notifyPropAtt加入当前的编译中
        var compilation = context.Compilation.AddSyntaxTrees(notifyPropAtt);

        //如果列表为空，就停止运行。
        if (receiver.IsEmpty) return;

        //获得DtoAttribute类符号
        var attSymbol = compilation.GetTypeByMetadataName("CC.CodeGenerator.AddNotifyPropertyChangedAttribute");

        //创建对应的类型并且实现接口
        receiver.Nodes.ForEach(item => CreateNotifyPropertyChanged
        (
            context,
            compilation,
            attSymbol,
            item
         ));
    }

    //创建NotifyProperty相关特性
    private SyntaxTree CreateNotifyPropertyChangedAttribute(GeneratorExecutionContext context)
    {
        var attTemplate = @$"
namespace CC.CodeGenerator;
{CreateXmlDocumentation("是否实现INotifyPropertyChanged接口")}
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class AddNotifyPropertyChangedAttribute: Attribute 
{{    
    {CreateXmlDocumentation("是否实现INotifyPropertyChanged接口", 1)}
    {CreateXmlParam("onPropChangedHandlerName", "变更方法名称，用于解决命名冲突(默认为:OnPropertyChanged)")}
    {CreateXmlParam("setPropHandlerName", "变更方法名称，用于解决命名冲突(默认为:OnPropertyChanged)")}
    public AddNotifyPropertyChangedAttribute(string setPropHandlerName = ""SetProperty""
          ,string onPropChangedHandlerName = ""OnPropertyChanged"") 
    {{        
        SetPropHandlerName = setPropHandlerName;
        OnPropertyChangedHandlerName = onPropChangedHandlerName;
    }}          

    {CreateXmlDocumentation("OnPropertyChanged函数名称", 1)}
    public string OnPropertyChangedHandlerName {{ get; }}

    {CreateXmlDocumentation("SetProperty函数名称", 1)}
    public string SetPropHandlerName {{ get; }}
}}
";
        var sourceText = SourceText.From(attTemplate, Encoding.UTF8);
        context.AddSource("NotifyPropertyChangedAttribute.cs", sourceText);
        return CSharpSyntaxTree.ParseText(sourceText);
    }

    //创建对应的类型并且实现接口
    private void CreateNotifyPropertyChanged(GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol attSymbol
        , TypeDeclarationSyntax typeDeclaration)
    {
        //获得类的类型符号
        if (compilation.GetDeclaredSymbol(typeDeclaration) is not ITypeSymbol typeSymbol) return;

        //寻找是否有NotifyPropertyAttribute
        if (!typeSymbol.IsExistsAttribute(attSymbol, out var notifyPropAttr)) return;

        //创建对应的类型
        CreateType(typeSymbol, notifyPropAttr, context);
    }

    private void CreateType(ITypeSymbol typeSymbol
        , AttributeData notifyPropAttr
        , GeneratorExecutionContext context)
    {
        var code = GetClassCode(typeSymbol, notifyPropAttr);
        var csFile = $@"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.Name}.cs";
        context.AddSource(csFile, SourceText.From(code, Encoding.UTF8));
    }

    private static (string setName,string onChangedName) GetParamValues(AttributeData notifyPropAttr)
    {
        var ctors = notifyPropAttr.ConstructorArguments;
        return (ctors[0].Value.ToString() ?? "SetProperty", ctors[1].Value.ToString() ?? "OnPropertyChanged");
    }

    private string GetClassCode(ITypeSymbol typeSymbol, AttributeData notifyPropAttr)
    {
        var declaredAccessibility = typeSymbol.DeclaredAccessibility.ToString().ToLower();
        var typeName = typeSymbol.GetTypeString();
        var (setName, onChangedName) = GetParamValues(notifyPropAttr);

        return $@"
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
{{
    {declaredAccessibility} partial {typeName} {typeSymbol.Name} : INotifyPropertyChanged
    {{

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool {setName}<T>(ref T propField, T value
            , [CallerMemberName] string? propertyName = null)
        {{
            if (EqualityComparer<T>.Default.Equals(propField, value)) return false;
            propField = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            return true;
        }}

        protected virtual void {onChangedName}(PropertyChangedEventArgs args) =>
            PropertyChanged?.Invoke(this, args);
                    
    }}
}}
";
    }
}