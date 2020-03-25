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

        public string Name;
        public float TextSize;
        public bool Bold;

        public SKFontStyle GetSKFontStyle() => Bold ? SKFontStyle.Bold : SKFontStyle.Normal;

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
        /// Get the typeface that best matches the (first few characters in a) string.
        /// This supports finding the appropriate typeface for many characters, including ݐ, 年, ↺, and 🚀
        /// </summary>
        public SKTypeface GetTypeface(string text)
        {

            text = text.Trim();

            var ch = text.Length == 0 ? 'a' : text[0];
            if (ch <= 256)
            {
                ch = 'a';
                var typeface = SKFontManager.Default.MatchCharacter(Name, GetSKFontStyle(), null, ch);
                if (typeface == null) typeface = SKTypeface.CreateDefault();
                return typeface;
            }
            else
            {

                SKTypeface typeface;

                // handle surrogates
                if (char.IsSurrogate(ch) && text.Length > 1 && char.IsSurrogatePair(ch, text[1]))
                {
                    var id = StringUtilities.GetUnicodeCharacterCode(text.Substring(0, 2), SKTextEncoding.Utf32);
                    typeface = SKFontManager.Default.MatchCharacter(Name, GetSKFontStyle(), null, id);
                }
                else
                    typeface = SKFontManager.Default.MatchCharacter(Name, GetSKFontStyle(), null, ch);

                if (typeface == null)
                    typeface = SKTypeface.CreateDefault();

                return typeface;

            }
        }

    }
}
