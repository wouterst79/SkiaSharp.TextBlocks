using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarfBuzzSharp;
using SkiaSharp.HarfBuzz;
using SkiaSharp.TextBlocks.Enum;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.TextBlocks
{

    /// <summary>
    /// This class interfaces with HarfBuzzSharp to produce a GlyphSpan, which contains glyph, word, and coordinate information
    /// </summary>
    public class TypefaceTextShaper : IDisposable
    {

        internal const int FONT_SIZE_SCALE = 512;


        private HarfBuzzSharp.Font font;

        public SKTypeface Typeface { get; private set; }

        public void Dispose() => font?.Dispose();


        public TypefaceTextShaper(SKTypeface typeface)
        {
            Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));

            int index;
            using (var blob = Typeface.OpenStream(out index).ToHarfBuzzBlob())
            using (var face = new Face(blob, index))
            {
                face.Index = index;
                face.UnitsPerEm = Typeface.UnitsPerEm;

                font = new HarfBuzzSharp.Font(face);
                font.SetScale(FONT_SIZE_SCALE, FONT_SIZE_SCALE);

                font.SetFunctionsOpenType();
            }
        }

        /// <summary>
        /// Break a string into words, and then uses HarfBuzzSharp to convert them into glyphs, and glyph coordinates.
        /// </summary>
        public GlyphSpan Shape(SKPaint paint, string text)
        {

            if (string.IsNullOrEmpty(text))
                return new GlyphSpan(paint);

            // get the sizes
            float scaley = paint.TextSize / FONT_SIZE_SCALE;
            float scalex = scaley * paint.TextScaleX;

            // prepare the output buffers
            var direction = FlowDirection.Unknown;
            var startpointlength = text.Length + 1;
            var startpoints = new SKPoint[startpointlength];
            var codepoints = new byte[startpointlength * 2];
            var glyphcount = 0;

            var words = new List<(int startglyph, int endglyph, WordType type)>();
            using (var buffer = new Buffer())
            {

                // determine direction
                buffer.AddUtf8(text);
                buffer.GuessSegmentProperties();
                if (buffer.Direction == Direction.LeftToRight || buffer.Direction == Direction.RightToLeft)
                    direction = buffer.Direction == Direction.LeftToRight ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                else
                    Console.WriteLine("only Direction.LeftToRight and Direction.RightToLeft are currently supported.");

                // count and add words.
                foreach (var word in GetWords(text))
                {

                    // add 1 word to the buffer
                    buffer.ClearContents();
                    buffer.AddUtf8(word.word);
                    buffer.GuessSegmentProperties();

                    // shape the word
                    var count = ShapeWord(buffer, startpoints, codepoints, glyphcount, scalex, scaley, direction);

                    // capture word glyph coordinates
                    words.Add((glyphcount, glyphcount + count - 1, word.type));

                    glyphcount += count;

                }

            }

            return new GlyphSpan(paint, direction, codepoints, startpoints, glyphcount, words);
        }


        /// <summary>
        /// Shape 1 word.
        /// </summary>
        /// <returns>The number of glyphs in the word</returns>
        public int ShapeWord(Buffer buffer, SKPoint[] startpoints, byte[] codepoints, int start, float scalex, float scaley, FlowDirection readDirection)
        {

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // do the shaping
            font.Shape(buffer);

            // get the shaping results
            var len = buffer.Length; // note that the length after shaping may be different from the length before shaping
            var info = buffer.GlyphInfos;
            var pos = buffer.GlyphPositions;


            if (readDirection != FlowDirection.RightToLeft)
            {

                // Default & LTR
                float x = startpoints[start].X;
                float y = startpoints[start].Y;
                for (var i = 0; i < len; i++)
                {

                    var glyph = start + i;

                    var bytes = BitConverter.GetBytes((ushort)info[i].Codepoint);
                    codepoints[glyph * 2] = bytes[0];
                    codepoints[glyph * 2 + 1] = bytes[1];

                    startpoints[glyph] = new SKPoint(x + pos[i].XOffset * scalex, y + pos[i].YOffset * scaley);

                    // move the cursor
                    x += pos[i].XAdvance * scalex;
                    y += pos[i].YAdvance * scaley;

                }

                startpoints[start + len] = new SKPoint(x, y);

            }
            else
            {

                // RTL: fill out startpoints in reverse order

                var idx = startpoints.Length - 1 - start;

                var x = startpoints[idx].X;
                var y = startpoints[idx].Y;
                for (var i = len - 1; i >= 0; i--)
                {

                    var glyph = idx - len + i;

                    var bytes = BitConverter.GetBytes((ushort)info[i].Codepoint);
                    codepoints[glyph * 2] = bytes[0];
                    codepoints[glyph * 2 + 1] = bytes[1];

                    // move the cursor
                    x -= pos[i].XAdvance * scalex;
                    y -= pos[i].YAdvance * scaley;

                    startpoints[glyph] = new SKPoint(x - pos[i].XOffset * scalex, y - pos[i].YOffset * scaley);

                }

                startpoints[idx - len] = new SKPoint(x, y);

            }

            return len;
        }

        /// <summary>
        /// Breaks a string into "words", including "whitespace" words, and "newline" words
        /// </summary>
        private IEnumerable<(string word, WordType type)> GetWords(string line)
        {

            var word = "";
            var lastwordtype = WordType.Word;
            for (var i = 0; i < line.Length; i++)
            {

                var c = line[i];
                if (c == '\n')
                {
                    if (word.Length > 0)
                    {
                        yield return (word, lastwordtype);
                        word = "";
                    }
                    yield return ("$", WordType.Linebreak);
                }
                else if (c == '\r')
                {
                    // ignore
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (lastwordtype != WordType.Whitespace)
                    {
                        if (word.Length > 0)
                        {
                            yield return (word, lastwordtype);
                            word = "";
                        }
                    }
                    lastwordtype = WordType.Whitespace;
                    word += c;
                }
                else
                {
                    if (lastwordtype == WordType.Whitespace)
                    {
                        if (word.Length > 0)
                        {
                            yield return (word, lastwordtype);
                            word = "";
                        }
                    }
                    else
                    {
                        var isnonlinebreakpunctuation = char.IsPunctuation(c) && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.DashPunctuation;
                        if (!char.IsLetterOrDigit(c) && !char.IsSurrogate(c) && !isnonlinebreakpunctuation)
                        {
                            if (word.Length > 0)
                            {
                                yield return (word, lastwordtype);
                                word = "";
                            }
                        }
                    }
                    lastwordtype = WordType.Word;
                    word += c;
                }

            }

            if (word.Length > 0)
                yield return (word, lastwordtype);

        }

    }
}
