using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            Buffer buffer;
            var direction = FlowDirection.Unknown;
            var startpointlength = text.Length + 1; // default point buffer length
            var startpoints = new SKPoint[startpointlength];
            var codepoints = new byte[startpointlength * 2];
            var glyphcount = 0;

            var words = new List<(int startglyph, int endglyph, WordType type)>();
            using (buffer = new Buffer())
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
                    ShapeWord(word.type);

                }

            }

            return new GlyphSpan(paint, direction, codepoints, startpoints, glyphcount, words);

            void ShapeWord(WordType wordType)
            {

                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));

                // do the shaping
                font.Shape(buffer);

                // get the shaping results
                var len = buffer.Length; // note that the length after shaping may be different from the length before shaping (shorter, or longer)
                var info = buffer.GlyphInfos;
                var pos = buffer.GlyphPositions;

                if (glyphcount + len >= startpoints.Length)
                {

                    // when the word produce more glyphs than fit in the buffers (IE, in Thai), resize the buffers.

                    startpointlength = glyphcount + len + startpointlength / 2 + 1;
                    var newstartpoints = new SKPoint[startpointlength];
                    var newcodepoints = new byte[startpointlength * 2];

                    int s;
                    if (direction == FlowDirection.LeftToRight)
                        s = 0;
                    else
                        s = newstartpoints.Length - startpoints.Length;

                    for (int i = 0; i < startpoints.Length; i++)
                    {
                        newstartpoints[s + i] = startpoints[i];
                        newcodepoints[(s + i) * 2] = codepoints[i * 2];
                        newcodepoints[(s + i) * 2 + 1] = codepoints[i * 2 + 1];
                    }

                    startpoints = newstartpoints;
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

                // add the shaped word
                words.Add((glyphcount, glyphcount + len - 1, wordType));

                // advance cursor
                glyphcount += len;

            }
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
                        var category = char.GetUnicodeCategory(c);
                        var isnonlinebreakpunctuation = char.IsPunctuation(c) && category != UnicodeCategory.DashPunctuation;
                        if (!char.IsLetterOrDigit(c) && !char.IsSurrogate(c) && !isnonlinebreakpunctuation && category != UnicodeCategory.NonSpacingMark)
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
