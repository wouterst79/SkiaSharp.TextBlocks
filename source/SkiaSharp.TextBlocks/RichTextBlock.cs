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
        public List<TextBlock> Spans = new List<TextBlock>();


        private const float floatroundingmargin = 0.01f;

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

            var line = new List<(TextBlock text, MeasuredSpan wordspan)>();
            var linewidth = 0.0f;

            var linefontheight = 0f;
            var linemarginy = 0f;

            var maxlinewidth = 0.0f;
            var y = rect.Top;

            if (Spans != null)
            {

                // calculate maximum font dimensions
                foreach (var child in Spans)
                    if (child != null)
                    {
                        child.LoadMeasures(textShaper);
                        if (linefontheight < child.FontHeight)
                            linefontheight = child.FontHeight;
                        if (linemarginy < child.MarginY)
                            linemarginy = child.MarginY;
                    }

                y += linefontheight + linemarginy;

                // draw all text blocks in succession
                foreach (var text in Spans)
                    if (text != null)
                    {

                        var childlines = text.GetLines(width, linewidth, false);

                        var childlinecount = childlines.Count;
                        for (var i = 0; i < childlinecount; i++)
                        {

                            var wordspan = childlines[i];
                            line.Add((text, wordspan));

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
                        foreach (var span in line)
                        {

                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            var text = span.text;
                            var spanwidth = span.wordspan.width;

                            // draw the span
                            canvas.DrawGlyphSpan(text.GlyphSpan, x, y, text.Color, span.wordspan);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x, y - text.FontHeight - text.MarginY, x + spanwidth, y + text.MarginY);
                            using (var borderpaint = new SKPaint() { Color = SKColors.Orange.WithAlpha(64), IsStroke = true })
                                canvas.DrawRect(paintrect, borderpaint);
#endif

                            x += spanwidth;

                        }
                    }
                    else
                    {

                        var x = rect.Right;
                        foreach (var span in line)
                        {

                            var text = span.text;
                            var spanwidth = span.wordspan.width;


                            if (PixelRounding)
                                x = (float)Math.Round(x);

                            x -= spanwidth;

                            // draw the span
                            canvas.DrawGlyphSpan(text.GlyphSpan, x, y, text.Color, span.wordspan);

#if DEBUGCONTAINER
                            var paintrect = new SKRect(x, y - text.FontHeight - text.MarginY, x + spanwidth, y + text.MarginY);
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
