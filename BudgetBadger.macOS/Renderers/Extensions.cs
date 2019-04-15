using System;
using CoreGraphics;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using AppKit;
using UIColor = AppKit.NSColor;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using System.Diagnostics;
using System.Collections.Generic;
using NativeFont = AppKit.NSFont;
using UITextAlignment = AppKit.NSTextAlignment;
using Foundation;

namespace BudgetBadger.macOS.Renderers
{
        public static class ColorExtensions
        {

            internal static readonly NSColor Black = NSColor.Black;
            internal static readonly NSColor SeventyPercentGrey = NSColor.FromRgba(0.7f, 0.7f, 0.7f, 1);


            public static CGColor ToCGColor(this Color color)
            {

                return color.ToNSColor().CGColor;

            }
            
            public static NSColor ToNSColor(this Color color)
            {
                return NSColor.FromRgba((float)color.R, (float)color.G, (float)color.B, (float)color.A);
            }

            public static NSColor ToNSColor(this Color color, Color defaultColor)
            {
                if (color.IsDefault)
                    return defaultColor.ToNSColor();

                return color.ToNSColor();
            }

            public static NSColor ToNSColor(this Color color, NSColor defaultColor)
            {
                if (color.IsDefault)
                    return defaultColor;

                return color.ToNSColor();
            }
        }

        public static class PointExtensions
        {
            public static Point ToPoint(this PointF point)
            {
                return new Point(point.X, point.Y);
            }

            public static PointF ToPointF(this Point point)
            {
                return new PointF(point.X, point.Y);
            }
        }

        public static class SizeExtensions
        {
            public static SizeF ToSizeF(this Size size)
            {
                return new SizeF((float)size.Width, (float)size.Height);
            }
        }

        public static class RectangleExtensions
        {
            public static Rectangle ToRectangle(this RectangleF rect)
            {
                return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }

            public static RectangleF ToRectangleF(this Rectangle rect)
            {
                return new RectangleF((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);
            }
        }

        public static partial class FontExtensions
        {
            static readonly string DefaultFontName = NSFont.SystemFontOfSize(12).FontName;

            public static NSFont ToNSFont(this Font self) => ToNativeFont(self);

            internal static NSFont ToNSFont(this IFontElement element) => ToNativeFont(element);

            static NSFont _ToNativeFont(string family, float size, FontAttributes attributes)
            {
                NSFont defaultFont = NSFont.SystemFontOfSize(size);
                NSFont font = null;
                NSFontDescriptor descriptor = null;
                var bold = (attributes & FontAttributes.Bold) != 0;
                var italic = (attributes & FontAttributes.Italic) != 0;

                if (family != null && family != DefaultFontName)
                {
                    try
                    {
                        descriptor = new NSFontDescriptor().FontDescriptorWithFamily(family);
                        font = NSFont.FromDescription(descriptor, size);

                        if (font == null)
                            font = NSFont.FromFontName(family, size);
                    }
                    catch
                    {
                        Debug.WriteLine("Could not load font named: {0}", family);
                    }
                }

                //if we didn't found a Font or Descriptor for the FontFamily use the default one 
                if (font == null)
                    font = defaultFont;

                if (descriptor == null)
                    descriptor = defaultFont.FontDescriptor;

                if (bold || italic)
                {
                    var traits = (NSFontSymbolicTraits)0;
                    if (bold)
                        traits = traits | NSFontSymbolicTraits.BoldTrait;
                    if (italic)
                        traits = traits | NSFontSymbolicTraits.ItalicTrait;

                    var fontDescriptoWithTraits = descriptor.FontDescriptorWithSymbolicTraits(traits);

                    font = NSFont.FromDescription(fontDescriptoWithTraits, size);
                }

                return font.ScreenFontWithRenderingMode(NSFontRenderingMode.AntialiasedIntegerAdvancements);
            }

            static readonly Dictionary<ToNativeFontFontKey, NativeFont> ToUiFont = new Dictionary<ToNativeFontFontKey, NativeFont>();

            internal static bool IsDefault(this Span self)
            {
                return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) &&
                        self.FontAttributes == FontAttributes.None;
            }

            static NativeFont ToNativeFont(this IFontElement element)
            {
                var fontFamily = element.FontFamily;
                var fontSize = (float)element.FontSize;
                var fontAttributes = element.FontAttributes;
                return ToNativeFont(fontFamily, fontSize, fontAttributes, _ToNativeFont);
            }

            static NativeFont ToNativeFont(this Font self)
            {
                var size = (float)self.FontSize;
                if (self.UseNamedSize)
                {
                    switch (self.NamedSize)
                    {
                        case NamedSize.Micro:
                            size = 12;
                            break;
                        case NamedSize.Small:
                            size = 14;
                            break;
                        case NamedSize.Medium:
                            size = 17; // as defined by iOS documentation
                            break;
                        case NamedSize.Large:
                            size = 22;
                            break;
                        default:
                            size = 17;
                            break;
                    }
                }

                var fontAttributes = self.FontAttributes;

                return ToNativeFont(self.FontFamily, size, fontAttributes, _ToNativeFont);
            }

            static NativeFont ToNativeFont(string family, float size, FontAttributes attributes, Func<string, float, FontAttributes, NativeFont> factory)
            {
                var key = new ToNativeFontFontKey(family, size, attributes);

                lock (ToUiFont)
                {
                    NativeFont value;
                    if (ToUiFont.TryGetValue(key, out value))
                        return value;
                }

                var generatedValue = factory(family, size, attributes);

                lock (ToUiFont)
                {
                    NativeFont value;
                    if (!ToUiFont.TryGetValue(key, out value))
                        ToUiFont.Add(key, value = generatedValue);
                    return value;
                }
            }

            struct ToNativeFontFontKey
            {
                internal ToNativeFontFontKey(string family, float size, FontAttributes attributes)
                {
                    _family = family;
                    _size = size;
                    _attributes = attributes;
                }
    #pragma warning disable 0414 // these are not called explicitly, but they are used to establish uniqueness. allow it!
                string _family;
                float _size;
                FontAttributes _attributes;
    #pragma warning restore 0414
            }
        }

    public static class FormattedStringExtensions
    {
        public static NSAttributedString ToAttributed(this Span span, Font defaultFont, Color defaultForegroundColor)
        {
            if (span == null)
                return null;

#pragma warning disable 0618 //retaining legacy call to obsolete code
            var font = span.Font != Font.Default ? span.Font : defaultFont;
#pragma warning restore 0618
            var fgcolor = span.TextColor;
            if (fgcolor.IsDefault)
                fgcolor = defaultForegroundColor;
            if (fgcolor.IsDefault)
                fgcolor = Color.Black; // as defined by apple docs      
                
            return new NSAttributedString(span.Text, font == Font.Default ? null : font.ToNSFont(), fgcolor.ToNSColor(),
                span.BackgroundColor.ToNSColor());
        }

        public static NSAttributedString ToAttributed(this FormattedString formattedString, Font defaultFont,
            Color defaultForegroundColor)
        {
            if (formattedString == null)
                return null;
            var attributed = new NSMutableAttributedString();
            for (int i = 0; i < formattedString.Spans.Count; i++)
            {
                Span span = formattedString.Spans[i];
                if (span.Text == null)
                    continue;

                attributed.Append(span.ToAttributed(defaultFont, defaultForegroundColor));
            }

            return attributed;
        }

        internal static NSAttributedString ToAttributed(this Span span, Element owner, Color defaultForegroundColor, TextAlignment textAlignment, double lineHeight = -1.0)
        {
            if (span == null)
                return null;

            var text = span.Text;
            if (text == null)
                return null;

            NSMutableParagraphStyle style = new NSMutableParagraphStyle();
            lineHeight = span.LineHeight >= 0 ? span.LineHeight : lineHeight;
            if (lineHeight >= 0)
            {
                style.LineHeightMultiple = new nfloat(lineHeight);
            }

            switch (textAlignment)
            {
                case TextAlignment.Start:
                    style.Alignment = UITextAlignment.Left;
                    break;
                case TextAlignment.Center:
                    style.Alignment = UITextAlignment.Center;
                    break;
                case TextAlignment.End:
                    style.Alignment = UITextAlignment.Right;
                    break;
                default:
                    style.Alignment = UITextAlignment.Left;
                    break;
            }

            NSFont targetFont;
            if (span.IsDefault())
                targetFont = ((IFontElement)owner).ToNSFont();
            else
                targetFont = span.ToNSFont();

            var fgcolor = span.TextColor;
            if (fgcolor.IsDefault)
                fgcolor = defaultForegroundColor;
            if (fgcolor.IsDefault)
                fgcolor = Color.Black; // as defined by apple docs

            NSColor spanFgColor;
            NSColor spanBgColor;
            spanFgColor = fgcolor.ToNSColor();
            spanBgColor = span.BackgroundColor.ToNSColor();

            bool hasUnderline = false;
            bool hasStrikethrough = false;
            if (span.IsSet(Span.TextDecorationsProperty))
            {
                var textDecorations = span.TextDecorations;
                hasUnderline = (textDecorations & TextDecorations.Underline) != 0;
                hasStrikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
            }

            var attrString = new NSAttributedString(text, targetFont, spanFgColor, spanBgColor,
                underlineStyle: hasUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None,
                strikethroughStyle: hasStrikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None, paragraphStyle: style);

            return attrString;
        }

        internal static NSAttributedString ToAttributed(this FormattedString formattedString, Element owner,
            Color defaultForegroundColor, TextAlignment textAlignment = TextAlignment.Start, double lineHeight = -1.0)
        {
            if (formattedString == null)
                return null;
            var attributed = new NSMutableAttributedString();

            for (int i = 0; i < formattedString.Spans.Count; i++)
            {
                Span span = formattedString.Spans[i];

                var attributedString = span.ToAttributed(owner, defaultForegroundColor, textAlignment, lineHeight);

                if (attributedString == null)
                    continue;

                attributed.Append(attributedString);
            }

            return attributed;
        }
    }
}

