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
                var size = new CGSize(fontsource.Size, fontsource.Size);
                if (fontsource.Size <= 0)
                {
                    size.Width = 1;
                    size.Height = 1;
                }

                var paragraph = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Center };
                var font = NSFont.FromFontName(fontsource.FontFamily ?? string.Empty, (float)fontsource.Size) ?? NSFont.SystemFontOfSize((float)fontsource.Size);
                var iconcolor = fontsource.Color.IsDefault ? _defaultColor : fontsource.Color;

                var attributedString = new NSAttributedString(fontsource.Glyph, font: font, foregroundColor: iconcolor.ToNSColor(), paragraphStyle: paragraph);

                image = new NSImage(size);
                image.LockFocus();
                attributedString.DrawInRect(new CGRect(0, 0, size.Width, size.Height));
                image.UnlockFocus();
            }

            return Task.FromResult(image);
        }
    }
}
