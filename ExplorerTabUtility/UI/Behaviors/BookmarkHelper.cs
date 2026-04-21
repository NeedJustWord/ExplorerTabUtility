using System.Windows;

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
    }
}
