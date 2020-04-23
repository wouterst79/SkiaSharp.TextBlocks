using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp.TextBlocks
{

    /// <summary>
    /// A part of the text in a rich text block
    /// </summary>
    public class RichTextSpan
    {

        /// <summary>
        /// The text to print
        /// </summary>
        public TextBlock TextBlock;

        /// <summary>
        /// X, and Y translation to apply before printing text in this span
        /// </summary>
        public SKPoint Translate;

    }
}
