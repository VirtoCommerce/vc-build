using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Serilog;

namespace HelpProvider
{
    public static class HelpProvider
    {
        public static string GetTargetDescription(string target)
        {
            var rawMd = GetRawMDContent();
            var md = GetParsedHelpFile(rawMd);
            var helpBlocks = SplitMarkdownDocumentBySeparators(md);
            var targetHelpBlocks = helpBlocks.FirstOrDefault(c =>
            {
                var heading = c.FirstOrDefault(b => b is HeadingBlock) as HeadingBlock;

                if (heading == null)
                {
                    return false;
                }

                return string.Compare(target, GetTextContent(heading), true) == 0;
            });

            if (targetHelpBlocks == null)
            {
                Log.Error($"Help is not found for the target {target}");
                return string.Empty;
            }

            var descriptionBlocks = targetHelpBlocks.OfType<LeafBlock>().Select(b =>
            {
                var blockText = GetTextContent(b);
                return string.Join(Environment.NewLine, blockText);
            }).ToArray();

            var description = string.Join(Environment.NewLine, descriptionBlocks);

            var usageBlocks = targetHelpBlocks.OfType<FencedCodeBlock>().Select(b =>
            {
                var blockText = GetFencedText(b);
                return string.Join(Environment.NewLine, blockText);
            }).ToArray();
            var usage = string.Join(Environment.NewLine, usageBlocks);
            return string.Join(Environment.NewLine, description, usage);
        }

        public static IEnumerable<string> GetTargets()
        {
            var rawMd = GetRawMDContent();
            var md = GetParsedHelpFile(rawMd);
            var helpBlocks = SplitMarkdownDocumentBySeparators(md);
            foreach(var targetHelpBlocks in helpBlocks)
            {
                var heading = targetHelpBlocks.FirstOrDefault(b => b is HeadingBlock) as HeadingBlock;

                if (heading == null)
                {
                    continue;
                }

                yield return GetTextContent(heading);
            }
        }

        private static List<List<Block>> SplitMarkdownDocumentBySeparators(MarkdownDocument document)
        {
            var result = new List<List<Block>>();
            foreach (var block in document)
            {
                if (block is ThematicBreakBlock)
                {
                    result.Add(new List<Block>());
                }
                else
                {
                    if (result.Count == 0)
                    {
                        result.Add(new List<Block>());
                    }
                    result.Last().Add(block);
                }
            }
            return result;
        }

        private static string GetRawMDContent()
        {

            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var helpFilePath = Path.Combine(rootDirectory, "targets.md");
            return File.ReadAllText(helpFilePath);
        }

        private static MarkdownDocument GetParsedHelpFile(string rawContent)
        {
            var pipeline = new MarkdownPipelineBuilder().UsePreciseSourceLocation().UseCustomContainers().Build();
            var markdownDocument = Markdown.Parse(rawContent, pipeline);
            return markdownDocument;
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

                    case CodeInline literal:
                        result.Append(literal.Content);
                        break;

                    case LineBreakInline:
                        result.AppendLine();
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
