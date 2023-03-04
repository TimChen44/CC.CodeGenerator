using CC.CodeGenerator.Common.DtoStructure;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CC.CodeGenerator.Common.Reader
{
    public interface IReader
    {
        List<DtoClass> Analysis(SyntaxNode syntaxTree);
    }
}