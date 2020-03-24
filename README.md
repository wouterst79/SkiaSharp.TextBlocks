# SkiaSharp.TextBlock
SkiaSharp.TextBlock adds text block and rich text layout to SkiaSharp 

## Sample uses:
NOTE: DrawTextBlock returns the SKRect that contains the text. The sample project draws a green box around this rect. See the [source](./samples) for details.

## Basic Samples
### Hello World:
![Basic_Samples_-_Hello_World](./samples/output/Basic_Samples_-_Hello_World.png)
```C#
canvas.DrawTextBlock("Hello world!", new SKRect(0, 0, 100, 0), new FLFont(14), SKColors.Black);
```

### FlowDirection.RightToLeft:
![Basic_Samples_-_FlowDirection.RightToLeft](./samples/output/Basic_Samples_-_FlowDirection.RightToLeft.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
```

### Word Wrap:
![Basic_Samples_-_Word_Wrap](./samples/output/Basic_Samples_-_Word_Wrap.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

### LineBreakMode.Center:
![Basic_Samples_-_LineBreakMode.Center](./samples/output/Basic_Samples_-_LineBreakMode.Center.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.Center);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

### LineBreakMode.MiddleTruncation:
![Basic_Samples_-_LineBreakMode.MiddleTruncation](./samples/output/Basic_Samples_-_LineBreakMode.MiddleTruncation.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "Hello world!", LineBreakMode.MiddleTruncation);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

### Word Wrap
![animated](./samples/output/animated.gif)
![animated](./samples/output/animated_rtl.gif)

## Basic Samples 2
### Word Wrap - Tight:
![Basic_Samples_2_-_Word_Wrap_-_Tight](./samples/output/Basic_Samples_2_-_Word_Wrap_-_Tight.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 20, 0));
```

### Courier New:
![Basic_Samples_2_-_Courier_New](./samples/output/Basic_Samples_2_-_Courier_New.png)
```C#
var text = new Text(new FLFont("Courier New", 14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

### Color and Size:
![Basic_Samples_2_-_Color_and_Size](./samples/output/Basic_Samples_2_-_Color_and_Size.png)
```C#
var text = new Text(new FLFont(20), SKColors.Red, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0)); 
```

### New line:
![Basic_Samples_2_-_New_line](./samples/output/Basic_Samples_2_-_New_line.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, @"
(leading) new- line support...

Hello World!
SkiaSharp Rocks!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
```

### New Line - Trailing:
![Basic_Samples_2_-_New_Line_-_Trailing](./samples/output/Basic_Samples_2_-_New_Line_-_Trailing.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, @"Trailing new- line support:

");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
```

## Typeface Detection
### Non-latin:
![Typeface_Detection_-_Non-latin](./samples/output/Typeface_Detection_-_Non-latin.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "年");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

### Symbols:
![Typeface_Detection_-_Symbols](./samples/output/Typeface_Detection_-_Symbols.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "↺");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

### Unicode:
![Typeface_Detection_-_Unicode](./samples/output/Typeface_Detection_-_Unicode.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "🌐🍪🍕🚀");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

### Rtl Support:
![Typeface_Detection_-_Rtl_Support](./samples/output/Typeface_Detection_-_Rtl_Support.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
```

### Rtl Word Wrap:
![Typeface_Detection_-_Rtl_Word_Wrap](./samples/output/Typeface_Detection_-_Rtl_Word_Wrap.png)
```C#
var text = new Text(new FLFont(14), SKColors.Black, "مرحبا بالعالم");
return canvas.DrawTextBlock(text, new SKRect(50, 0, 100, 0), null, FlowDirection.RightToLeft);
```

## Rich Text
### Shorter:
![Rich_Text_-_Shorter](./samples/output/Rich_Text_-_Shorter.png)
```C#
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

```

### Longer:
![Rich_Text_-_Longer](./samples/output/Rich_Text_-_Longer.png)
```C#
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
```

## Lorum ipsum
![Lorum_ipsum](./samples/output/Lorum_ipsum.png)
