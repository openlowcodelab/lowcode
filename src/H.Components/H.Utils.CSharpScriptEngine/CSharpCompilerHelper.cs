using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Immutable;
using System.Reflection;

namespace H.Utils.CodeCompiler
{
    public static class CSharpCompilerHelper
    {
        public static void Execute(string code, string className, string methodName, object[] args)
        {
            Assembly assembly = GenerateAssemblyFromCode(code);

            if(assembly == null)
                throw new InvalidOperationException(nameof(assembly));

            // 反射获取程序集中 的类
            Type type = assembly.GetType(className);

            if(type == null)
                throw new InvalidOperationException(nameof(type));

            // 创建该类的实例
            object obj = Activator.CreateInstance(type);

            // 通过反射方式调用类中的方法。（Hello World 便是我们传入方法的参数）
            type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod, null, obj, args);
        }

        /// <summary>
        /// 动态编译
        /// </summary>
        /// <param name="code">需要动态编译的代码</param>
        /// <returns>动态生成的程序集</returns>
        private static Assembly GenerateAssemblyFromCode(string code)
        {
            //解析代码文本
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            //获取随机程序集名称
            string assemblyName = Path.GetRandomFileName();

            //获取程序集依赖
            var references = AppDomain.CurrentDomain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location));

            //创建编译对象
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree);

            ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            bool error = false;
            foreach (Diagnostic diag in diagnostics)
            {
                switch (diag.Severity)
                {
                    case DiagnosticSeverity.Info:
                        Console.WriteLine(diag.ToString());
                        continue;
                    case DiagnosticSeverity.Warning:
                        Console.WriteLine(diag.ToString());
                        continue;
                    case DiagnosticSeverity.Error:
                        error = true;
                        Console.WriteLine(diag.ToString());
                        break;
                }
            }

            if (error)
            {
                throw new InvalidOperationException($"代码编译错误，code:\n{code}");
            }

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                return Assembly.Load(ms.ToArray());
            }
        }
    }
}