using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views;

namespace ExplorerTabUtility.UI.Themes
{
    public partial class TreeViewStyles
    {
        private bool isCancel;

        private void BookmarkEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            var txt = (TextBox)sender;
            var info = (BookmarkTreeViewInfo)txt.DataContext;

            if (isCancel || string.IsNullOrEmpty(info.Name))
            {
                info.RecoverName();
            }
            else
            {
                if (info.IsNeedSave && GetParentObjectEx<BookmarkSavePopup>(txt, out var popup))
                {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    popup.AddFolder(info);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                }
            }

            isCancel = false;
        }

        private void BookmarkEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                isCancel = e.Key == Key.Escape;
                if (isCancel) e.Handled = true;

                var txt = (TextBox)sender;
                if (GetParentObjectEx<TreeViewItem>(txt, out var item))
                {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    item.Focus();
#pragma warning restore CS8602 // 解引用可能出现空引用。
                }
            }
        }

        private bool GetParentObjectEx<TreeViewItem>(DependencyObject obj, out TreeViewItem? result) where TreeViewItem : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is TreeViewItem item)
                {
                    result = item;
                    return true;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            result = null;
            return false;
        }
    }
}
