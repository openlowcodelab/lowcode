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
        [Trait("desc", "���ö�̬�����Ľű�����")]
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
        //[Trait("desc", "ʹ�����ʵ��������Ĵ������ķ���������ȡ����ֵ")]
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