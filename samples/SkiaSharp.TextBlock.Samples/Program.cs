using SkiaSharp.TextBlock.Enum;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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


            var markdown = new StringBuilder();


            // basic samples
            new TextBlockSample(outputfolder, "basic samples", markdown)

                .Paint("Hello World", 200, (canvas) =>
                {
                    return canvas.DrawTextBlock("Hello world!", new SKRect(0, 0, 100, 0), new FLFont(14), SKColors.Black);
                }, "canvas.DrawTextBlock(\"Hello world!\", new SKRect(0, 0, 100, 0), new FLFont(14), SKColors.Black);")

                .Paint("FlowDirection.RightToLeft", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""Hello world!"");
canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);")

                .Paint("Word Wrap", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""Hello world!"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));")

                .Paint("LineBreakMode.Center", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.Center);
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""Hello world!"", LineBreakMode.Center);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));")

                .Paint("LineBreakMode.MiddleTruncation", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.MiddleTruncation);
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""Hello world!"", LineBreakMode.MiddleTruncation);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));")

                            ;


            markdown.AppendLine(@"### word wrap
![animated](./samples/output/animated.gif)
![animated](./samples/output/animated%20rtl.gif)
");


            // basic samples 2
            new TextBlockSample(outputfolder, "basic samples 2", markdown)

                .Paint("Word Wrap - Tight", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 20, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""Hello world!"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 20, 0));")

                .Paint("Courier New", 200, (canvas) =>
                {
                    var text = new Text(new FLFont("Courier New", 14), SKColors.Black, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
                }, @"var text = new Text(new FLFont(""Courier New"", 14), SKColors.Black, ""Hello world!"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));")

                .Paint("Color and Size", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(20), SKColors.Red, "Hello world!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
                }, @"var text = new Text(new FLFont(20), SKColors.Red, ""Hello world!"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0)); ")

                .Paint("New line", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, @"
(leading) new-line support...

Hello World!
SkiaSharp Rocks!");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, @""
(leading) new- line support...

Hello World!
SkiaSharp Rocks!"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));")

                .Paint("New Line - Trailing", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, @"Trailing new-line support:

");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, @""Trailing new- line support:

"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));")

                ;


            // font detection
            new TextBlockSample(outputfolder, "typeface detection", markdown)

                .Paint("Typeface Detection - Non-latin", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "年");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""年"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));")

                .Paint("Typeface Detection - Symbols", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "↺");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""↺"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));")

                .Paint("Unicode", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "🌐🍪🍕🚀");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""🌐🍪🍕🚀"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));")

                .Paint("Rtl Support", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""مرحبا بالعالم"");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);")

                .Paint("Rtl Word Wrap", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
                    return canvas.DrawTextBlock(text, new SKRect(50, 0, 100, 0), null, FlowDirection.RightToLeft);
                }, @"var text = new Text(new FLFont(14), SKColors.Black, ""مرحبا بالعالم"");
return canvas.DrawTextBlock(text, new SKRect(50, 0, 100, 0), null, FlowDirection.RightToLeft);")

                ;


            // rich text samples
            new TextBlockSample(outputfolder, "rich text", markdown)

                .Paint("Rich Text", 200, (canvas) =>
                {
                    var text = new RichText()
                    {
                        Spans =
                        {
                            new Text(new FLFont(10), SKColors.Black, "Hello "),
                            new Text(new FLFont(20, true), SKColors.Black, "world! "),
                            new Text(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                        }
                    };
                    return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
                }, @"var text = new RichText()
{
    Spans =
    {
        new Text(new FLFont(10), SKColors.Black, ""Hello ""),
        new Text(new FLFont(20, true), SKColors.Black, ""world! ""),
        new Text(new FLFont(16), SKColors.Green, ""SkiaSharp Rocks!""),
    }
};
return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
");

                ;

            // rich text 2
            new TextBlockSample(outputfolder, "rich text 2", markdown)

                .Paint("rich text 2", 200, (canvas) =>
                {
                    var text = new RichText()
                    {
                        Spans =
                        {
                            new Text(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new Text(new FLFont(10), SKColors.Black, "Hello "),
                            new Text(new FLFont(20, true), SKColors.Black, "world! "),
                            new Text(new FLFont(14), SKColors.Black, @"Trailing new-line support:

"),
                            new Text(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new Text(new FLFont(14), SKColors.Black, "مرحبا بالعالم"),
                            new Text(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new Text(new FLFont(14), SKColors.Black, "年"),
                            new Text(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
                            new Text(new FLFont(14), SKColors.Black, "↺"),
                        }
                    };
                    return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
                }, @"var text = new RichText()
{
    Spans =
    {
        new Text(new FLFont(16), SKColors.Green, ""SkiaSharp Rocks!""),
        new Text(new FLFont(10), SKColors.Black, ""Hello ""),
        new Text(new FLFont(20, true), SKColors.Black, ""world! ""),
        new Text(new FLFont(14), SKColors.Black, @""Trailing new-line support:

""),
        new Text(new FLFont(16), SKColors.Green, ""SkiaSharp Rocks!""),
        new Text(new FLFont(14), SKColors.Black, ""مرحبا بالعالم""),
        new Text(new FLFont(16), SKColors.Green, ""SkiaSharp Rocks!""),
        new Text(new FLFont(14), SKColors.Black, ""年""),
        new Text(new FLFont(16), SKColors.Green, ""SkiaSharp Rocks!""),
        new Text(new FLFont(14), SKColors.Black, ""↺""),
    }
};
return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));")

                ;


            var lorum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.

Curabitur pretium tincidunt lacus. Nulla gravida orci a odio.Nullam varius, turpis et commodo pharetra, est eros bibendum elit, nec luctus magna felis sollicitudin mauris.Integer in mauris eu nibh euismod gravida.Duis ac tellus et risus vulputate vehicula.Donec lobortis risus a elit.Etiam tempor. Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam.Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi.Duis sapien sem, aliquet nec, commodo eget, consequat quis, neque. Aliquam faucibus, elit ut dictum aliquet, felis nisl adipiscing sapien, sed malesuada diam lacus eget erat.Cras mollis scelerisque nunc. Nullam arcu. Aliquam consequat. Curabitur augue lorem, dapibus quis, laoreet et, pretium ac, nisi. Aenean magna nisl, mollis quis, molestie eu, feugiat in, orci.In hac habitasse platea dictumst.;
";

            // font detection
            new TextBlockSample(outputfolder, "lorum ipsum", markdown)

                .Paint("lorum ipsum", 200, (canvas) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, lorum);
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 400, 0));
                }, null)

                ;

            var lorumshort = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            new TextBlockSampleAnimated(outputfolder, "animated", 300, 200, 15)

                .Paint((canvas, pct, y) =>
                {
                    var text = new Text(new FLFont(14), SKColors.Black, lorumshort);
                    return canvas.DrawTextBlock(text, new SKRect(0, 0, 10 + 290 * (pct * pct), 0));
                }, "")

                .Save(30);

            var lorumshortrtl = "أبجد هوز دولور الجلوس امات، إيليت، سد قيام الإيقاع والحيوية، بحيث تعبا وحزنا، وبعض الأمور الهامة.";

            new TextBlockSampleAnimated(outputfolder, "animated rtl", 300, 200, 15)

                .Paint((canvas, pct, y) =>
                {
                    var w = 10 + 290 * (pct * pct);
                    var text = new Text(new FLFont(14), SKColors.Black, lorumshortrtl);
                    return canvas.DrawTextBlock(text, new SKRect(299 - w, y, 299, 0), null, FlowDirection.RightToLeft);
                }, "")

                .Save(30);


            var mdpath = Path.Combine(outputfolder, "README.mddraft");
            File.WriteAllText(mdpath, markdown.ToString());

            // open the output folder
            Process.Start(new ProcessStartInfo() { FileName = outputfolder, UseShellExecute = true });

        }
    }
}
