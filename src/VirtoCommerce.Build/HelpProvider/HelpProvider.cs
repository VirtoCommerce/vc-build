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

                return string.Compare(target, GetTextContent(heading), StringComparison.OrdinalIgnoreCase) == 0;
            });

            if (targetHelpBlocks == null)
            {
                Log.Error("Help is not found for the target {target}", target);
                return string.Empty;
            }

            var descriptionParts = new List<string>();

            foreach (var block in targetHelpBlocks)
            {
                switch (block)
                {
                    case FencedCodeBlock fencedCodeBlock:
                        var codeText = GetFencedText(fencedCodeBlock);
                        if (!string.IsNullOrWhiteSpace(codeText))
                        {
                            descriptionParts.Add(codeText);
                        }
                        break;

                    case LeafBlock leafBlock:
                        var blockText = GetTextContent(leafBlock);
                        if (!string.IsNullOrWhiteSpace(blockText))
                        {
                            descriptionParts.Add(blockText);
                        }
                        break;

                    case ListBlock listBlock:
                        var listText = GetListContent(listBlock);
                        if (!string.IsNullOrWhiteSpace(listText))
                        {
                            descriptionParts.Add(listText);
                        }
                        break;
                }
            }

            return string.Join(Environment.NewLine, descriptionParts);
        }

        public static IEnumerable<string> GetTargets()
        {
            var rawMd = GetRawMDContent();
            var md = GetParsedHelpFile(rawMd);
            var helpBlocks = SplitMarkdownDocumentBySeparators(md);
            foreach (var targetHelpBlocks in helpBlocks)
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

        private static string GetListContent(ListBlock listBlock)
        {
            if (listBlock == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (var listItem in listBlock)
            {
                if (listItem is ListItemBlock itemBlock)
                {
                    foreach (var block in itemBlock)
                    {
                        if (block is ParagraphBlock paragraph)
                        {
                            var itemText = GetInlineContent(paragraph.Inline);
                            if (!string.IsNullOrWhiteSpace(itemText))
                            {
                                result.AppendLine($"- {itemText}");
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }

        private static string GetInlineContent(ContainerInline containerInline)
        {
            if (containerInline == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            var inline = containerInline.FirstChild;

            while (inline != null)
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        var inlineContent = literal.Content;
                        result.Append(inlineContent.Text.AsSpan(inlineContent.Start, inlineContent.Length));
                        break;

                    case CodeInline codeInline:
                        result.Append($"`{codeInline.Content}`");
                        break;

                    case EmphasisInline emphasis:
                        var emphasisContent = GetInlineContent(emphasis);
                        if (emphasis.DelimiterChar == '*' && emphasis.DelimiterCount == 2)
                        {
                            // Bold text - remove the formatting for console output
                            result.Append(emphasisContent);
                        }
                        else if (emphasis.DelimiterChar == '*' && emphasis.DelimiterCount == 1)
                        {
                            // Italic text
                            result.Append(emphasisContent);
                        }
                        break;

                    case LineBreakInline:
                        result.AppendLine();
                        break;

                    case LinkInline link:
                        var linkText = GetInlineContent(link);
                        result.Append(linkText);
                        break;

                    default:
                        // Handle other inline types by extracting their text content
                        if (inline is ContainerInline container)
                        {
                            result.Append(GetInlineContent(container));
                        }
                        break;
                }

                inline = inline.NextSibling;
            }

            return result.ToString();
        }
    }
}
