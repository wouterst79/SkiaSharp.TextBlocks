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
        public bool PixelRounding = false;

        /// <summary>
        /// The contents of the rich text block
        /// </summary>
        public List<RichTextSpan> Spans = new List<RichTextSpan>();


        public int MaxLines = int.MaxValue;
        public LineAlignment LineAlignment = LineAlignment.Near;

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
            var newspan = new TextBlockSpan(textblock);
            Spans.Add(newspan);
            return newspan;
        }

        /// <summary>
        /// Add multiple textblocks
        /// </summary>
        public void Add(IEnumerable<TextBlock> textblocks)
        {
            foreach (var textblock in textblocks)
                Spans.Add(new TextBlockSpan(textblock));
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

            var maxlines = MaxLines;

            if (Spans != null)
            {

                // calculate maximum font dimensions
                foreach (var span in Spans)
                    if (span != null)
                    {
                        var (fontheight, marginy) = span.GetMeasures(textShaper);
                        if (linefontheight < fontheight) linefontheight = fontheight;
                        if (linemarginy < marginy) linemarginy = marginy;
                    }

                y += linefontheight + linemarginy;

                // draw all text blocks in succession
                foreach (var span in Spans)
                    if (span != null)
                    {

                        var childlines = span.GetLines(width, linewidth, false);

                        if (childlines != null)
                        {

                            var childlinecount = childlines.Count;
                            for (var i = 0; i < childlinecount; i++)
                            {

                                var wordspan = childlines[i];
                                line.Add((span, wordspan));

                                linewidth += wordspan.width;
                                if (maxlinewidth < linewidth)
                                    maxlinewidth = linewidth;

                                if (i != childlinecount - 1)
                                {
                                    LayoutLine();
                                    if (maxlines <= 0) break;
                                }

                            }

                        }

                        if (maxlines <= 0) break;

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
                        if (LineAlignment == LineAlignment.Far) x += rect.Width - linewidth;
                        if (LineAlignment == LineAlignment.Center) x += (rect.Width - linewidth) / 2;
                        foreach (var chunk in line)
                        {

                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            var span = chunk.span;
                            var spanwidth = chunk.wordspan.width;

                            // draw the span
                            span.DrawMeasuredSpan(canvas, x, y, linefontheight, linemarginy, chunk.wordspan, false);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x, y - linefontheight - linemarginy, x + spanwidth, y + linemarginy);
                            using (var borderpaint = new SKPaint() { Color = SKColors.Orange.WithAlpha(64), IsStroke = true })
                                canvas.DrawRect(paintrect, borderpaint);
#endif

                            x += spanwidth;

                        }
                    }
                    else
                    {

                        var x = rect.Right;
                        if (LineAlignment == LineAlignment.Far) x -= rect.Width - linewidth;
                        if (LineAlignment == LineAlignment.Center) x -= (rect.Width - linewidth) / 2;
                        foreach (var chunk in line)
                        {

                            var span = chunk.span;
                            var spanwidth = chunk.wordspan.width;

                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            x -= spanwidth;

                            // draw the span
                            span.DrawMeasuredSpan(canvas, x, y, linefontheight, linemarginy, chunk.wordspan, true);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x, y - linefontheight - linemarginy, x + spanwidth, y + linemarginy);
                            using (var borderpaint = new SKPaint() { Color = SKColors.Orange.WithAlpha(64), IsStroke = true })
                                canvas.DrawRect(paintrect, borderpaint);
#endif

                        }

                    }

                linewidth = 0;
                y += linefontheight + linemarginy * 2;

                line.Clear();

                maxlines--;

            }
        }

    }
}
