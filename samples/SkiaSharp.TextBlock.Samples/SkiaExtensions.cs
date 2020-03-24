using System;
using System.Collections.Generic;
using System.Text;

// https://github.com/mono/SkiaSharp/blob/5e8dc3e2c9e72f2ad0d9feecefbef503ca9fcc15/source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs

namespace SkiaSharp.TextBlock.Samples
{
    public static class SkiaExtensions
    {

		public static System.Drawing.Bitmap ToBitmap(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var bitmap = new System.Drawing.Bitmap(skiaImage.Width, skiaImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

			// copy
			using (var pixmap = new SKPixmap(new SKImageInfo(data.Width, data.Height), data.Scan0, data.Stride))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}

			bitmap.UnlockBits(data);
			return bitmap;
		}


	}
}
