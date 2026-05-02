using System;
using System.Collections.Generic;
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

        private BookmarkBarInfo allBookmark;
        private ObservableCollection<BookmarkBarInfo> mainBookmarks;
        private ObservableCollection<BookmarkBarInfo> overflowBookmarks;
        private ObservableCollection<BookmarkBarInfo> otherBookmarks;

        public BookmarkBar()
        {
            InitializeComponent();

            allBookmark = new BookmarkBarInfo(BookmarkManager.Instance.Folder, -1, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);
            mainBookmarks = new ObservableCollection<BookmarkBarInfo>();
            overflowBookmarks = new ObservableCollection<BookmarkBarInfo>
            {
                new BookmarkBarInfo(BookmarkManager.Instance.OverflowFolder, 0, null)
            };
            otherBookmarks = new ObservableCollection<BookmarkBarInfo>
            {
                new BookmarkBarInfo(BookmarkManager.Instance.OtherFolder, 0, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, false)
            };
        }

        /// <summary>
        /// 初始化布局
        /// </summary>
        public void InitLayout()
        {
            UpdateMenuLayout();

            MnuMainBookmarks.ItemsSource = mainBookmarks;
            MnuOverflowBookmarks.ItemsSource = overflowBookmarks;
            MnuOtherBookmarks.ItemsSource = otherBookmarks;
        }

        private void UpdateMenuLayout()
        {
            var allBookmarks = allBookmark.Children;
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

                item.FirstLevel = true;
                item.Parent = allBookmark;
                item.PlacementMode = PlacementMode.Bottom;
                mainBookmarks.Add(item);
                availableWidth -= item.Width;
            }

            //溢出书签
            for (; index < allBookmarks.Count; index++)
            {
                item = allBookmarks[index];
                item.FirstLevel = false;
                item.Parent = overflowBookmark;
                item.PlacementMode = PlacementMode.Right;
                overflowBookmark.Children.Add(item);
            }

            TxtSeparator.Visibility = otherBookmark.IsVisibility ? Visibility.Visible : Visibility.Collapsed;
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
                case BookmarkBarAction.BookmarkManager:
                case BookmarkBarAction.NewBookmark:
                case BookmarkBarAction.NewFolder:
                case BookmarkBarAction.OpenInCurrentTab:
                case BookmarkBarAction.OpenInNewTab:
                case BookmarkBarAction.OpenInNewWindow:
                case BookmarkBarAction.Edit:
                    BookmarkHandle?.Invoke(info, bookmark, action);
                    break;
                case BookmarkBarAction.Delete:
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    BookmarkManager.Instance.Delete(info.Parent.CurrentFolder, bookmark);
#pragma warning restore CS8602 // 解引用可能出现空引用。
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
                case BookmarkBarAction.BookmarkManager:
                case BookmarkBarAction.NewBookmark:
                case BookmarkBarAction.NewFolder:
                case BookmarkBarAction.Rename:
                    FolderHandle?.Invoke(info, folder, action);
                    break;
                case BookmarkBarAction.Delete:
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    BookmarkManager.Instance.Delete(info.Parent.CurrentFolder, folder);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                    BookmarkManager.Instance.SaveConfig();

                    info.Parent.Delete(info);
                    Delete(info);
                    break;
            }
        }

        private void Delete(BookmarkBarInfo info)
        {
            var needUpdateLayout = NeedUpdateMenuLayout(info, Guid.Empty);

            allBookmark.Children.Remove(info);
            mainBookmarks.Remove(info);

            if (needUpdateLayout)
            {
                UpdateMenuLayout();
            }
        }

        public void RenameFolder(BookmarkBarInfo info, string newName)
        {
            info.Name = newName;

            if (NeedUpdateMenuLayout(info, Guid.Empty))
            {
                info.Width = 0;
                UpdateMenuLayout();
            }
        }

        public void EditBookmark(BookmarkBarInfo info, BookmarkInfo bookmark)
        {
            info.Name = bookmark.Name;

            if (info.Parent == null) throw new ArgumentNullException(nameof(info.Parent));

            var needUpdateLayout = NeedUpdateMenuLayout(info, bookmark.ParentId);
            if (info.GetParentId() != bookmark.ParentId)
            {
                info.Parent.Delete(info);

                var newParent = SearchFolder(bookmark.ParentId);
                if (newParent != null)
                {
                    newParent.Add(info);
                }
            }

            if (needUpdateLayout)
            {
                info.Width = 0;
                UpdateMenuLayout();
            }
        }

        public void Refresh()
        {
            allBookmark = new BookmarkBarInfo(BookmarkManager.Instance.Folder, -1, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);
            otherBookmarks[0] = new BookmarkBarInfo(BookmarkManager.Instance.OtherFolder, 0, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, false);

            UpdateMenuLayout();
        }

        public void NewBookmark(BookmarkInfo bookmark)
        {
            var parent = SearchFolder(bookmark.ParentId);
            if (parent != null)
            {
                var info = new BookmarkBarInfo(bookmark, parent.Level + 1, parent, BookmarkClickAction, BookmarkMenuClickAction);
                parent.Add(info);

                var needUpdateLayout = NeedUpdateMenuLayout(info, bookmark.ParentId);
                if (needUpdateLayout)
                {
                    UpdateMenuLayout();
                }
            }
        }

        public void NewFolder(BookmarkBarInfo info, FolderInfo folder)
        {
            var parentId = info.GetCurrentFolderId();
            var newInfo = new BookmarkBarInfo(folder, info.Level + 1, info, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);

            if (parentId == BookmarkManager.Instance.Folder.Id)
            {
                allBookmark.Add(newInfo);
            }
            else
            {
                info.Add(newInfo);
            }

            var needUpdateLayout = NeedUpdateMenuLayout(info, parentId);
            if (needUpdateLayout)
            {
                UpdateMenuLayout();
            }
        }

        private BookmarkBarInfo? SearchFolder(Guid searchId)
        {
            var searchData = otherBookmarks.Union(new List<BookmarkBarInfo>() { allBookmark });
            return BookmarkBarInfo.SearchFolder(searchData, searchId);
        }

        private bool NeedUpdateMenuLayout(BookmarkBarInfo info, Guid newParentId)
        {
            //简单判断：涉及主书签和其他书签就更新菜单布局
            if (mainBookmarks.Contains(info) || otherBookmarks.First().Children.Contains(info)) return true;
            if (newParentId == Guid.Empty) return false;

            return newParentId == BookmarkManager.Instance.Folder.Id || newParentId == BookmarkManager.Instance.OtherFolder.Id;
        }
    }
}
