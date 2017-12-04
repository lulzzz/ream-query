namespace ReamQuery.Server.Services
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
    using ReamQuery.Server.Models;
    using NLog;

    public class FragmentService 
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        const string _wrapperTemplate = @"class Foo { public void wrapper() {##INPUT##}}";
        public FragmentText Fix(string input) 
        {
            Logger.Info("Input fragment {0}", input);
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
                    var oldExpr = SyntaxFactory.ParenthesizedExpression(
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
                    singleLineLocations.Add(GetLine(node));
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
            Logger.Info("Transformed fragment {0}", newFragment);
            return new FragmentText 
            {
                Text = newFragment,
                ExpressionLocations = singleLineLocations
            };
        }

        int GetLine(SyntaxNode node)
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
            return startLoc.Line;
        }
    }
}
