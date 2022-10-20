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
        /// �޵��������������ļ򵥴��뵥��
        /// </summary>
        [Fact]
        [Trait("desc", "����ֵ��ȡ")]
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
        /// �޵��������������ļ򵥴��뵥��
        /// </summary>
        [Fact]
        [Trait("desc", "ʵ����-ʵ������-�޲�-�޷���ֵ")]
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
        [Trait("desc", "ʵ����-ʵ������-�в�-�з���ֵ")]
        public void SimpleCodeTest2()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "ʵ����-��̬����-�в�-�з���ֵ")]
        public void SimpleCodeTest3()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        [Trait("desc", "ʵ����-��̬����-�в�-�з���ֵ")]
        public void SimpleCodeTest4()
        {

        }
    }
}