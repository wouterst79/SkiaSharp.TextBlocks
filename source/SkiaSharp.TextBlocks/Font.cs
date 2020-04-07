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
        /// True to use SKPaint.FakeBoldText for bold fonts.
        /// Default: false
        /// </summary>
        public static bool UseFakeBoldText = false;


        public string Name;
        public float TextSize;
        public bool Bold;

        public SKFontStyle GetSKFontStyle() => Bold && !UseFakeBoldText ? SKFontStyle.Bold : SKFontStyle.Normal;

        public Font(float textSize, bool bold = false) : this(null, textSize, bold)
        {
        }

        public Font(string name, float textSize, bool bold = false)
        {
            Name = name;
            TextSize = textSize;
            Bold = bold;
        }

        public static Font FromPaint(SKPaint paint) => new Font(paint.Typeface?.FamilyName, paint.TextSize, paint.Typeface?.IsBold ?? paint.FakeBoldText);

        public override bool Equals(object obj) => obj is Font font && Name == font.Name && TextSize == font.TextSize && Bold == font.Bold;

        public override int GetHashCode()
        {
            var hashCode = -274887227;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + TextSize.GetHashCode();
            hashCode = hashCode * -1521134295 + Bold.GetHashCode();
            return hashCode;
        }


        /// <summary>
        /// Get the typefaces needed to print the character at position 
        /// This supports finding the appropriate typeface for many characters, including ݐ, 年, ↺, and 🚀
        /// It doesn't currently handle mixed text very well, but it's doing ok.
        /// </summary>
        public (SKTypeface[] typefaces, byte[] ids) GetTypefaces(string text, SKFontManager fontManager)
        {

            using (var fontstyle = GetSKFontStyle())
            {

                var typefaces = new List<SKTypeface>();
                var ids = new byte[text.Length];

                for (int i = 0; i < text.Length; i++)
                {

                    var ch = text[i];

                    SKTypeface typeface;
                    if (char.IsSurrogate(ch) && text.Length > i + 1 && char.IsSurrogatePair(ch, text[i + 1]))
                    {

                        // handle surrogates

                        var id = StringUtilities.GetUnicodeCharacterCode(text.Substring(i, 2), SKTextEncoding.Utf32);
                        typeface = fontManager.MatchCharacter(Name, fontstyle, null, id);

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

                        typeface = fontManager.MatchCharacter(Name, fontstyle, null, ch);

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
}
