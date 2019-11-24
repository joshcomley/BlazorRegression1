using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorApp3
{
    public class GenericNameFetcher : CSharpSyntaxWalker
    {
        public override void VisitGenericName(GenericNameSyntax node)
        {
            GenericName = node;
            base.VisitGenericName(node);
        }

        public GenericNameSyntax GenericName { get; set; }
    }

    public class Class
    {
        public static async Task Boot()
        {
            var tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(@"using System.Collections.Generic;
    public class Dictionaries
    {
        public void Method1()
        {
            var dic = typeof(Dictionary<string, int>);
        }
    }");
            var bytes = await new HttpClient().GetByteArrayAsync(
                $"http://localhost:62902/_framework/_bin/{typeof(Dictionary<,>).Assembly.Location}");
            var mscorlib = MetadataReference.CreateFromImage(bytes);
            var compilation = CSharpCompilation
                .Create("TempComp")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(mscorlib)
                .AddSyntaxTrees(tree);
            var root = tree.GetRoot();
            var cv = new GenericNameFetcher();
            cv.Visit(root);
            tree = (CSharpSyntaxTree)root.SyntaxTree;
            var model = compilation.GetSemanticModel(tree);
            var s = model.GetSymbolInfo(cv.GenericName).Symbol;
            Console.WriteLine(s.ToDisplayString());
        }
    }
}
