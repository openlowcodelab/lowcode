using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace H.LowCode.Docs.Dumi.Util
{
    internal class MarkdownToHtml
    {
        internal static string ConvertToHtml(string fileName, Type type)
        {
            string markdownString = GetEmbeddedFile(fileName, type);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string html = Markdown.ToHtml(markdownString, pipeline);
            return html;
        }

        private static string GetEmbeddedFile(string fileName, Type type)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            string resourceName = type.Namespace + "." + fileName;

            assembly.GetManifestResourceStream(resourceName);
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();

            return content;
        }
    }
}
