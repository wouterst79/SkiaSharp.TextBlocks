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
        /// Gets the best character in a string to use to resolve typename
        /// </summary>
        public (char representative, int location) GetRepresentativeCharacter(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch > 256)
                    return (ch, i);
            }
            return ('a', 0);
        }

        /// <summary>
        /// Get the typeface that best matches the representative characters in a string.
        /// This supports finding the appropriate typeface for many characters, including ݐ, 年, ↺, and 🚀
        /// It doesn't currently handle mixed text very well, but it's doing ok.
        /// </summary>
        public SKTypeface GetTypeface(string text, (char representative, int location) character, SKFontManager fontManager)
        {

            var fontstyle = GetSKFontStyle();
            SKTypeface typeface;

            var ch = character.representative;

            // handle surrogates
            if (char.IsSurrogate(ch) && text.Length > character.location + 1 && char.IsSurrogatePair(ch, text[character.location + 1]))
            {
                var id = StringUtilities.GetUnicodeCharacterCode(text.Substring(character.location, 2), SKTextEncoding.Utf32);
                typeface = fontManager.MatchCharacter(Name, GetSKFontStyle(), null, id);
            }
            else
            {
                typeface = fontManager.MatchCharacter(Name, GetSKFontStyle(), null, ch);
            }

            if (typeface == null) typeface = fontManager.MatchFamily(Name, fontstyle);
            if (typeface == null) typeface = SKTypeface.CreateDefault();

            return typeface;

        }

    }
}
