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

        private static ListPool<MeasuredSpan> ListCache = new ListPool<MeasuredSpan>();

        public readonly Font Font;
        public SKColor Color;
        public readonly string Text;

        public readonly LineBreakMode LineBreakMode = LineBreakMode.WordWrap;
        public int MaxLines = int.MaxValue;


        // Animated drawing
        public Func<int, int, SKColor> GetLineColor { get; set; }
        public GlyphAnimation GlyphAnimation { get; set; }


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
        /// The factor to apply to line height. Set this before calling LoadMeasures.
        /// Default: set through DefaultLineSpacing.
        /// </summary>
        public float LineSpacing = DefaultLineSpacing;

        /// <summary>
        /// Default line spacing.
        /// </summary>
        public static float DefaultLineSpacing = 1f;

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

        public TextBlock(Font font, Func<int, int, SKColor> getlinecolor, string text)
        {
            Font = font;
            Color = default;
            GetLineColor = getlinecolor;
            Text = text ?? "";
        }

        public TextBlock(Font font, SKColor color, string text, GlyphAnimation glyphAnimation)
        {
            Font = font;
            Color = color;
            Text = text ?? "";
            GlyphAnimation = glyphAnimation;
        }



        /// <summary>
        /// Measure this text block, given supplied maximum width
        /// </summary>
        /// <param name="textShaper">Text Shaper (measurement cache) to use, or null if measurements shouldn't be cached in memory</param>
        public SKSize Measure(float maxwidth, TextShaper textShaper = null)
        {

            var lines = ListCache.Get();

            try
            {

                float width = 0;
                LoadMeasures(textShaper);
                if (LineBreakMode == LineBreakMode.MiddleTruncation)
                {
                    GetSpans(lines, maxwidth);
                    foreach (var span in lines)
                        width += span.width;
                    return new SKSize(width, LineHeight);
                }
                else
                {
                    GetLines(lines, maxwidth);
                    foreach (var line in lines)
                        if (width < line.width) width = line.width;
                    return new SKSize(width, lines.Count * LineHeight);
                }

            }
            finally
            {
                ListCache.Return(lines);
            }

        }



        public SKRect Draw(SKCanvas canvas, SKRect rect, TextShaper textShaper = null, FlowDirection flowDirection = FlowDirection.LeftToRight)
        {

            var lines = ListCache.Get();

            try
            {

                LoadMeasures(textShaper);

                var y = rect.Top + FontHeight + MarginY;

                if (LineBreakMode == LineBreakMode.MiddleTruncation)
                {
                    GetSpans(lines, rect.Width);
                    if (flowDirection == FlowDirection.LeftToRight)
                    {
                        var x = rect.Left;
                        if (lines.Count > 0)
                        {
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, lines[0]);
                            x += lines[0].width;
                        }
                        if (lines.Count > 1)
                        {
                            canvas.DrawGlyphSpan(EllipsisGlyphSpan, x, y, Color, lines[1]);
                            x += lines[1].width;
                        }
                        if (lines.Count > 2)
                        {
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, lines[2]);
                        }
                    }
                    else
                    {
                        var x = rect.Right;
                        if (lines.Count > 0)
                        {
                            x -= lines[0].width;
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, lines[0]);
                        }
                        if (lines.Count > 1)
                        {
                            x -= lines[1].width;
                            canvas.DrawGlyphSpan(EllipsisGlyphSpan, x, y, Color, lines[1]);
                        }
                        if (lines.Count > 2)
                        {
                            x -= lines[2].width;
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, lines[2]);
                        }
                    }
                    y += LineHeight;
                }
                else
                {

                    GetLines(lines, rect.Width);
                    var i = 0;
                    foreach (var line in lines)
                    {
                        if (GetLineColor != null) Color = GetLineColor(i++, lines.Count);
                        if (LineBreakMode == LineBreakMode.WordWrap)
                        {
                            float x;
                            if (flowDirection == FlowDirection.LeftToRight)
                                x = rect.Left;
                            else
                                x = rect.Right - line.width;
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, line, GlyphAnimation);
                        }
                        else if (LineBreakMode == LineBreakMode.Center)
                        {
                            var x = rect.Left + (rect.Width - line.width) / 2;
                            canvas.DrawGlyphSpan(GlyphSpan, x, y, Color, line, GlyphAnimation);
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
            finally
            {
                ListCache.Return(lines);
            }

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
                if (Font.LineHeight.HasValue)
                    LineHeight = Font.LineHeight.Value;
                else
                    LineHeight = (fontMetrics.Descent - fontMetrics.Ascent + fontMetrics.Leading) * LineSpacing;
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
        public void GetSpans(List<MeasuredSpan> result, float width)
        {

            if (LineBreakMode != LineBreakMode.MiddleTruncation)
                throw new ArgumentOutOfRangeException("GetSpans is not supported for this LineBreakMode");

            AssertGlyphSpan();

            var l = GlyphSpan.GlyphCount - 1;

            var full = GlyphSpan.Measure(0, l);
            if (full.width <= width)
                // whole line fits
                result.Add(full);
            else
            {

                var elspan = EllipsisGlyphSpan.Measure(0, 2);
                if (elspan.width > width)
                    return;

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
                        return;
                    }

                }

            }

        }


        /// <summary>
        /// Get spans for multi-line linebreakmodes
        /// </summary>
        /// <param name="firstlinestart">The start position for the first line, useful when concatenating multiple textblocks</param>
        /// <param name="trimtrailingwhitespace">True to trim trailing whitespaces, use false when concatenating multiple textblocks</param>
        public void GetLines(List<MeasuredSpan> result, float maximumwidth, float firstlinestart = 0, bool trimtrailingwhitespace = true)
        {

            AssertGlyphSpan();

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
                            if (result.Count == MaxLines) return;
                            if (word.type == WordType.Linebreak) // trailing linebreaks get an extra line
                                result.Add(new MeasuredSpan(0, -1, -2, 0));
                            return;
                        }
                        else if (word.type == WordType.Linebreak)
                        {
                            // line breaks force a line
                            result.Add(size);
                            if (result.Count == MaxLines) return;
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
                                if (result.Count == MaxLines) return;
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
                                                if (result.Count == MaxLines) return;
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

        }



    }
}
