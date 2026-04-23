using System.Windows;
using System.Windows.Controls;

namespace ExplorerTabUtility.UI.Behaviors
{
    internal class BookmarkHelper
    {
        #region 选择其他文件夹文本
        public static string GetSelectOtherFolderText(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectOtherFolderTextProperty);
        }

        public static void SetSelectOtherFolderText(DependencyObject obj, string value)
        {
            obj.SetValue(SelectOtherFolderTextProperty, value);
        }

        public static readonly DependencyProperty SelectOtherFolderTextProperty =
            DependencyProperty.RegisterAttached("SelectOtherFolderText", typeof(string), typeof(BookmarkHelper), new PropertyMetadata(""));
        #endregion

        #region MVVM聚焦
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(BookmarkHelper), new FrameworkPropertyMetadata(IsFocusedChanged) { BindsTwoWayByDefault = true });

        public static bool? GetIsFocused(DependencyObject element)
        {
            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            element.SetValue(IsFocusedProperty, value);
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if (e.OldValue == null)
            {
                fe.GotFocus += FrameworkElement_GotFocus;
                fe.LostFocus += FrameworkElement_LostFocus;
            }

            if (!fe.IsVisible)
            {
                fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
            }

            if ((bool)e.NewValue)
            {
                if (fe is TextBox txt) txt.SelectAll();
                fe.Focus();
            }
        }

        private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            if (fe.IsVisible && (bool)((FrameworkElement)sender).GetValue(IsFocusedProperty))
            {
                fe.IsVisibleChanged -= fe_IsVisibleChanged;
                fe.Focus();
            }
        }

        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
        #endregion
    }
}
