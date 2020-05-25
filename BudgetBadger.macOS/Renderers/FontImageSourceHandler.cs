using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using BudgetBadger.macOS.Renderers;
using CoreGraphics;
using CoreText;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportImageSourceHandler(typeof(FontImageSource), typeof(FontImageSourceHandler))]
namespace BudgetBadger.macOS.Renderers
{
    public class FontImageSourceHandler : IImageSourceHandler
    {
        readonly Color _defaultColor = Color.White;

        public Task<NSImage> LoadImageAsync(
           ImageSource imagesource,
           CancellationToken cancelationToken = default(CancellationToken),
           float scale = 1f)
        {
            NSImage image = null;
            var fontsource = imagesource as FontImageSource;
            if (fontsource != null)
            {
                var font = NSFont.FromFontName(fontsource.FontFamily ?? string.Empty, (float)fontsource.Size) ??
                    NSFont.SystemFontOfSize((float)fontsource.Size);
                var iconcolor = fontsource.Color.IsDefault ? _defaultColor : fontsource.Color;
                var centerAlign = new NSMutableParagraphStyle() { Alignment = NSTextAlignment.Center };
                var attString = new NSAttributedString(fontsource.Glyph, font: font, foregroundColor: iconcolor.ToNSColor(), paragraphStyle: centerAlign);
                var stringSize = attString.GetSize();
                image = new NSImage(stringSize);
                image.LockFocus();
                var actualDrawRect = new CGRect(0, 0, stringSize.Width, stringSize.Height);
                attString.DrawInRect(actualDrawRect);
                image.UnlockFocus();
            }
            return Task.FromResult(image);
        }
    }
}
