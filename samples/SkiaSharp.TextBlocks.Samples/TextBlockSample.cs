using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkiaSharp.TextBlocks.Samples
{

    public class TextBlockSample
    {

        public string Folder;
        public string Section;

        public StringBuilder Markdown;

        public TextBlockSample(string folder, string section, StringBuilder markdown)
        {

            Folder = folder;
            Section = section;

            Markdown = markdown;
            Markdown.AppendLine($"## {section}");

        }

        public TextBlockSample Paint(string name, int width, Func<SKCanvas, SKRect> drawsample, string code)
        {

            var filename = string.IsNullOrEmpty(name) ? Section : $"{Section}-{name}";
            filename = filename.Replace(" ", "_");
            var FullFilename = Path.Combine(Folder, $"{filename}.png");

            // delete existing
            if (File.Exists(FullFilename))
                File.Delete(FullFilename);

            // create a surface
            using (var Surface = SKSurface.Create(new SKImageInfo(width, 3000, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
            {

                var canvas = Surface.Canvas;

                // start with white
                canvas.Clear(SKColors.White);

                // draw the sample
                var rect = drawsample(canvas);

                // rect around the output
                using (var rectpaint = new SKPaint() { Color = SKColors.DarkGreen.WithAlpha(64), IsStroke = true })
                    canvas.DrawRect(rect, rectpaint);

                var y = rect.Bottom + 1;

                //// description below the sample
                //rect = canvas.DrawTextBlock(name, new SKRect(0, y, width, 0), new Font(10), SKColors.DarkGray);
                //y = rect.Bottom;



                // save
                using (var resized = SKSurface.Create(new SKImageInfo(width, (int)y, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
                {

                    // resize to fit sample
                    resized.Canvas.DrawImage(Surface.Snapshot(), 0, 0, new SKPaint());

                    // save the sample
                    using (var outstream = new FileStream(FullFilename, FileMode.Create))
                    using (var pixmap = resized.Snapshot().PeekPixels())
                    using (var data = pixmap.Encode(new SKPngEncoderOptions()))
                        data.SaveTo(outstream);

                }


            }

            // Markdown
            if (!string.IsNullOrEmpty(name))
                Markdown.AppendLine(@$"{name}:
");
            Markdown.AppendLine($@"![{filename}](./samples/output/{filename}.png)
");
            if (!string.IsNullOrEmpty(code))
                Markdown.AppendLine(@$"```C#
{code}
```
");

            return this;
        }

    }
}