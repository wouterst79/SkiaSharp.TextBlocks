using SkiaSharp.TextBlock.Enum;
using System;
using System.Collections.Generic;

namespace SkiaSharp.TextBlock
{

    /// <summary>
    /// Contains all the information necessary to quickly measure and draw parts (or all) of a string.
    /// </summary>
    public class GlyphSpan : IDisposable
    {

        public readonly SKPaint Paint;

        /// <summary>
        /// The direction the span should be read in.
        /// Note that for RLT strings, the first word in the string will be printed on the right.
        /// IOW, Words are 0 is first word, but Glyphs in the span are ReadDirection dependant.
        /// This is important for correct line breaking for RTL strings.
        /// </summary>
        public readonly FlowDirection ReadDirection;

        // RTL: 
        //                     word2abc  word0
        //                               ^   ^
        //                               |   first glyph:[0]
        //                               last glyph: [4] 
        // 
        // glyph               15            0
        // StartPoints.Index   0             15
        // StartPoints.x:    -100            0
        // codepoints          0             30
        //                              
        private readonly byte[] Codepoints; // due to the way HarfBuzz works, these are always LTR
        private readonly SKPoint[] StartPoints;

        /// <summary>
        /// All the words in the set.
        /// </summary>
        public readonly (int firstglyph, int lastglyph, WordType type)[] Words;

        /// <summary>
        /// The number of glyphs in the whole set.
        /// Note that this may be different from the number of characters in the original string.
        /// </summary>
        public readonly int GlyphCount;

        /// <summary>
        /// The number of words.
        /// </summary>
        public readonly int WordCount;

        public GlyphSpan()
        {
            Codepoints = new byte[0];
            StartPoints = new SKPoint[0];
            Words = new (int, int, WordType)[0];
        }

        public void Dispose() => Paint.Dispose();

        public GlyphSpan(SKPaint paint, FlowDirection readDirection, byte[] codepoints, SKPoint[] startpoints, int glyphcount, List<(int firstglyph, int lastglyph, WordType type)> words)
        {
            Paint = paint;
            ReadDirection = readDirection == FlowDirection.Unknown ? FlowDirection.LeftToRight : readDirection;
            Codepoints = codepoints;
            StartPoints = startpoints;
            GlyphCount = glyphcount; // note that the startpoints array in some scenario's isn't fully filled out, and glyphcount may be different from StartPoints.Length
            Words = words.ToArray();
            WordCount = Words.Length;
        }


        /// <summary>
        /// Calculate the full extent of the span, optionally removing trailing white space.
        /// Note that linebreak words are always trimmed from the end. 
        /// The caller is responsible for making sure no linebreak words are embedded in the requested span.
        /// </summary>
        /// <param name="wordstart">the index of the first word to measure</param>
        /// <param name="wordend"></param>
        /// <param name="trimtrailingwhitespace"></param>
        /// <returns></returns>
        public MeasuredSpan MeasureWordSpan(int wordstart, int wordend, bool trimtrailingwhitespace = false)
        {

            var lastglyph = Words[wordend].lastglyph; // last measured glyph (ie, including whitespace, and line breaks)
            var start = Words[wordstart].firstglyph; // first printed glyph

            if (wordend < wordstart)
                return new MeasuredSpan(start, -1, lastglyph, 0);

            if (trimtrailingwhitespace)
                while (wordend > -1 && Words[wordend].type != WordType.Word)
                    wordend--; // trim trailing whitespace and line breaks
            else
                while (wordend > -1 && Words[wordend].type == WordType.Linebreak)
                    wordend--; // trim trailing line breaks only


            if (wordend < wordstart)
                return new MeasuredSpan(start, -1, lastglyph, 0);

            var end = Words[wordend].lastglyph; // last printed glyph (ie, excluding whitespace, and line breaks)

            return Measure(start, end, lastglyph);

        }

        /// <summary>
        /// Calculate the full extent of the span, optionally removing trailing white space.
        /// Note that linebreak words are always trimmed from the end. 
        /// The caller is responsible for making sure no linebreak words are embedded in the requested span.
        /// </summary>
        /// <param name="wordstart">the index of the first word to measure</param>
        /// <param name="wordend"></param>
        /// <param name="trimtrailingwhitespace"></param>
        /// <returns></returns>
        public MeasuredSpan MeasureGlyphToWordSpan(int start, int wordend, bool trimtrailingwhitespace = false)
        {

            var lastglyph = Words[wordend].lastglyph; // last measured glyph (ie, including whitespace, and line breaks)

            if (trimtrailingwhitespace)
                while (wordend > -1 && Words[wordend].type != WordType.Word)
                    wordend--; // trim trailing whitespace and line breaks
            else
                while (wordend > -1 && Words[wordend].type == WordType.Linebreak)
                    wordend--; // trim trailing line breaks only

            var end = Words[wordend].lastglyph; // last printed glyph (ie, excluding whitespace, and line breaks)

            return Measure(start, end, lastglyph);

        }

        /// <summary>
        /// Calculate the measure of a (zero based) span of glyphs.
        /// </summary>
        public MeasuredSpan Measure(int firstglyph, int lastglyph)
        {
            return Measure(firstglyph, lastglyph, lastglyph);
        }

        private MeasuredSpan Measure(int firstglyph, int lastglyph, int lastmeasuredglyph)
        {
            if (ReadDirection == FlowDirection.LeftToRight)
            {
                var xstart = StartPoints[firstglyph].X;
                var xend = StartPoints[lastglyph + 1].X;
                return new MeasuredSpan(firstglyph, lastglyph, lastmeasuredglyph, xend - xstart);
            }
            else
            {
                var pstart = StartPoints.Length - lastglyph - 2;
                var pend = StartPoints.Length - firstglyph - 1;
                var xstart = StartPoints[pstart].X;
                var xend = StartPoints[pend].X;
                return new MeasuredSpan(firstglyph, lastglyph, lastmeasuredglyph, xend - xstart);
            }
        }

        /// <summary>
        /// GetBlock is useful for preparing a part of the coordinate set for printing.
        /// </summary>
        public (byte[] bytes, SKPoint[] points) GetBlock(int firstglyph, int lastglyph, float x, float y)
        {

            var s = firstglyph;
            var e = lastglyph;

            var len = e - s + 1;
            var bytes = new byte[len * 2];

            var points = new SKPoint[len];

            var start = (ReadDirection == FlowDirection.LeftToRight) ? s : StartPoints.Length - e - 2;
            Buffer.BlockCopy(Codepoints, start * 2, bytes, 0, len * 2);

            var deltax = x - StartPoints[start].X;
            deltax = (float)Math.Round(deltax);

            for (var i = 0; i < len; i++)
            {
                var sp = StartPoints[start + i];
                points[i] = new SKPoint(deltax + sp.X, sp.Y + y);
            }

            return (bytes, points);

        }

    }

}
