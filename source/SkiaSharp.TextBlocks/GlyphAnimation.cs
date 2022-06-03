using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp.TextBlocks
{
    public class GlyphAnimation
    {

        // glyph, glyphcount, original, transposed
        public Func<int, int, SKPoint, SKPoint> Transpose;
        public Func<int, int, SKColor> GetColor;
        public Action<int, int, SKPaint, bool> UpdatePaint;

    }
}
