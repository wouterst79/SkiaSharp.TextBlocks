using SkiaSharp.TextBlock.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp.TextBlock
{
    public static class CanvasExtensions
    {

        /// <summary>
        /// Draw a measuredspan (ie a substring) of the glyphspan
        /// </summary>
        /// <param name="x">Left coordinate</param>
        /// <param name="y">Bottom coordinate of the text baseline</param>
        /// <param name="measuredSpan"></param>
        public static void DrawGlyphSpan(this SKCanvas canvas, GlyphSpan glyphSpan, float x, float y, SKColor color, MeasuredSpan measuredSpan)
        {

            if (canvas == null)
                return;

            if (measuredSpan.glyphstart < 0)
                return;

            if (measuredSpan.glyphend < 0)
                return;

            // calculate the block ("substring")
            var block = glyphSpan.GetBlock(measuredSpan.glyphstart, measuredSpan.glyphend, x, y);

            // paint the block
            var paint = glyphSpan.Paint;
            paint.Color = color;

            canvas.DrawPositionedText(block.bytes, block.points, paint);

        }


        /// <summary>
        /// Draw a text block 
        /// </summary>
        public static SKRect DrawTextBlock(this SKCanvas canvas, string text, SKRect rect, FLFont font, SKColor color, TextShaper textShaper = null)
        {
            var textblock = new TextBlock(font, color, text);
            return DrawTextBlock(canvas, textblock, rect, textShaper);
        }

        /// <summary>
        /// Draw a block of text on the canvas.
        /// </summary>
        /// <returns>The bounds of the painted text. Note that the returned rectangle's Width is equal to the input (IE only bottom is calculated). Use TextBlock.Measure to get text bounds</returns>
        public static SKRect DrawTextBlock(this SKCanvas canvas, TextBlock text, SKRect rect, TextShaper textShaper = null, FlowDirection flowDirection = FlowDirection.LeftToRight)
        {
            return text.Draw(canvas, rect, textShaper, flowDirection);
        }

        /// <summary>
        /// Draw a block of text on the canvas.
        /// </summary>
        /// <returns>The bounds of the painted text. Note that the returned rectangle's Width is equal to the input (IE only bottom is calculated). Use TextBlock.Measure to get text bounds</returns>
        public static SKRect DrawRichTextBlock(this SKCanvas canvas, RichTextBlock text, SKRect rect, TextShaper textShaper = null, FlowDirection flowDirection = FlowDirection.LeftToRight)
        {
            return text.Paint(canvas, rect, flowDirection, textShaper);
        }



    }
}
