using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Picker), typeof(BudgetBadger.macOS.Renderers.PickerRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class PickerRenderer : ViewRenderer<Picker, NSPopUpButton>
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        bool _disposed;
        NSColor _defaultBackgroundColor;

        IElementController ElementController => Element;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            if (e.OldElement != null)
                ((INotifyCollectionChanged)e.OldElement.Items).CollectionChanged -= RowsCollectionChanged;

            if (e.NewElement != null)
            {
                if (Control == null)
                    SetNativeControl(new NSPopUpButton());

                _defaultBackgroundColor = Control.Cell.BackgroundColor;

                Control.Activated -= ComboBoxSelectionChanged;
                Control.Activated += ComboBoxSelectionChanged;
                UpdatePicker();

                ((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged -= RowsCollectionChanged;
                ((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged += RowsCollectionChanged;
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Picker.TitleProperty.PropertyName ||
                e.PropertyName == Picker.ItemsSourceProperty.PropertyName ||
                e.PropertyName == Picker.SelectedIndexProperty.PropertyName ||
                e.PropertyName == Picker.TextColorProperty.PropertyName ||
                e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
                e.PropertyName == Picker.SelectedItemProperty.PropertyName ||
                e.PropertyName == Picker.FontSizeProperty.PropertyName ||
                e.PropertyName == Picker.FontFamilyProperty.PropertyName ||
                e.PropertyName == Picker.FontAttributesProperty.PropertyName)
            {
                UpdatePicker();
            }
        }

        protected override void SetBackgroundColor(Color color)
        {
            base.SetBackgroundColor(color);

            if (Control == null)
                return;

            Control.Cell.BackgroundColor = color == Color.Default ? _defaultBackgroundColor : color.ToNSColor();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    if (Element != null)
                        ((INotifyCollectionChanged)Element.Items).CollectionChanged -= RowsCollectionChanged;

                    if (Control != null)
                        Control.Activated -= ComboBoxSelectionChanged;
                }
            }
            base.Dispose(disposing);
        }

        void ComboBoxSelectionChanged(object sender, EventArgs e)
        {
            ElementController?.SetValueFromRenderer(Picker.SelectedIndexProperty, (int)Control.IndexOfSelectedItem);
        }

        void OnEnded(object sender, EventArgs eventArgs)
        {
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
        }

        void OnStarted(object sender, EventArgs eventArgs)
        {
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
        }

        void RowsCollectionChanged(object sender, EventArgs e)
        {
            UpdatePicker();
        }

        void UpdatePicker()
        {
            if (Control == null || Element == null)
                return;

            var attributes = new NSStringAttributes
            {
                Font = Element.ToNSFont(),
                ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Left }
            };

            if (Element.TextColor != Color.Default)
            {
                attributes.ForegroundColor = Element.TextColor.ToNSColor();
            }

            Control.RemoveAllItems();
            foreach (var item in Element.Items)
            {
                var menuItem = new NSMenuItem(item);
                menuItem.AttributedTitle = new NSAttributedString(menuItem.Title, attributes);
                Control.Menu.AddItem(menuItem);
            }

            var selectedIndex = Element.SelectedIndex;
            var items = Element.Items;

            if (items != null && items.Count != 0 && selectedIndex >= 0)
            {
                Control.SelectItem(selectedIndex);
            }

            if (Control.SelectedItem != null)
            {
                Control.SelectedItem.AttributedTitle = new NSAttributedString(Control.SelectedItem.Title, attributes);
            }
        }
    }
}
