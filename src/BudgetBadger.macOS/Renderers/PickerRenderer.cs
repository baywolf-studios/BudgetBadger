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
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var popUpButton = new FormsNSPopUpButton();
                    popUpButton.FocusChanged += ControlFocusChanged;
                    SetNativeControl(popUpButton);
                }

                _defaultBackgroundColor = Control.Cell.BackgroundColor;

                Control.Activated += ComboBoxSelectionChanged;
                ResetItems();

                ((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged += CollectionChanged; 
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Picker.ItemsSourceProperty.PropertyName)
            {
                ResetItems();
            }
            if(e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
            {
                UpdateSelectedItem();
            }
            if (e.PropertyName == Picker.TextColorProperty.PropertyName ||
                e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
                e.PropertyName == Picker.FontSizeProperty.PropertyName ||
                e.PropertyName == Picker.FontFamilyProperty.PropertyName ||
                e.PropertyName == Picker.FontAttributesProperty.PropertyName)
            {
                UpdateFontAndColor();
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

                    if (Control != null)
                    {
                        Control.Activated -= ComboBoxSelectionChanged;
                        (Control as FormsNSPopUpButton).FocusChanged -= ControlFocusChanged;
                    }

                    if (Element != null)
                    {
                        ((INotifyCollectionChanged)Element.Items).CollectionChanged -= CollectionChanged;
                    }
                }
            }
            base.Dispose(disposing);
        }

        void ControlFocusChanged(object sender, BoolEventArgs e)
        {
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, e.Value);
        }

        void ComboBoxSelectionChanged(object sender, EventArgs e)
        {
            ElementController?.SetValueFromRenderer(Picker.SelectedIndexProperty, (int)Control.IndexOfSelectedItem);
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e);
                    break;
                default: //Move, Replace, Reset
                    ResetItems();
                    break;
            }
        }
        void AddItems(NotifyCollectionChangedEventArgs e)
        {
            if (Control == null || Element == null)
            {
                return;
            }
            int index = e.NewStartingIndex < 0 ? Control.Items().Count() : e.NewStartingIndex;
            foreach (object newItem in e.NewItems)
            {
                var menuItem = new NSMenuItem(newItem.ToString())
                {
                    AttributedTitle = new NSAttributedString(newItem.ToString(), GetNSStringAttributes())
                };
                Control.Menu.AddItem(menuItem);
            }
        }

        void RemoveItems(NotifyCollectionChangedEventArgs e)
        {
            if (Control == null || Element == null)
            {
                return;
            }

            int index = e.OldStartingIndex < Control.Items().Count() ? e.OldStartingIndex : Control.Items().Count();
            foreach (object _ in e.OldItems)
            {
                Control.RemoveItem(index--);
            }
        }

        void ResetItems()
        {
            if (Control == null || Element == null)
            {
                return;
            }

            Control.RemoveAllItems();
            foreach (var item in Element.Items)
            {
                var menuItem = new NSMenuItem(item ?? String.Empty)
                {
                    AttributedTitle = new NSAttributedString(item ?? String.Empty, GetNSStringAttributes())
                };
                Control.Menu.AddItem(menuItem);
            }

            UpdateSelectedItem();
        }

        void UpdateSelectedItem()
        {
            if (Control == null || Element == null)
            {
                return;
            }

            var selectedIndex = Element.SelectedIndex;
            var items = Element.Items;

            if (items != null && items.Count != 0 && selectedIndex >= 0)
            {
                Control.SelectItem(selectedIndex);
            }

            if (Control.SelectedItem != null)
            {
                Control.SelectedItem.AttributedTitle = new NSAttributedString(Control.SelectedItem.Title, GetNSStringAttributes());
            }
        }

        NSStringAttributes GetNSStringAttributes()
        {
            var attributes = new NSStringAttributes
            {
                Font = Element.ToNSFont(),
                ParagraphStyle = new NSMutableParagraphStyle { Alignment = NSTextAlignment.Left }
            };

            if (Element.TextColor != Color.Default)
            {
                attributes.ForegroundColor = Element.TextColor.ToNSColor();
            }

            return attributes;
        }

        void UpdateFontAndColor()
        {
            var attributes = GetNSStringAttributes();

            foreach (var it in Control.Items())
            {
                it.AttributedTitle = new NSAttributedString(it.Title, attributes);
            }

            if (Control.SelectedItem != null)
            {
                Control.SelectedItem.AttributedTitle = new NSAttributedString(Control.SelectedItem.Title, attributes);
            }
        }
    }
}
