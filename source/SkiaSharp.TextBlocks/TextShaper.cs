using SkiaSharp;
using SkiaSharp.TextBlocks.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private const int charsfortypefacecache = 10;

        public Dictionary<char, SKTypeface> TypefaceCache;
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
                TypefaceCache = new Dictionary<char, SKTypeface>();
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
            {
                // Typefaces may be shared between multiple caches if the default font manager is used.
                // in this case, don't dispose them (and let them be garbage collected instead)

                if (TypefaceCache != null)
                    foreach (var typeface in TypefaceCache.Values)
                        typeface.Dispose();

                FontManager.Dispose();

            }

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

                var typeface = GetTypeface(font, text);
                var shaper = GetFontShaper(typeface);

                if (typeface == null)
                {
                    System.Diagnostics.Debug.WriteLine($"{font} - {typeface} - {text}");
                }
                //System.Diagnostics.Debug.WriteLine($"{font} - {typeface} - {typeface?.FamilyName} - {text}");

                var paint = new SKPaint()
                {
                    TextSize = font.TextSize * Scale,
                    Typeface = typeface,
                    TextEncoding = SKTextEncoding.GlyphId,
                    IsAntialias = IsAntiAlias,
                    FakeBoldText = font.Bold && Font.UseFakeBoldText,
                };

                shape = shaper.Shape(paint, text);

                if (GlyphSpanCache != null)
                    GlyphSpanCache.Add((font, text), shape);

            }

            return shape;

        }



        private SKTypeface GetTypeface(Font font, string text)
        {

            // no cache
            if (TypefaceCache == null)
                return font.GetTypeface(text, FontManager);


            // use the cache
            text = text.Trim();
            var ch = text.Length == 0 ? 'a' : text[0];
            if (ch <= 256)
            {
                ch = 'a';
                if (!TypefaceCache.TryGetValue(ch, out var typeface))
                    TypefaceCache.Add(ch, typeface = font.GetTypeface(text, FontManager));

                return typeface;
            }
            else
            {

                SKTypeface typeface;
                for (var i = 0; i < text.Length && i < charsfortypefacecache; i++)
                    if (TypefaceCache.TryGetValue(text[i], out typeface))
                        return typeface;

                typeface = font.GetTypeface(text, FontManager);

                for (var i = 0; i < text.Length && i < charsfortypefacecache; i++)
                    if (!TypefaceCache.ContainsKey(text[i]))
                        TypefaceCache.Add(text[i], typeface);

                return typeface;

            }
        }

        private TypefaceTextShaper GetFontShaper(SKTypeface typeface)
        {

            if (TypeShaperCache == null)
                return new TypefaceTextShaper(typeface);

            if (!TypeShaperCache.TryGetValue(typeface, out var shaper))
                TypeShaperCache.Add(typeface, shaper = new TypefaceTextShaper(typeface));

            return shaper;

        }

    }

}
