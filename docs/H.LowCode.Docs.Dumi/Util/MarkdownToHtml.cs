using Markdig;
using System.Reflection;

namespace H.LowCode.Docs.Dumi.Util
{
    internal class MarkdownToHtml
    {
        private static Assembly _assembly;

        internal static string ConvertToHtml(string fileName, Type type)
        {
            string markdownString = GetEmbeddedFile(fileName, type);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string html = Markdown.ToHtml(markdownString, pipeline);
            return html;
        }

        private static string GetEmbeddedFile(string fileName, Type type)
        {
            if (_assembly == null)
            {
                _assembly = Assembly.GetAssembly(type);
            }
            string resourceName = type.Namespace + "." + fileName;

            _assembly.GetManifestResourceStream(resourceName);
            Stream stream = _assembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();

            return content;
        }
    }
}
