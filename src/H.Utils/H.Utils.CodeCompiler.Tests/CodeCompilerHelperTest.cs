using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;

namespace H.Utils.CodeCompiler.Tests
{
    public class CodeCompilerHelperTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "调用动态创建的脚本方法")]
        public void CallScriptFromText()
        {
            string code = @"
                public class ScriptedClass
                {
                    public string HelloWorld { get; set; }
                    public ScriptedClass()
                    {
                        HelloWorld = ""Hello Roslyn!"";
                    }
                }";
            var script = CSharpScript.RunAsync(code).Result;

            var result = script.ContinueWithAsync<string>("new ScriptedClass().HelloWorld").Result;

            Assert.Equal("Hello Roslyn!", result.ReturnValue);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="x"></param>
        //[Fact]
        //[Trait("desc", "使用类的实例调用类的带参数的方法，并获取返回值")]
        //[InlineData("123")]
        //public void CallScriptFromAssemblyWithArgument(string x)
        //{
        //    var script = CSharpScript.Create<string>("return new TestClass().DealString(arg1);",
        //        ScriptOptions.Default
        //        .WithReferences(typeof(TestClass).Assembly)
        //        .WithImports("Test.Standard.DynamicScript"), globalsType: typeof(TestClass));

        //    script.Compile();

        //    var result = script.RunAsync(new TestClass { arg1 = x }).Result.ReturnValue;

        //    Assert.Equal(x, result.ToString());
        //}
    }
}