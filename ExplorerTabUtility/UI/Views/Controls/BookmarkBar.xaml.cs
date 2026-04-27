using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.UI.Views.Controls
{
    /// <summary>
    /// BookmarkBar.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkBar : UserControl
    {
        #region 事件
        public delegate void BookmarkEventHandler(BookmarkBarInfo info, BookmarkInfo bookmark, BookmarkBarAction action);
        public event BookmarkEventHandler? BookmarkHandle;

        public delegate void FolderEventHandler(BookmarkBarInfo info, FolderInfo folder, BookmarkBarAction action);
        public event FolderEventHandler? FolderHandle;
        #endregion

        private ObservableCollection<BookmarkBarInfo> allBookmarks;
        private ObservableCollection<BookmarkBarInfo> mainBookmarks;
        private ObservableCollection<BookmarkBarInfo> overflowBookmarks;
        private ObservableCollection<BookmarkBarInfo> otherBookmarks;

        public BookmarkBar()
        {
            InitializeComponent();

            allBookmarks = new ObservableCollection<BookmarkBarInfo>();
            mainBookmarks = new ObservableCollection<BookmarkBarInfo>();
            otherBookmarks = new ObservableCollection<BookmarkBarInfo>();
            overflowBookmarks = new ObservableCollection<BookmarkBarInfo>
            {
                new BookmarkBarInfo(BookmarkManager.Instance.OverflowFolder, 0, null)
            };
        }

        public void InitBookmark(FolderInfo folder, FolderInfo otherFolder)
        {
            var otherBookmark = new BookmarkBarInfo(otherFolder, 0, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, false);
            otherBookmarks.Add(otherBookmark);

            foreach (var item in new BookmarkBarInfo(folder, -1, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true).Children)
            {
                allBookmarks.Add(item);
            }

            UpdateMenuLayout();

            MnuMainBookmarks.ItemsSource = mainBookmarks;
            MnuOverflowBookmarks.ItemsSource = overflowBookmarks;
            MnuOtherBookmarks.ItemsSource = otherBookmarks;
            TxtSeparator.Visibility = otherBookmark.IsVisibility ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateMenuLayout()
        {
            var overflowBookmark = overflowBookmarks[0];

            //清空主书签和溢出书签
            mainBookmarks.Clear();
            overflowBookmark.Children.Clear();

            //所有书签数量为0直接返回
            if (allBookmarks.Count == 0) return;

            //初始可用宽度是控件的宽度
            var availableWidth = Width;

            //其他书签有子集合，就会显示，减去其宽度
            var otherBookmark = otherBookmarks[0];
            if (otherBookmark.Children.Count > 0)
            {
                if (otherBookmark.Width == 0)
                {
                    otherBookmark.Width = GetItemWidth(otherBookmark.DisplayName);
                }

                availableWidth -= otherBookmark.Width;
            }

            //减去溢出书签的宽度
            if (overflowBookmark.Width == 0)
            {
                overflowBookmark.Width = GetItemWidth(overflowBookmark.DisplayName);
            }
            availableWidth -= overflowBookmark.Width;

            //主书签
            int index;
            BookmarkBarInfo item;
            for (index = 0; index < allBookmarks.Count; index++)
            {
                item = allBookmarks[index];
                if (item.Width == 0)
                {
                    item.Width = GetItemWidth(item.DisplayName);
                }

                if (availableWidth < item.Width)
                {
                    break;
                }

                item.PlacementMode = PlacementMode.Bottom;
                mainBookmarks.Add(item);
                availableWidth -= item.Width;
            }

            //溢出书签
            for (; index < allBookmarks.Count; index++)
            {
                item = allBookmarks[index];
                item.PlacementMode = PlacementMode.Right;
                overflowBookmark.Children.Add(allBookmarks[index]);
            }
        }

        private int GetItemWidth(string name)
        {
            var width = 0;

            //名字的宽度
            var byteWith = 10;
            var byteCount = GetByteCount(name);
            width += byteCount * byteWith;

            //图标的宽度
            var iconWidth = 30;
            width += iconWidth;

            return width;
        }

        /// <summary>
        /// 计算字符数，大于255当汉字两个字符
        /// </summary>
        /// <returns></returns>
        private int GetByteCount(string str)
        {
            int count = 0;
            foreach (char c in str)
            {
                count += c > 255 ? 2 : 1;
            }
            return count;
        }

        private void BookmarkClickAction(BookmarkBarInfo info, BookmarkInfo bookmark)
        {
            BookmarkBarAction action;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                action = BookmarkBarAction.OpenInNewTab;
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                action = BookmarkBarAction.OpenInNewWindow;
            }
            else
            {
                action = BookmarkBarAction.OpenInCurrentTab;
            }

            BookmarkHandle?.Invoke(info, bookmark, action);
        }

        private void BookmarkMenuClickAction(BookmarkBarInfo info, BookmarkInfo bookmark, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.OpenInCurrentTab:
                case BookmarkBarAction.OpenInNewTab:
                case BookmarkBarAction.OpenInNewWindow:
                case BookmarkBarAction.Edit:
                    BookmarkHandle?.Invoke(info, bookmark, action);
                    break;
                case BookmarkBarAction.Delete:
                    if (info == null || info.Parent == null) throw new ArgumentNullException(nameof(info.Parent));

                    BookmarkManager.Instance.Delete(info.Parent.CurrentFolder, bookmark);
                    BookmarkManager.Instance.SaveConfig();

                    info.Parent.Delete(info);
                    Delete(info);
                    break;
            }
        }

        private void FolderMenuClickAction(BookmarkBarInfo info, FolderInfo folder, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.Rename:
                    FolderHandle?.Invoke(info, folder, action);
                    break;
                case BookmarkBarAction.Delete:
                    if (info == null || info.Parent == null) throw new ArgumentNullException(nameof(info.Parent));

                    BookmarkManager.Instance.Delete(info.Parent.CurrentFolder, folder);
                    BookmarkManager.Instance.SaveConfig();

                    info.Parent.Delete(info);
                    Delete(info);
                    break;
            }
        }

        private void Delete(BookmarkBarInfo info)
        {
            allBookmarks.Remove(info);
            mainBookmarks.Remove(info);

            //todo:添加是否需要更新布局的判断
            UpdateMenuLayout();
        }

        public void RenameFolder(BookmarkBarInfo info, string newName)
        {
            info.Name = newName;

            if (info.FirstLevel == false) return;

            info.Width = 0;
            UpdateMenuLayout();
        }

        public void EditBookmark(BookmarkBarInfo info, BookmarkInfo bookmark)
        {
            info.Name = bookmark.Name;

            if (info.Parent == null) throw new ArgumentNullException(nameof(info.Parent));

            if (info.Parent.CurrentFolder.Id != bookmark.ParentId)
            {
                info.Parent.Delete(info);

                var searchInfo = new BookmarkBarInfo(allBookmarks.Union(otherBookmarks));
                var newParent = searchInfo.SearchFolder(bookmark.ParentId);
                if (newParent != null)
                {
                    newParent.Add(info);
                }
            }

            //todo:添加是否需要更新布局的判断
            info.Width = 0;
            UpdateMenuLayout();
        }
    }
}
