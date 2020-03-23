# SkiaSharp.TextBlock
SkiaSharp.TextBlock adds text block and rich text layout to SkiaSharp 

## Sample uses:
[Source](./samples)

### basic samples
![basic samples](./samples/output/basic%20samples.png)
```
canvas.DrawTextBlock("Hello world!", new SKRect(0, y, 100, 0), new FLFont(14), SKColors.Black)

new TextBlock(new FLFont(14), SKColors.Black, "Hello world!");
canvas.DrawTextBlock(text, new SKRect(0, y, 100, 0), null, FlowDirection.RightToLeft);

canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));

new TextBlock(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.Center);
canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));

new TextBlock(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.MiddleTruncation);
canvas.DrawTextBlock(text, new SKRect(0, y, 50, 0));
```

### basic samples 2
![basic samples 2](./samples/output/basic%20samples%202.png)
```
var text = new TextBlock(new FLFont(14), SKColors.Black, "Hello world!");
canvas.DrawTextBlock(text, new SKRect(0, y, 20, 0));

new TextBlock(new FLFont("Courier New", 14), SKColors.Black, "Hello world!");

new TextBlock(new FLFont(20), SKColors.Red, "Hello world!");

new TextBlock(new FLFont(14), SKColors.Black, @"
(leading) new-line support...

Hello World!
SkiaSharp Rocks!");

new TextBlock(new FLFont(14), SKColors.Black, @"Trailing new-line support:

");
```

### font detection
![font detection](./samples/output/font%20detection.png)
```

new TextBlock(new FLFont(14), SKColors.Black, "年");
new TextBlock(new FLFont(14), SKColors.Black, "↺");
new TextBlock(new FLFont(14), SKColors.Black, "🌐🍪🍕🚀");
new TextBlock(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
```

### rich text
![rich text](./samples/output/rich%20text.png)
```
var text = new RichTextBlock()
{
    Spans =
    {
        new TextBlock(new FLFont(10), SKColors.Black, "Hello "),
        new TextBlock(new FLFont(20, true), SKColors.Black, "world! "),
        new TextBlock(new FLFont(16), SKColors.Green, "SkiaSharp Rocks!"),
    }
};
 canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
```

### other
![rich text 2](./samples/output/rich%20text%202.png)

```
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
 canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
```

![lorum ipsum](./samples/output/lorum%20ipsum.png)
```
new TextBlock(new FLFont(14), SKColors.Black, lorum);
```

