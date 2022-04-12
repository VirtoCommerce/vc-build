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
using Serilog;

namespace HelpProvider
{
    public static class HelpProvider
    {
        public static string GetHelpForTarget(string target)
        {
            var containers = GetCustomContainers();

            var container = containers.FirstOrDefault(c =>
            {
                var heading = c.Descendants<HeadingBlock>().FirstOrDefault();

                if (heading == null)
                {
                    return false;
                }

                return string.Compare(target, GetTextContent(heading), true) == 0;
            });

            if (container == null)
            {
                Log.Error($"Help is not found for the target {target}");
                return string.Empty;
            }

            var descriptionBlock = container.Descendants<ParagraphBlock>().FirstOrDefault();
            var exampleBlock = container.Descendants<FencedCodeBlock>().FirstOrDefault();
            var description = GetTextContent(descriptionBlock);
            var examples = GetFencedText(exampleBlock);
            var result = $"{description}{Environment.NewLine}{examples}";
            return result;
        }

        public static IEnumerable<string> GetTargets()
        {
            var containers = GetCustomContainers();
            var result = new List<string>();
            foreach (var container in containers)
            {
                var heading = container.Descendants<HeadingBlock>().FirstOrDefault();

                if (heading == null)
                {
                    continue;
                }
                result.Add(GetTextContent(heading));
            }
            return result;
        }

        private static IEnumerable<CustomContainer> GetCustomContainers()
        {
            var pipeline = new MarkdownPipelineBuilder().UseCustomContainers().Build();
            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var helpFilePath = Path.Combine(rootDirectory, "targets.md");
            var markdownDocument = Markdown.Parse(File.ReadAllText(helpFilePath), pipeline);
            return markdownDocument.Descendants<CustomContainer>();
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
                        result.Append(inlineContent.Text.AsSpan(inlineContent.Start, inlineContent.Length));
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
