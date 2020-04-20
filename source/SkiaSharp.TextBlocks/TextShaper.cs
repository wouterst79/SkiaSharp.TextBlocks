using HBBuffer = HarfBuzzSharp.Buffer;
using HarfBuzzSharp;
using SkiaSharp;
using System;
using SkiaSharp.TextBlocks.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SkiaSharp.TextBlocks
{


    /// <summary>
    /// Provides easy access to text measurements. Optionally caches measurement results in memory.
    /// If you make a lot of TextBlock objects, that all have the same content (rather than re-using existing textblocks), 
    /// supplying a shared TextShaper, that is created with UseCache=True will speed up calculations, while slightly increasing memory use
    /// </summary>
    public class TextShaper : IDisposable
    {

        /// <summary>
        /// True to create paint with IsAntiAlias set. Does not affect measures.
        /// Default is True.
        /// </summary>
        public bool IsAntiAlias = true;

        /// <summary>
        /// The scale factor applied to all measures.
        /// Default is 1 (no scaling).
        /// </summary>
        public int Scale = 1;

        public Dictionary<SKTypeface, TypefaceTextShaper> TypeShaperCache;
        public Dictionary<(Font font, string text), GlyphSpan> GlyphSpanCache;

        /// <summary>
        /// Use the shared font manager (and don't dispose cached typefaces).
        /// Default: true
        /// </summary>
        public static bool UseSharedFontManagerWhenCaching = true;
        public SKFontManager FontManager;

        /// <summary>
        /// Create a new Text Shaper
        /// </summary>
        /// <param name="usechache">true to store all produced glyphspans (and typeface references) in an internal dictionary</param>
        public TextShaper(bool usechache, float scale = 1f)
        {
            if (usechache)
            {
                TypeShaperCache = new Dictionary<SKTypeface, TypefaceTextShaper>();
                GlyphSpanCache = new Dictionary<(Font font, string text), GlyphSpan>();

                if (!UseSharedFontManagerWhenCaching)
                    FontManager = SKFontManager.CreateDefault();
            }

            if (FontManager == null)
                FontManager = SKFontManager.Default;
        }

        public void Dispose()
        {

            // Dispose type shapers
            if (TypeShaperCache != null)
                foreach (var shaper in TypeShaperCache.Values)
                    shaper.Dispose();

            // Dispose font manager and type face cache
            var ownsFontManager = FontManager != SKFontManager.Default;
            if (ownsFontManager)
                FontManager.Dispose();

            // Dispose glyph span cache
            if (GlyphSpanCache != null)
                foreach (var span in GlyphSpanCache.Values)
                    span.Dispose();

        }


        /// <summary>
        /// Produces a glyph span for provided font and text
        /// </summary>
        public GlyphSpan GetGlyphSpan(Font font, string text)
        {

            if (GlyphSpanCache == null || !GlyphSpanCache.TryGetValue((font, text), out var shape))
            {

                var (typefaces, ids) = font.GetTypefaces(text, FontManager);
                var typefacecount = typefaces.Length;
                var shapers = new TypefaceTextShaper[typefacecount];
                var paints = new SKPaint[typefacecount];

                for (int i = 0; i < typefaces.Length; i++)
                {
                    shapers[i] = GetFontShaper(typefaces[i]);
                    paints[i] = new SKPaint()
                    {
                        TextSize = font.TextSize * Scale,
                        Typeface = typefaces[i],
                        TextEncoding = SKTextEncoding.GlyphId,
                        IsAntialias = IsAntiAlias,
                        FakeBoldText = font.Bold && Font.UseFakeBoldText,
                    };
                }

                shape = Shape(typefaces, shapers, paints, text, ids);

                if (GlyphSpanCache != null)
                    GlyphSpanCache.Add((font, text), shape);

            }

            return shape;

        }


        private TypefaceTextShaper GetFontShaper(SKTypeface typeface)
        {

            if (TypeShaperCache == null)
                return new TypefaceTextShaper(typeface);

            if (!TypeShaperCache.TryGetValue(typeface, out var shaper))
                TypeShaperCache.Add(typeface, shaper = new TypefaceTextShaper(typeface));

            return shaper;

        }


        /// <summary>
        /// Break a string into words, and then uses HarfBuzzSharp to convert them into glyphs, and glyph coordinates.
        /// </summary>
        public GlyphSpan Shape(SKTypeface[] typefaces, TypefaceTextShaper[] shapers, SKPaint[] paints, string text, byte[] typefaceids)
        {

            if (string.IsNullOrEmpty(text))
                return new GlyphSpan(paints);

            var firstpaint = paints[0];

            // get the sizes
            float scaley = firstpaint.TextSize / TypefaceTextShaper.FONT_SIZE_SCALE;
            float scalex = scaley * firstpaint.TextScaleX;

            // prepare the output buffers
            HBBuffer buffer;
            var direction = FlowDirection.Unknown;
            var startpointlength = text.Length + 1; // default point buffer length
            var startpoints = new SKPoint[startpointlength];
            var paintids = new byte[startpointlength];
            var codepoints = new byte[startpointlength * 2];
            var glyphcount = 0;

            var words = new List<(int startglyph, int endglyph, WordType type)>();
            using (buffer = new HBBuffer())
            {

                // determine direction (for the whole text)
                buffer.AddUtf8(text);
                buffer.GuessSegmentProperties();
                if (buffer.Direction == Direction.LeftToRight || buffer.Direction == Direction.RightToLeft)
                    direction = buffer.Direction == Direction.LeftToRight ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                else
                    Console.WriteLine("only Direction.LeftToRight and Direction.RightToLeft are currently supported.");

                // prepare buffer
                buffer.ClearContents();

                // count and add words.
                foreach (var word in GetWords(text))
                {


                    // start the word                    
                    var wordstart = glyphcount;

                    // shape spans by unique typefaces (may have multiple in 1 word)
                    var s = word.start;
                    var typefaceid = typefaceids[s];
                    for (var i = word.start + 1; i <= word.end; i++)
                    {

                        if (typefaceid != typefaceids[i])
                        {

                            var span = text.Substring(s, i - s);
                            buffer.AddUtf8(span);
                            buffer.GuessSegmentProperties();
                            ShapeSpan(typefaceid);

                            s = i;
                            typefaceid = typefaceids[i];

                        }

                    }

                    // shape remaining span
                    var remainder = text.Substring(s, word.end - s + 1);
                    buffer.AddUtf8(remainder);
                    buffer.GuessSegmentProperties();
                    ShapeSpan(typefaceid);


                    // add the shaped word
                    words.Add((wordstart, glyphcount - 1, word.type));


                }

            }

            return new GlyphSpan(paints, direction, paintids, codepoints, startpoints, glyphcount, words);

            void ShapeSpan(byte typefaceid)
            {

                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));

                var shaper = shapers[typefaceid];

                // do the shaping
                shaper.Shape(buffer);

                // get the shaping results
                var len = buffer.Length; // note that the length after shaping may be different from the length before shaping (shorter, or longer)
                var info = buffer.GlyphInfos;
                var pos = buffer.GlyphPositions;

                if (glyphcount + len >= startpoints.Length)
                {

                    // when the word produce more glyphs than fit in the buffers (IE, in Thai), resize the buffers.

                    startpointlength = glyphcount + len + startpointlength / 2 + 1;
                    var newstartpoints = new SKPoint[startpointlength];
                    var newpaintids = new byte[startpointlength];
                    var newcodepoints = new byte[startpointlength * 2];

                    int s;
                    if (direction == FlowDirection.LeftToRight)
                        s = 0;
                    else
                        s = newstartpoints.Length - startpoints.Length;

                    for (int i = 0; i < startpoints.Length; i++)
                    {
                        newstartpoints[s + i] = startpoints[i];
                        newpaintids[s + i] = paintids[i];
                        newcodepoints[(s + i) * 2] = codepoints[i * 2];
                        newcodepoints[(s + i) * 2 + 1] = codepoints[i * 2 + 1];
                    }

                    startpoints = newstartpoints;
                    paintids = newpaintids;
                    codepoints = newcodepoints;

                }

                if (direction != FlowDirection.RightToLeft)
                {

                    // Default & LTR
                    float x = startpoints[glyphcount].X;
                    float y = startpoints[glyphcount].Y;
                    for (var i = 0; i < len; i++)
                    {

                        var glyph = glyphcount + i;

                        var bytes = BitConverter.GetBytes((ushort)info[i].Codepoint);
                        codepoints[glyph * 2] = bytes[0];
                        codepoints[glyph * 2 + 1] = bytes[1];

                        paintids[glyph] = typefaceid;
                        startpoints[glyph] = new SKPoint(x + pos[i].XOffset * scalex, y - pos[i].YOffset * scaley);

                        // move the cursor
                        x += pos[i].XAdvance * scalex;
                        y += pos[i].YAdvance * scaley;

                    }

                    startpoints[glyphcount + len] = new SKPoint(x, y);

                }
                else
                {

                    // RTL: fill out startpoints in reverse order

                    var idx = startpoints.Length - 1 - glyphcount;

                    var x = startpoints[idx].X;
                    var y = startpoints[idx].Y;
                    for (var i = len - 1; i >= 0; i--)
                    {

                        var cp = idx - len + i;
                        var glyph = glyphcount + i;

                        var bytes = BitConverter.GetBytes((ushort)info[i].Codepoint);
                        codepoints[cp * 2] = bytes[0];
                        codepoints[cp * 2 + 1] = bytes[1];

                        // move the cursor
                        x -= pos[i].XAdvance * scalex;
                        y -= pos[i].YAdvance * scaley;

                        paintids[glyph] = typefaceid;
                        startpoints[cp] = new SKPoint(x - pos[i].XOffset * scalex, y - pos[i].YOffset * scaley);

                    }

                    startpoints[idx - len] = new SKPoint(x, y);

                }

                // advance cursor
                glyphcount += len;

                // reset buffer
                buffer.ClearContents();

            }
        }

        /// <summary>
        /// Breaks a string into "words", including "whitespace" words, and "newline" words
        /// </summary>
        private IEnumerable<(int start, int end, WordType type)> GetWords(string line)
        {

            var start = 0;
            var lastwordtype = WordType.Word;
            for (var i = 0; i < line.Length; i++)
            {

                var c = line[i];
                if (c == '\n')
                {
                    if (start < i)
                    {
                        yield return (start, i - 1, lastwordtype);
                    }
                    yield return (i, i, WordType.Linebreak);
                    start = i + 1;
                }
                else if (c == '\r')
                {
                    // ignore \r
                    if (start < i)
                    {
                        yield return (start, i - 1, lastwordtype);
                    }
                    start = i + 1;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (lastwordtype != WordType.Whitespace)
                    {
                        if (start < i)
                        {
                            yield return (start, i - 1, lastwordtype);
                            start = i;
                        }
                    }
                    lastwordtype = WordType.Whitespace;
                }
                else
                {
                    if (lastwordtype == WordType.Whitespace)
                    {
                        if (start < i)
                        {
                            yield return (start, i - 1, lastwordtype);
                            start = i;
                        }
                    }
                    else
                    {
                        var category = char.GetUnicodeCategory(c);
                        if (category == UnicodeCategory.DashPunctuation)
                        {
                            if (start < i)
                            {
                                yield return (start, i - 1, lastwordtype);
                                start = i;
                            }
                        }
                    }
                    lastwordtype = WordType.Word;
                }

            }

            if (start < line.Length)
                yield return (start, line.Length - 1, lastwordtype);

        }

    }

}
