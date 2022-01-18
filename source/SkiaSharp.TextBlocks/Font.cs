using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp.TextBlocks
{

    /// <summary>
    /// A font reference that can be used in a dictionary, and doesn't need to be disposed.
    /// </summary>
    public class Font
    {
        /// <summary>
        /// Default font name if none supplied: Default: null = use system default
        /// </summary>
        public static string DefaultFontName;

        public string Name;
        public float TextSize;
        public SKFontStyle FontStyle;
        public float? LineHeight;

        public SKTypeface Typeface;

        public Font(float textSize, bool bold = false) : this(null, textSize, bold)
        {
        }

        public Font(string name, float textSize, bool bold = false)
        {
            Name = name ?? DefaultFontName;
            TextSize = textSize;
            FontStyle = bold ? SKFontStyle.Bold : SKFontStyle.Normal;
        }

        public Font(string name, float textSize, SKFontStyle fontStyle, float? lineHeight = null)
        {
            Name = name ?? DefaultFontName;
            TextSize = textSize;
            FontStyle = fontStyle;
            LineHeight = lineHeight;
        }

        public Font(Font prototype) : this(prototype.Name, prototype.TextSize, prototype.FontStyle)
        {
        }

        public static Font FromPaint(SKPaint paint) => new Font(paint.Typeface?.FamilyName, paint.TextSize, paint.Typeface?.IsBold ?? paint.FakeBoldText);
        public Font WithTextSize(float textSize) => new Font(this) { TextSize = textSize };
        public Font WithBold(float factor) => new Font(this) { FontStyle = new SKFontStyle((int)(FontStyle.Weight * factor), FontStyle.Width, FontStyle.Slant) };

        public override bool Equals(object obj) => obj is Font font && Name == font.Name && TextSize == font.TextSize && FontStyle.Weight == font.FontStyle.Weight && FontStyle.Width == font.FontStyle.Width && FontStyle.Slant == font.FontStyle.Slant;

        public override int GetHashCode()
        {
            var hashCode = -274887227;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + TextSize.GetHashCode();
            hashCode = hashCode * -1521134295 + FontStyle.Weight.GetHashCode();
            hashCode = hashCode * -1521134295 + FontStyle.Width.GetHashCode();
            hashCode = hashCode * -1521134295 + FontStyle.Slant.GetHashCode();
            return hashCode;
        }


        /// <summary>
        /// Get the typefaces needed to print the character at position 
        /// This supports finding the appropriate typeface for many characters, including ݐ, 年, ↺, and 🚀
        /// It doesn't currently handle mixed text very well, but it's doing ok.
        /// </summary>
        public (SKTypeface[] typefaces, byte[] ids) GetTypefaces(string text, SKFontManager fontManager)
        {

            var typefaces = new List<SKTypeface>();
            var ids = new byte[text.Length];

            for (int i = 0; i < text.Length; i++)
            {

                var ch = text[i];

                SKTypeface typeface = Typeface;
                if (typeface != null)
                {

                    var idx = (byte)typefaces.IndexOf(typeface);
                    if (idx == 255)
                    {
                        typefaces.Add(typeface);
                        idx = (byte)(typefaces.Count - 1);
                    }
                    ids[i] = idx;

                }
                else if (char.IsSurrogate(ch) && text.Length > i + 1 && char.IsSurrogatePair(ch, text[i + 1]))
                {

                    // handle surrogates

                    var id = StringUtilities.GetUnicodeCharacterCode(text.Substring(i, 2), SKTextEncoding.Utf32);
                    typeface = fontManager.MatchCharacter(Name, FontStyle, null, id);

                    if (typeface == null)
                    {
                        if (fontManager == SKFontManager.Default)
                            typeface = SKTypeface.Default;
                        else
                            typeface = SKTypeface.CreateDefault();
                    }

                    var idx = (byte)typefaces.IndexOf(typeface);
                    if (idx == 255)
                    {
                        typefaces.Add(typeface);
                        idx = (byte)(typefaces.Count - 1);
                    }

                    ids[i] = idx;
                    ids[++i] = idx;

                }
                else
                {

                    // single character

                    typeface = fontManager.MatchCharacter(Name, FontStyle, null, ch);

                    if (typeface == null)
                    {
                        if (fontManager == SKFontManager.Default)
                            typeface = SKTypeface.Default;
                        else
                            typeface = SKTypeface.CreateDefault();
                    }

                    var idx = (byte)typefaces.IndexOf(typeface);
                    if (idx == 255)
                    {
                        typefaces.Add(typeface);
                        idx = (byte)(typefaces.Count - 1);
                    }

                    ids[i] = idx;

                }

            }

            if (typefaces.Count == 0)
            {
                // ie: empty string. we still need a typeface, to get font metrics
                typefaces.Add(SKTypeface.Default);
            }

            return (typefaces.ToArray(), ids);

        }

    }
}
