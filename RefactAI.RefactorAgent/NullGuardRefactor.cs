using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactAI.RefactorAgent
{
    public static class NullGuardRefactor
    {
        public static async Task<Document> ApplyAsync(Document doc, CancellationToken ct)
        {
            var root = await doc.GetSyntaxRootAsync(ct) as CompilationUnitSyntax;
            var model = await doc.GetSemanticModelAsync(ct);

            var rewriter = new NullGuardRewriter(model!);
            var newRoot = rewriter.Visit(root!);
            return doc.WithSyntaxRoot(newRoot);
        }
    }

    class NullGuardRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;
        public NullGuardRewriter(SemanticModel model) => _model = model;

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Skip abstract, interface, or extern methods (no body to modify)
            if (node.Body == null)
                return base.VisitMethodDeclaration(node);

            var guards = new List<StatementSyntax>();

            foreach (var p in node.ParameterList.Parameters)
            {
                // Skip parameters without type (e.g., interface declarations)
                if (p.Type == null)
                    continue;

                var t = _model.GetTypeInfo(p.Type).Type;

                // Only add guards for non-nullable reference types
                if (t?.IsReferenceType == true && !p.Type.ToString().EndsWith("?"))
                {
                    var stmt = SyntaxFactory.ParseStatement(
                        $"if ({p.Identifier.ValueText} is null) " +
                        $"throw new ArgumentNullException(nameof({p.Identifier.ValueText}));\n"
                    );
                    guards.Add(stmt);
                }
            }

            if (guards.Count == 0)
                return base.VisitMethodDeclaration(node);

            var newBody = node.Body.WithStatements(node.Body.Statements.InsertRange(0, guards));
            return node.WithBody(newBody);
        }

    }
}
