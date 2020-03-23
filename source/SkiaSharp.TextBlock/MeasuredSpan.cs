namespace SkiaSharp.TextBlock
{
    public struct MeasuredSpan
    {
        public int glyphstart;
        public int glyphend;
        public int lastglyph;
        public float width;

        public MeasuredSpan(int glyphstart, int glyphend, int lastglyph, float width)
        {
            this.glyphstart = glyphstart;
            this.glyphend = glyphend;
            this.lastglyph = lastglyph == -1 ? glyphend : lastglyph;
            this.width = width;
        }
    }
}
