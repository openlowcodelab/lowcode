using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace H.Utils.CodeCompiler
{
    public static class CodeCompilerHelper
    {
        public static void Execute(string code, string className, string methodName, object[]? args)
        {
            Assembly assembly = GenerateAssemblyFromCode(code);

            // 反射获取程序集中 的类
            Type type = assembly.GetType(className);

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
            Assembly assembly = null;
            // 丛代码中转换表达式树
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            // 随机程序集名称
            string assemblyName = Path.GetRandomFileName();
            // 引用
            var references = AppDomain.CurrentDomain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location));

            // 创建编译对象
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // 将编译好的IL代码放入内存流
                EmitResult result = compilation.Emit(ms);

                // 编译失败，提示
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                                diagnostic.IsWarningAsError ||
                                diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    // 编译成功，从内存中加载编译好的程序集
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
            }
            return assembly;
        }
    }
}