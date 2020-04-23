#if DEBUG
//#define DEBUGCONTAINER
#endif
using SkiaSharp;
using SkiaSharp.TextBlocks.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SkiaSharp.TextBlocks
{

    public class RichTextBlock
    {

        /// <summary>
        /// Set to true if text should be aligned on the pixel grid.
        /// This will make text look less "jumpy" if the same text is moved on the canvas,
        /// but may not be the best option if certain translations are applied to the canvas.
        /// Default is true.
        /// </summary>
        public bool PixelRounding = true;

        /// <summary>
        /// The contents of the rich text block
        /// </summary>
        public List<RichTextSpan> Spans = new List<RichTextSpan>();


        private const float floatroundingmargin = 0.01f;


        // constructors
        public RichTextBlock() { }
        public RichTextBlock(TextBlock textBlock) { Add(textBlock); }
        public RichTextBlock(RichTextSpan richTextSpan) { Add(richTextSpan); }
        public RichTextBlock(params TextBlock[] textBlocks) { Add(textBlocks); }
        public RichTextBlock(params RichTextSpan[] richTextSpans) { Add(richTextSpans); }


        #region Add methods

        /// <summary>
        /// Add one textblock
        /// </summary>
        public RichTextSpan Add(TextBlock textblock)
        {
            var newspan = new RichTextSpan { TextBlock = textblock };
            Spans.Add(newspan);
            return newspan;
        }

        /// <summary>
        /// Add multiple textblocks
        /// </summary>
        public void Add(IEnumerable<TextBlock> textblocks)
        {
            foreach (var textblock in textblocks)
                Spans.Add(new RichTextSpan { TextBlock = textblock });
        }

        /// <summary>
        /// Add one richtextspan
        /// </summary>
        public RichTextSpan Add(RichTextSpan span)
        {
            Spans.Add(span);
            return span;
        }

        /// <summary>
        /// Add multiple richtextspans
        /// </summary>
        public void Add(IEnumerable<RichTextSpan> spans)
        {
            Spans.AddRange(spans);
        }

        #endregion


        /// <summary>
        /// Measures the Rich Text, given the maximum width
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="textShaper"></param>
        /// <returns></returns>
        public SKSize Measure(float maxWidth, TextShaper textShaper = null)
        {
            var calculated = Layout(null, new SKRect(0, 0, maxWidth, float.MaxValue), FlowDirection.LeftToRight, textShaper ?? new TextShaper(false));
            return new SKSize(calculated.Width, calculated.Height);
        }

        /// <summary>
        /// Paint's the rich text in the provided rectangle
        /// </summary>
        /// <returns>a rect containing the actual bottom of painted text</returns>
        public SKRect Paint(SKCanvas canvas, SKRect rect, FlowDirection flowDirection = FlowDirection.LeftToRight, TextShaper textShaper = null)
        {
            return Layout(canvas, rect, flowDirection, textShaper);
        }


        private SKRect Layout(SKCanvas canvas, SKRect rect, FlowDirection flowDirection, TextShaper textShaper)
        {

            var width = rect.Width;

            var line = new List<(RichTextSpan span, MeasuredSpan wordspan)>();
            var linewidth = 0.0f;

            var linefontheight = 0f;
            var linemarginy = 0f;

            var maxlinewidth = 0.0f;
            var y = rect.Top;

            if (Spans != null)
            {

                // calculate maximum font dimensions
                foreach (var span in Spans)
                    if (span != null && span.TextBlock != null)
                    {
                        var text = span.TextBlock;
                        text.LoadMeasures(textShaper);
                        if (linefontheight < text.FontHeight)
                            linefontheight = text.FontHeight;
                        if (linemarginy < text.MarginY)
                            linemarginy = text.MarginY;
                    }

                y += linefontheight + linemarginy;

                // draw all text blocks in succession
                foreach (var span in Spans)
                    if (span != null && span.TextBlock != null)
                    {

                        var text = span.TextBlock;
                        var childlines = text.GetLines(width, linewidth, false);

                        var childlinecount = childlines.Count;
                        for (var i = 0; i < childlinecount; i++)
                        {

                            var wordspan = childlines[i];
                            line.Add((span, wordspan));

                            linewidth += wordspan.width;
                            if (maxlinewidth < linewidth)
                                maxlinewidth = linewidth;

                            if (i != childlinecount - 1)
                                // text returned multiple lines, lay a line out for all that aren't the last
                                LayoutLine();
                            else
                                if (wordspan.glyphend == -1)
                                LayoutLine(); // finish the line if the lines end with a line break;

                        }

                    }

                if (line.Count > 0)
                    LayoutLine();

                y -= linefontheight + linemarginy;

            }

            if (flowDirection == FlowDirection.Unknown || flowDirection == FlowDirection.LeftToRight)
                return new SKRect(rect.Left, rect.Top, rect.Left + maxlinewidth + floatroundingmargin, y);
            else
                return new SKRect(rect.Right - maxlinewidth - floatroundingmargin, rect.Top, rect.Right, y);

            void LayoutLine()
            {

                if (canvas != null)

                    if (flowDirection == FlowDirection.LeftToRight)
                    {
                        var x = rect.Left;
                        foreach (var chunk in line)
                        {

                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            var span = chunk.span;
                            var text = span.TextBlock;
                            var spanwidth = chunk.wordspan.width;
                            var t = span.Translate;

                            // draw the span
                            canvas.DrawGlyphSpan(text.GlyphSpan, x + t.X, y + t.Y, text.Color, chunk.wordspan);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x + t.X, y - text.FontHeight - text.MarginY + t.Y, x + spanwidth + t.X, y + text.MarginY + t.Y);
                            using (var borderpaint = new SKPaint() { Color = SKColors.Orange.WithAlpha(64), IsStroke = true })
                                canvas.DrawRect(paintrect, borderpaint);
#endif

                            x += spanwidth;

                        }
                    }
                    else
                    {

                        var x = rect.Right;
                        foreach (var chunk in line)
                        {

                            var span = chunk.span;
                            var text = span.TextBlock;
                            var spanwidth = chunk.wordspan.width;
                            var t = span.Translate;

                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            x -= spanwidth;

                            // draw the span
                            canvas.DrawGlyphSpan(text.GlyphSpan, x - t.X, y + t.Y, text.Color, chunk.wordspan);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x - t.X, y - text.FontHeight - text.MarginY + t.Y, x + spanwidth - t.X, y + text.MarginY + t.Y);
                            using (var borderpaint = new SKPaint() { Color = SKColors.Orange.WithAlpha(64), IsStroke = true })
                                canvas.DrawRect(paintrect, borderpaint);
#endif

                        }

                    }

                linewidth = 0;
                y += linefontheight + linemarginy * 2;

                line.Clear();

            }
        }

    }
}
