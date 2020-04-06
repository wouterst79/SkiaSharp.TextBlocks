# SkiaSharp.TextBlock
SkiaSharp.TextBlock adds text block and rich text layout to SkiaSharp 

## Sample uses:
NOTE: DrawTextBlock returns the SKRect that contains the text. The sample project draws a green box around this rect. See the [source](./samples) for details.

## Basic Samples
Hello World:

![Basic_Samples-Hello_World](./samples/output/Basic_Samples-Hello_World.png)

```C#
canvas.DrawTextBlock("Hello world!", new SKRect(0, 0, 100, 0), new Font(14), SKColors.Black);
```

FlowDirection.RightToLeft:

![Basic_Samples-FlowDirection.RightToLeft](./samples/output/Basic_Samples-FlowDirection.RightToLeft.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "Hello world!");
canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
```

Word Wrap:

![Basic_Samples-Word_Wrap](./samples/output/Basic_Samples-Word_Wrap.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

LineBreakMode.Center:

![Basic_Samples-LineBreakMode.Center](./samples/output/Basic_Samples-LineBreakMode.Center.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "Hello world!", LineBreakMode.Center);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

LineBreakMode.MiddleTruncation:

![Basic_Samples-LineBreakMode.MiddleTruncation](./samples/output/Basic_Samples-LineBreakMode.MiddleTruncation.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "Hello world!", LineBreakMode.MiddleTruncation);
return canvas.DrawTextBlock(text, new SKRect(0, 0, 50, 0));
```

### Word Wrap
![animated](./samples/output/animated.gif)
![animated](./samples/output/animated_rtl.gif)

## Basic Samples 2
Word Wrap - Tight:

![Basic_Samples_2-Word_Wrap_-_Tight](./samples/output/Basic_Samples_2-Word_Wrap_-_Tight.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 20, 0));
```

Courier New:

![Basic_Samples_2-Courier_New](./samples/output/Basic_Samples_2-Courier_New.png)

```C#
var text = new TextBlock(new Font("Courier New", 14), SKColors.Black, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Color and Size:

![Basic_Samples_2-Color_and_Size](./samples/output/Basic_Samples_2-Color_and_Size.png)

```C#
var text = new TextBlock(new Font(20), SKColors.Red, "Hello world!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0)); 
```

New line:

![Basic_Samples_2-New_line](./samples/output/Basic_Samples_2-New_line.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, @"
(leading) new- line support...

Hello World!
SkiaSharp Rocks!");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
```

New Line - Trailing:

![Basic_Samples_2-New_Line_-_Trailing](./samples/output/Basic_Samples_2-New_Line_-_Trailing.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, @"Trailing new- line support:

");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 150, 0));
```

## Typeface Detection
Non-latin:

![Typeface_Detection-Non-latin](./samples/output/Typeface_Detection-Non-latin.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "年");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Cyrillic:

![Typeface_Detection-Cyrillic](./samples/output/Typeface_Detection-Cyrillic.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "yči");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Symbols:

![Typeface_Detection-Symbols](./samples/output/Typeface_Detection-Symbols.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "↺");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Unicode:

![Typeface_Detection-Unicode](./samples/output/Typeface_Detection-Unicode.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "🌐🍪🍕🚀");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Rtl Support:

![Typeface_Detection-Rtl_Support](./samples/output/Typeface_Detection-Rtl_Support.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "مرحبا بالعالم");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0), null, FlowDirection.RightToLeft);
```

Multi glyph:

![Typeface_Detection-Multi_glyph](./samples/output/Typeface_Detection-Multi_glyph.png)

```C#
var text = new TextBlock(new Font(20), SKColors.Black, "น้ำ");
return canvas.DrawTextBlock(text, new SKRect(0, 0, 100, 0));
```

Rtl Word Wrap:

![Typeface_Detection-Rtl_Word_Wrap](./samples/output/Typeface_Detection-Rtl_Word_Wrap.png)

```C#
var text = new TextBlock(new Font(14), SKColors.Black, "مرحبا بالعالم");
return canvas.DrawTextBlock(text, new SKRect(50, 0, 100, 0), null, FlowDirection.RightToLeft);
```

## Rich Text
Shorter:

![Rich_Text-Shorter](./samples/output/Rich_Text-Shorter.png)

```C#
var text = new RichTextBlock()
{
    Spans =
    {
        new TextBlock(new Font(10), SKColors.Black, "Hello "),
        new TextBlock(new Font(20, true), SKColors.Black, "world! (bold)"),
        new TextBlock(new Font(16), SKColors.Green, "SkiaSharp Rocks!"),
    }
};
return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));

```

Longer:

![Rich_Text-Longer](./samples/output/Rich_Text-Longer.png)

```C#
var text = new RichTextBlock()
{
    Spans =
    {
        new TextBlock(new Font(16), SKColors.Green, "SkiaSharp Rocks!"),
        new TextBlock(new Font(10), SKColors.Black, "Hello "),
        new TextBlock(new Font(20, true), SKColors.Black, "world! "),
        new TextBlock(new Font(14), SKColors.Black, @"Trailing new-line support:

"),
        new TextBlock(new Font(16), SKColors.Green, "SkiaSharp Rocks!"),
        new TextBlock(new Font(14), SKColors.Black, "مرحبا بالعالم"),
        new TextBlock(new Font(16), SKColors.Green, "SkiaSharp Rocks!"),
        new TextBlock(new Font(14), SKColors.Black, "年"),
        new TextBlock(new Font(16), SKColors.Green, "SkiaSharp Rocks!"),
        new TextBlock(new Font(14), SKColors.Black, "↺"),
    }
};
return canvas.DrawRichTextBlock(text, new SKRect(0, 0, 200, 0));
```

## Lorum ipsum
default line spacing:

![Lorum_ipsum-default_line_spacing](./samples/output/Lorum_ipsum-default_line_spacing.png)

1.5x line spacing:

![Lorum_ipsum-1.5x_line_spacing](./samples/output/Lorum_ipsum-1.5x_line_spacing.png)

