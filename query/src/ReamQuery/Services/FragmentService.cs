namespace ReamQuery.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Text.RegularExpressions;
    using System.Text;
    using Microsoft.CodeAnalysis.Text;
    using ReamQuery.Models;

    public class FragmentService 
    {
        const string _wrapperTemplate = @"class Foo { public void wrapper() {##INPUT##}}";
        public FragmentText Fix(string input) 
        {
            var wrappedInp = _wrapperTemplate.Replace("##INPUT##", input);
            var replacements = new Dictionary<SyntaxNode, SyntaxNode>();
            var singleLineLocations = new List<int>();
            var fragment = CSharpSyntaxTree.ParseText(wrappedInp);
            var methodBlock = fragment.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .ChildNodes()
                .OfType<BlockSyntax>()
                .Single()
                ;
            
            foreach (var node in methodBlock.Statements)
            {
                var localNode = node as LocalDeclarationStatementSyntax;
                var exprNode = node as ExpressionStatementSyntax;
                SyntaxNode replacementNode = null;
                if (localNode != null && localNode.SemicolonToken.IsMissing)
                {
                    replacementNode = SyntaxFactory.LocalDeclarationStatement(
                        localNode.Modifiers,
                        localNode.Declaration, 
                        SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                    );
                }
                else if (exprNode != null && exprNode.SemicolonToken.IsMissing)
                {
                    var dumpIdent = SyntaxFactory.IdentifierName("Dump");
                    var oldExpr = !IsQueryStatement(exprNode.Expression) ? exprNode.Expression :
                        SyntaxFactory.ParenthesizedExpression(
                            SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                            exprNode.Expression,
                            SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                        ) as ExpressionSyntax;
                    var simplMA = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
                        oldExpr, SyntaxFactory.Token(SyntaxKind.DotToken), dumpIdent);
                    var dumpArgs = SyntaxFactory.ArgumentList(
                        SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(),
                        SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                    );
                    var invoc = SyntaxFactory.InvocationExpression(simplMA, dumpArgs);
                    replacementNode = SyntaxFactory.ExpressionStatement(invoc, SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    int line;
                    if (!IsMultiline(node, out line))
                    {
                        // should offset by -1 to compensate for opening parens
                        singleLineLocations.Add(line);
                    }
                }

                if (replacementNode != null)
                {
                    replacements.Add(node, replacementNode);
                }
            }
            
            var newRoot = methodBlock.ReplaceNodes(replacements.Keys, (n1, n2) => replacements[n1]);
            var openBraceRegex = new Regex(@"^\w\{", RegexOptions.Multiline);
            
            // todo print statements with trailing/leading trivia from brace tokens in methodblock
            // for now just strip out leading/trailing trivia+brace from full src
            var newFragment = Regex.Replace(newRoot.ToFullString(), @"^\w*\{", "");
            newFragment = Regex.Replace(newFragment, @"\}\w*$", "");
            return new FragmentText 
            {
                Text = newFragment,
                ExpressionLocations = singleLineLocations
            };
        }

        bool IsQueryStatement(ExpressionSyntax exprNode)
        {
            var qExpr = exprNode.ChildNodes().OfType<QueryExpressionSyntax>();
            return exprNode is QueryExpressionSyntax || qExpr.Count() > 0;
        }

        bool IsMultiline(SyntaxNode node, out int line)
        {
            var startLoc = node.GetLocation().GetLineSpan().StartLinePosition;
            if (node.HasLeadingTrivia)
            {
                startLoc = node.GetLeadingTrivia().First().GetLocation().GetLineSpan().StartLinePosition;
            }
            var endLoc = node.GetLocation().GetLineSpan().EndLinePosition;
            if (node.HasTrailingTrivia)
            {
                endLoc = node.GetTrailingTrivia().Last().GetLocation().GetLineSpan().EndLinePosition;
            }
            line = startLoc.Line;
            return startLoc.Line != endLoc.Line;
        }
    }
}
