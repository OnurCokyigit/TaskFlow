using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaskFlow.Helpers
{
    public static class PlaceholderBehavior
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(PlaceholderBehavior),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(DependencyObject obj)
            => (string)obj.GetValue(PlaceholderTextProperty);

        public static void SetPlaceholderText(DependencyObject obj, string value)
            => obj.SetValue(PlaceholderTextProperty, value);

        private static void OnPlaceholderTextChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox) return;

            textBox.Loaded += (s, _) => UpdatePlaceholder(textBox);
            textBox.TextChanged += (s, _) => UpdatePlaceholder(textBox);
            textBox.GotFocus += (s, _) => UpdatePlaceholder(textBox);
            textBox.LostFocus += (s, _) => UpdatePlaceholder(textBox);
        }

        private static void UpdatePlaceholder(TextBox textBox)
        {
            var placeholder = GetPlaceholderText(textBox);
            if (string.IsNullOrEmpty(placeholder)) return;

            // Mevcut AdornerLayer yerine Tag kullanacağız
            textBox.Tag = string.IsNullOrEmpty(textBox.Text)
                ? placeholder : string.Empty;
        }
    }
}