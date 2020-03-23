using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkiaSharp.TextBlock.Samples
{

    public class TextBlockSample
    {

        public string FullFilename;
        public SKPngEncoderOptions EncoderOptions;
        public int Width;

        public float Y;

        public SKSurface Surface;

        public TextBlockSample(string folder, string filename, int width)
        {

            //EncoderOptions = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 90);
            //FullFilename = Path.Combine(outputfolder, filename + ".webp");

            EncoderOptions = new SKPngEncoderOptions();
            FullFilename = Path.Combine(folder, filename + ".png");

            Width = width;

            if (File.Exists(FullFilename))
                File.Delete(FullFilename);

            Surface = SKSurface.Create(new SKImageInfo(width, 3000, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            var canvas = Surface.Canvas;

            canvas.Clear(SKColors.White);

        }

        public TextBlockSample Paint(Func<SKCanvas, float, SKRect> drawsample, string description)
        {

            var canvas = Surface.Canvas;

            // draw the sample
            var rect = drawsample(canvas, Y);

            // rect around the output
            using (var rectpaint = new SKPaint() { Color = SKColors.DarkGreen.WithAlpha(64), IsStroke = true })
                canvas.DrawRect(rect, rectpaint);

            Y = rect.Bottom;

            // description below the sample
            rect = canvas.DrawTextBlock(description, new SKRect(0, Y, Width, 0), new FLFont(10), SKColors.DarkGray);

            Y = rect.Bottom + 10;

            return this;
        }

        public void Save()
        {

            using (var resized = SKSurface.Create(new SKImageInfo(Width, (int)Y, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
            {

                // resize to fit sample
                resized.Canvas.DrawImage(Surface.Snapshot(), 0, 0, new SKPaint());

                // save the sample
                using (var outstream = new FileStream(FullFilename, FileMode.Create))
                using (var pixmap = resized.Snapshot().PeekPixels())
                using (var data = pixmap.Encode(EncoderOptions))
                    data.SaveTo(outstream);

            }

            Surface.Dispose();

        }

    }
}