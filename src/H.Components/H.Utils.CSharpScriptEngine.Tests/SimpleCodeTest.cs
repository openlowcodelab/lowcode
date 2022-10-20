using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;

namespace H.Utils.CodeCompiler.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleCodeTest
    {
        /// <summary>
        /// 无第三方程序集依赖的简单代码单测
        /// </summary>
        [Fact]
        [Trait("desc", "属性值获取")]
        public void SimpleCodeTest0()
        {
            string code = @"
                public class SampleClass
                {
                    public string HelloWorld { get; set; }
                    public SampleClass()
                    {
                        HelloWorld = ""Hello Roslyn!"";
                    }
                }";
            var script = CSharpScript.RunAsync(code).Result;

            var result = script.ContinueWithAsync<string>("new SampleClass().HelloWorld").Result;

            Assert.Equal("Hello Roslyn!", result.ReturnValue);
        }

        /// <summary>
        /// 无第三方程序集依赖的简单代码单测
        /// </summary>
        [Fact]
        [Trait("desc", "实例类-实例方法-无参-无返回值")]
        public void SimpleCodeTest1()
        {
            string code = @"
                public class SampleClass
                {
                    public SampleMethod()
                    {
                    }
                }";
            var script = CSharpScript.RunAsync(code).Result;

            var result = script.ContinueWithAsync<string>("new SampleClass().SampleMethod()").Result;

            Assert.Equal("Hello Roslyn!", result.ReturnValue);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "实例类-实例方法-有参-有返回值")]
        public void SimpleCodeTest2()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "实例类-静态方法-有参-有返回值")]
        public void SimpleCodeTest3()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "实例类-静态方法-有参-有返回值")]
        public void SimpleCodeTest4()
        {

        }
    }
}