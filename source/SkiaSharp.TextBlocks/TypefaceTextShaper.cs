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

        public void Shape(Buffer buffer)
        {
            font.Shape(buffer);
        }

    }
}
