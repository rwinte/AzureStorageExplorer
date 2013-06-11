
// WPF Spinner control from SharpFellows blog post / shared code.
// http://sharpfellows.com/post/WPF-Spinner-e28093-take-two.aspx

using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SharpFellows.Toolkit.Controls
{
    /// <summary>
    /// WPF DomainUpDown control
    /// </summary>
    [TemplatePart(Name = "PART_UpButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DownButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class DomainUpDown : Control
    {
        #region Fields

        private int _selectedIndex;
        private RepeatButton _upBuppton;
        private RepeatButton _downButton;

        #endregion

        #region Dependency properties
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DomainUpDown), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable), typeof(DomainUpDown), new PropertyMetadata(OnItemsChanged));
        #endregion

        /// <summary>
        /// Initializes the <see cref="DomainUpDown"/> class.
        /// </summary>
        static DomainUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DomainUpDown), new FrameworkPropertyMetadata(typeof(DomainUpDown)));
            BorderBrushProperty.OverrideMetadata(typeof(DomainUpDown), new FrameworkPropertyMetadata(SystemColors.ControlLightBrush));
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public IEnumerable Items
        {
            get { return GetValue(ItemsProperty) as IEnumerable; }
            set { SetValue(ItemsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the index of the selected value.
        /// </summary>
        /// <value>The index of the selected.</value>
        protected int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value)
                    return;

                _selectedIndex = value;

                Value = Items.Cast<object>().Skip(SelectedIndex).First();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Down)
            {
                if (_downButton != null)
                    _downButton.Focus();

                OnDown(this, null);
                e.Handled = true;
            }

            if (e.Key == Key.Up)
            {
                if (_upBuppton != null)
                    _upBuppton.Focus();

                OnUp(this, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call 
        /// <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_upBuppton != null)
                _upBuppton.Click -= OnUp;

            if (_downButton != null)
                _downButton.Click -= OnDown;

            // Get the parts and attach event handlers to them
            _upBuppton = GetTemplateChild("PART_UpButton") as RepeatButton;
            _downButton = GetTemplateChild("PART_DownButton") as RepeatButton;

            if (_upBuppton != null)
                _upBuppton.Click += OnUp;

            if (_downButton != null)
                _downButton.Click += OnDown;
        }

        /// <summary>
        /// Invoked whenever an unhandled <see cref="E:System.Windows.UIElement.GotFocus"/> event reaches this element in its route.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            // Move focus immediately to the buttons
            if (_upBuppton != null)
                _upBuppton.Focus();
        }

        private void OnUp(object sender, RoutedEventArgs routedEventArgs)
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
        }

        private void OnDown(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Items != null && SelectedIndex < Items.Cast<object>().Count() - 1)
                SelectedIndex++;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var updown = d as DomainUpDown;
            SynchroniseValueWithIndex(updown, e.NewValue);
        }


        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var updown = d as DomainUpDown;
            SynchroniseValueWithIndex(updown, updown == null ? null : updown.Value);
        }

        private static void SynchroniseValueWithIndex(DomainUpDown updown, object newValue)
        {
            if (updown == null || updown.Items == null)
                return;

            int i = 0;

            foreach (var element in updown.Items)
            {
                if (element.Equals(newValue))
                {
                    updown.SelectedIndex = i;
                    break;
                }

                i++;
            }
        }
    }
}
