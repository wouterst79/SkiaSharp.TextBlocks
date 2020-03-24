using AnimatedGif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkiaSharp.TextBlock.Samples
{

    public class TextBlockSampleAnimated
    {

        public string FullFilename;
        public SKPngEncoderOptions EncoderOptions;
        public int Width;
        public int Height;
        public int MSBetweenFrame;

        public AnimatedGifCreator AnimatedGifCreator;

        public List<(Func<SKCanvas, float, float, SKRect> drawsample, string description)> Images = new List<(Func<SKCanvas, float, float, SKRect> drawsample, string description)>();

        public float Y;

        public SKSurface Surface;

        public TextBlockSampleAnimated(string folder, string filename, int width, int height, int msbetweenframe)
        {

            // save options = PNG
            EncoderOptions = new SKPngEncoderOptions();
            FullFilename = Path.Combine(folder, filename + ".gif");

            Width = width;
            Height = height;
            MSBetweenFrame = msbetweenframe;

            // delete existing
            if (File.Exists(FullFilename))
                File.Delete(FullFilename);

            // create a new gif
            Surface = SKSurface.Create(new SKImageInfo(Width, 3000, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            var canvas = Surface.Canvas;

            // start with white
            canvas.Clear(SKColors.White);


        }

        public TextBlockSampleAnimated Paint(Func<SKCanvas, float, float, SKRect> drawsample, string description)
        {

            // add image to the stack
            Images.Add((drawsample, description));

            return this;
        }

        private void Draw(SKCanvas canvas, Func<SKCanvas, float, float, SKRect> drawsample, string description, float pct)
        {

            // draw the sample
            var rect = drawsample(canvas, pct, Y);

            // rect around the output
            using (var rectpaint = new SKPaint() { Color = SKColors.DarkGreen.WithAlpha(64), IsStroke = true })
                canvas.DrawRect(rect, rectpaint);

            Y = rect.Bottom;

            // description below the sample
            rect = canvas.DrawTextBlock(description, new SKRect(0, Y, Width, 0), new FLFont(10), SKColors.DarkGray);

            Y = rect.Bottom + 10;

        }

        public void Save(int FrameCount)
        {

            var gif = AnimatedGif.AnimatedGif.Create(FullFilename, MSBetweenFrame);
            for (int i = 0; i < FrameCount; i++)
            {

                using (var frame = SKSurface.Create(new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
                {

                    Y = 0;

                    // clear the frame
                    frame.Canvas.Clear(SKColors.White);

                    // determine pct, with bounce
                    var pct = ((float)i * 2) / FrameCount;
                    if (pct > 1) pct = 1 - (pct - 1);

                    System.Diagnostics.Debug.WriteLine(pct);

                    // draw all images on the frame
                    foreach (var img in Images)
                    {
                        Draw(frame.Canvas, img.drawsample, img.description, pct);
                    }

                    using (var image = frame.Snapshot().ToBitmap())
                        gif.AddFrame(image);

                }


            }

            gif.Dispose();

        }

    }
}