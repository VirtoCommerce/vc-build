using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Nuke.Common;

namespace VirtoCommerce.Build.HelpProvider
{
    public class HelpProvider
    {

        public static string GetHelpForTarget(string target)
        {
            var pipeline = new MarkdownPipelineBuilder().UseCustomContainers().Build();
            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var helpFilePath = Path.Combine(rootDirectory, "docs", "targets.md");
            var markdownDocument = Markdown.Parse(File.ReadAllText(helpFilePath), pipeline);
            var containers = markdownDocument.Descendants<CustomContainer>();

            var container = containers.FirstOrDefault(c =>
            {
                var heading = c.Descendants<HeadingBlock>().FirstOrDefault();

                if (heading == null)
                {
                    return false;
                }

                return target == GetTextContent(heading);
            });

            if (container == null)
            {
                Logger.Error($"Help is not found for the target {target}");
                return string.Empty;
            }

            var descriptionBlock = container.Descendants<ParagraphBlock>().FirstOrDefault();
            var exampleBlock = container.Descendants<FencedCodeBlock>().FirstOrDefault();
            var description = GetTextContent(descriptionBlock);
            var examples = GetFencedText(exampleBlock);
            var result = $"{description}{Environment.NewLine}{examples}";
            return result;
        }

        private static string GetTextContent(LeafBlock leaf)
        {
            var inline = leaf?.Inline?.FirstChild;

            if (inline is null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            do
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        var inlineContent = literal.Content;
                        result.Append(inlineContent.Text.Substring(inlineContent.Start, inlineContent.Length));
                        break;
                    case LineBreakInline:
                        result.Append(Environment.NewLine);
                        break;
                }

                inline = inline.NextSibling;
            }
            while (inline != null);

            return result.ToString();
        }

        private static string GetFencedText(FencedCodeBlock fencedCodeBlock)
        {
            if (fencedCodeBlock == null)
            {
                return string.Empty;
            }

            var lines = fencedCodeBlock.Lines.Lines.Select(line =>
            {
                var slice = line.Slice;

                if (EqualityComparer<StringSlice>.Default.Equals(slice, default))
                {
                    return string.Empty;
                }

                return slice.Text?.Substring(line.Slice.Start, line.Slice.Length) ?? string.Empty;
            });

            return string.Join(Environment.NewLine, lines);
        }
    }
}
