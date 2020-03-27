#if DEBUG
//#define DEBUGCONTAINER
#endif
using SkiaSharp;
using SkiaSharp.TextBlocks.Enum;
using System;
using System.Collections.Generic;

namespace SkiaSharp.TextBlocks
{

    public class TextBlock
    {

        public readonly Font Font;
        public readonly SKColor Color;
        public readonly string Text;

        public readonly LineBreakMode LineBreakMode = LineBreakMode.WordWrap;
        public int MaxLines = int.MaxValue;


        /// <summary>
        /// The Glyph Measures. Use LoadMeasures to populate.
        /// </summary>
        public GlyphSpan GlyphSpan;

        /// <summary>
        /// The height of capital letters. Use LoadMeasures to populate.
        /// </summary>
        public float FontHeight;

        /// <summary>
        /// The height of every line. Use LoadMeasures to populate.
        /// </summary>
        public float LineHeight;

        /// <summary>
        /// Vertical margin, printed at both the top, and bottom. Use LoadMeasures to populate.
        /// </summary>
        public float MarginY; // both at the top, and at the bottom 




        /// <summary>
        /// Measurements for ellipsis (used in LineBreakMode.MiddleTruncation) 
        /// </summary>
        internal GlyphSpan EllipsisGlyphSpan;

        // Layout rounding fix
        private const float textwidthmargin = .1f;



        public TextBlock(Font font, SKColor color, string text)
        {
            Font = font;
            Color = color;
            Text = text ?? "";
        }

        public TextBlock(Font font, SKColor color, string text, LineBreakMode lineBreakMode) : this(font, color, text)
        {
            LineBreakMode = lineBreakMode;
        }


        /// <summary>
        /// Measure this text block, given supplied maximum width
        /// </summary>
        /// <param name="textShaper">Text Shaper (measurement cache) to use, or null if measurements shouldn't be cached in memory</param>
        public SKSize Measure(float maxwidth, TextShaper textShaper = null)
        {
            float width = 0;
            LoadMeasures(textShaper);
            if (LineBreakMode == LineBreakMode.MiddleTruncation)
            {
                var spans = GetSpans(maxwidth);
                foreach (var span in spans)
                    width += span.width;
                return new SKSize(width, LineHeight);
            }
            else
            {
                var lines = GetLines(maxwidth);
                foreach (var line in lines)
                    if (width < line.width) width = line.width;
                return new SKSize(width, lines.Count * LineHeight);
            }
        }



        public SKRect Draw(SKCanvas canvas, SKRect rect, TextShaper textShaper = null, FlowDirection flowDirection = FlowDirection.LeftToRight)
        {

            LoadMeasures(textShaper);

            var y = rect.Top + FontHeight + MarginY;

            if (LineBreakMode == LineBreakMode.MiddleTruncation)
            {
                var spans = GetSpans(rect.Width);
                if (flowDirection == FlowDirection.LeftToRight)
                {
                    var x = rect.Left;
                    if (spans.Count > 0)
                    {
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, spans[0]);
                        x += spans[0].width;
                    }
                    if (spans.Count > 1)
                    {
                        canvas.DrawGlyphSpan(EllipsisGlyphSpan, x, y, Color, spans[1]);
                        x += spans[1].width;
                    }
                    if (spans.Count > 2)
                    {
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, spans[2]);
                    }
                }
                else
                {
                    var x = rect.Right;
                    if (spans.Count > 0)
                    {
                        x -= spans[0].width;
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, spans[0]);
                    }
                    if (spans.Count > 1)
                    {
                        x -= spans[1].width;
                        canvas.DrawGlyphSpan(EllipsisGlyphSpan, x, y, Color, spans[1]);
                    }
                    if (spans.Count > 2)
                    {
                        x -= spans[2].width;
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, spans[2]);
                    }
                }
                y += LineHeight;
            }
            else
            {

                var lines = GetLines(rect.Width);
                foreach (var line in lines)
                {
                    if (LineBreakMode == LineBreakMode.WordWrap)
                    {
                        float x;
                        if (flowDirection == FlowDirection.LeftToRight)
                            x = rect.Left;
                        else
                            x = rect.Right - line.width;
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, line);
                    }
                    else if (LineBreakMode == LineBreakMode.Center)
                    {
                        var x = rect.Left + (rect.Width - line.width) / 2;
                        canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, line);
                    }
                    y += LineHeight;
                }

            }

            var paintrect = new SKRect(rect.Left, rect.Top, rect.Right, y - LineHeight + MarginY);

#if DEBUGCONTAINER
            if (canvas != null)
                using (var borderpaint = new SKPaint() { Color = SKColors.Red.WithAlpha(64), IsStroke = true })
                    canvas.DrawRect(paintrect, borderpaint);
#endif

            return paintrect;

        }


        /// <summary>
        /// Populates text measures. If you use a shared textShaper, call this before GetSpans, or GetLines.
        /// </summary>
        public void LoadMeasures(TextShaper textShaper = null)
        {

            if (textShaper == null)
                textShaper = new TextShaper(false);

            if (GlyphSpan == null)
            {

                GlyphSpan = textShaper.GetGlyphSpan(Font, Text);

                var fontMetrics = GlyphSpan.Paint.FontMetrics;
                FontHeight = fontMetrics.CapHeight;
                LineHeight = -fontMetrics.Ascent + fontMetrics.Descent;
                MarginY = (LineHeight - FontHeight) / 2;

            }

            if (LineBreakMode == LineBreakMode.MiddleTruncation)
            {
                if (EllipsisGlyphSpan == null)
                    EllipsisGlyphSpan = textShaper.GetGlyphSpan(Font, " … ");
            }

        }

        private void AssertGlyphSpan()
        {
            if (GlyphSpan == null)
#if DEBUG
                throw new ArgumentNullException("Glyph spans aren't loaded, use LoadMeasures before this call");
#else
                LoadMeasures();
#endif
        }

        /// <summary>
        /// Get spans for single line line break modes.
        /// </summary>
        public List<MeasuredSpan> GetSpans(float width)
        {

            if (LineBreakMode != LineBreakMode.MiddleTruncation)
                throw new ArgumentOutOfRangeException("GetSpans is not supported for this LineBreakMode");

            AssertGlyphSpan();

            var l = GlyphSpan.GlyphCount - 1;

            var result = new List<MeasuredSpan>();
            var full = GlyphSpan.Measure(0, l);
            if (full.width <= width)
                // whole line fits
                result.Add(full);
            else
            {

                var elspan = EllipsisGlyphSpan.Measure(0, 2);
                if (elspan.width > width)
                    return result;

                for (var chars = l; chars > 0; chars--)
                {

                    var start = chars / 2;
                    var endlen = chars - start;

                    var startspan = GlyphSpan.Measure(0, start);
                    var endspan = GlyphSpan.Measure(l - endlen, l);

                    var w = startspan.width + elspan.width + endspan.width;

                    if (w < width)
                    {

                        result.Add(startspan);
                        result.Add(elspan);
                        result.Add(endspan);
                        return result;
                    }

                }

            }

            return result;

        }


        /// <summary>
        /// Get spans for multi-line linebreakmodes
        /// </summary>
        /// <param name="firstlinestart">The start position for the first line, useful when concatenating multiple textblocks</param>
        /// <param name="trimtrailingwhitespace">True to trim trailing whitespaces, use false when concatenating multiple textblocks</param>
        public List<MeasuredSpan> GetLines(float maximumwidth, float firstlinestart = 0, bool trimtrailingwhitespace = true)
        {

            AssertGlyphSpan();

            var result = new List<MeasuredSpan>();

            if (LineBreakMode == LineBreakMode.MiddleTruncation)
                throw new ArgumentOutOfRangeException("GetLines is not supported for this LineBreakMode");

            maximumwidth += textwidthmargin; // fix for width rounding layout issues

            var l = GlyphSpan.WordCount - 1;
            var words = GlyphSpan.Words;
            var linestart = firstlinestart;
            for (var s = 0; s <= l; s++)
            {
                if (s == 0 || words[s].type != WordType.Whitespace)
                {

                    for (var e = s; e <= l; e++)
                    {

                        var word = words[e];
                        var size = GlyphSpan.MeasureWordSpan(s, e, trimtrailingwhitespace);
                        if (e == l && size.width <= maximumwidth - linestart)
                        {
                            // all words fit
                            result.Add(size);
                            if (result.Count == MaxLines) return result;
                            if (word.type == WordType.Linebreak) // trailing linebreaks get an extra line
                                result.Add(new MeasuredSpan(0, -1, -2, 0));
                            return result;
                        }
                        else if (word.type == WordType.Linebreak)
                        {
                            // line breaks force a line
                            result.Add(size);
                            if (result.Count == MaxLines) return result;
                            s = e + 1;
                            linestart = 0;
                        }
                        else if (size.width > maximumwidth - linestart)

                            // doesn't fit

                            if (e > s)
                            {
                                // one full, or multiple whole words
                                size = GlyphSpan.MeasureWordSpan(s, e - 1, trimtrailingwhitespace);
                                result.Add(size);
                                if (result.Count == MaxLines) return result;
                                s = e - 1;
                                linestart = 0;
                                break;
                            }
                            else
                            {

                                // no full word fits, 
                                if (linestart == 0)
                                {

                                    // break up in smaller sets of glyps

                                    var gs = word.firstglyph;
                                    var more = true;
                                    while (more)
                                    {
                                        more = false;
                                        for (var ge = word.lastglyph; ge >= gs; ge--)
                                        {

                                            size = GlyphSpan.Measure(gs, ge);
                                            if (size.width <= maximumwidth - linestart)
                                            {

                                                if (ge == word.lastglyph)
                                                {
                                                    // expand to include additional full words if possible
                                                    for (var we = l; we > e; we--)
                                                    {

                                                        var size2 = GlyphSpan.MeasureGlyphToWordSpan(size.glyphstart, we, trimtrailingwhitespace);
                                                        if (size2.width <= maximumwidth - linestart)
                                                        {
                                                            size = size2;
                                                            s = we;
                                                            break;
                                                        }
                                                    }
                                                }

                                                result.Add(size);
                                                if (result.Count == MaxLines) return result;
                                                gs = ge + 1;
                                                more = gs <= word.lastglyph;
                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }
                                else
                                {

                                    // first line in "concat" mode

                                    // add a newline, then try measuring again
                                    result.Add(new MeasuredSpan(-1, -1, -1, 0));
                                    linestart = 0;
                                    System.Diagnostics.Debug.Assert(s == 0);
                                    s--;

                                    break;

                                }

                            }

                    }
                }
            }

            return result;

        }



    }
}
