using SkiaSharp.TextBlock.Enum;
using System;
using System.Diagnostics;
using System.IO;

namespace SkiaSharp.TextBlock.Samples
{
    class Program
    {
        static void Main(string[] args)
        {

            // make output folder
            var outputfolder = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\output");
            outputfolder = new DirectoryInfo(outputfolder).FullName;
            Directory.CreateDirectory(outputfolder);


            // basic samples
            new TextBlockSample(outputfolder, "basic samples", 200)

                .Paint((canvas, y) =>
                {
                    return canvas.DrawTextBlock("Hello world!", new SKRect(0, y, 100, 0), new FLFont(14), SKColors.Black);
                }, "canvas.DrawTextBlock(\"Hello world!\", new SKRect(0, 0, 100, 0), new FLFont(14), SKColors.Black);")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0), null, FlowDirection.RightToLeft);
                }, "FlowDirection.RightToLeft")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));
                }, "Word Wrap")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.Center);
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));
                }, "LineBreakMode.Center (Width=50)")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.MiddleTruncation);
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));
                }, "LineBreakMode.MiddleTruncation (Width=50)")

                .Save();



            // basic samples 2
            new TextBlockSample(outputfolder, "basic samples 2", 200)

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 20, 0));
                }, "Word wrap, smaller")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont("Courier New", 14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0));
                }, "Courier New")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(20), SKColors.Red, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0));
                }, "Color and Size")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, @"
(leading) new-line support...

Hello World!
SkiaSharp Rocks!");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 150, 0));
                }, "New line support")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, @"Trailing new-line support:

");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 150, 0));
                }, "trailing new line support")

                .Save();


            // font detection
            new TextBlockSample(outputfolder, "font detection", 200)

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "年");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0));
                }, "font detection - non-latin")

                .Paint((canvas, y) =>
                 {
                     var text = new TextBlock(new FLFont(14), SKColors.Black, "↺");
                     return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0));
                 }, "font detection - symbols")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "🌐🍪🍕🚀");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0));
                }, "unicode support")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0), null, FlowDirection.RightToLeft);
                }, "rtl support")

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
                    return canvas.DrawTextBlock(text, new SKRect(50, y, 100, 0), null, FlowDirection.RightToLeft);
                }, "rtl word wrap")

                .Save();


            // rich text samples
            new TextBlockSample(outputfolder, "rich text", 200)

                .Paint((canvas, y) =>
                {
                    var text = new RichTextBlock()
                    {
                        Spans =
                        {
                            new TextBlock(new FLFont(10), SKColors.Black, "Hello "),
                            new TextBlock(new FLFont(20, true), SKColors.Black, "world! "),
                            new TextBlock(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                        }
                    };
                    return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
                }, "rich text")

                .Save();

            // rich text 2
            new TextBlockSample(outputfolder, "rich text 2", 200)

                .Paint((canvas, y) =>
                {
                    var text = new RichTextBlock()
                    {
                        Spans =
                        {
                            new TextBlock(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new TextBlock(new FLFont(10), SKColors.Black, "Hello "),
                            new TextBlock(new FLFont(20, true), SKColors.Black, "world! "),
                            new TextBlock(new FLFont(14), SKColors.Black, @"Trailing new-line support:

"),
                            new TextBlock(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new TextBlock(new FLFont(14), SKColors.Black, "مرحبا بالعالم"),
                            new TextBlock(new FLFont(14), SKColors.Black, "年"),
                            new TextBlock(new FLFont(14), SKColors.Black, "↺"),
                        }
                    };
                    return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
                }, "rich text 2")

                .Save();


            var lorum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.

Curabitur pretium tincidunt lacus. Nulla gravida orci a odio.Nullam varius, turpis et commodo pharetra, est eros bibendum elit, nec luctus magna felis sollicitudin mauris.Integer in mauris eu nibh euismod gravida.Duis ac tellus et risus vulputate vehicula.Donec lobortis risus a elit.Etiam tempor. Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam.Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi.Duis sapien sem, aliquet nec, commodo eget, consequat quis, neque. Aliquam faucibus, elit ut dictum aliquet, felis nisl adipiscing sapien, sed malesuada diam lacus eget erat.Cras mollis scelerisque nunc. Nullam arcu. Aliquam consequat. Curabitur augue lorem, dapibus quis, laoreet et, pretium ac, nisi. Aenean magna nisl, mollis quis, molestie eu, feugiat in, orci.In hac habitasse platea dictumst.;
";

            // font detection
            new TextBlockSample(outputfolder, "lorum ipsum", 400)

                .Paint((canvas, y) =>
                {
                    var text = new TextBlock(new FLFont(14), SKColors.Black, lorum);
                    return canvas.DrawTextBlock(text, new SKRect(0, y, 400, 0));
                }, "lorum ipsum")

                .Save();



            // open the output folder
            Process.Start(new ProcessStartInfo() { FileName = outputfolder, UseShellExecute = true });

        }
    }
}
