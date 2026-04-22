using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views.Controls
{
    internal class BookmarkTreeView : TreeView
    {
        /// <summary>
        /// 获取选中项的SaveFolderItem
        /// </summary>
        /// <returns></returns>
        public SaveFolderItem? GetSaveFolderItem()
        {
            return ((BookmarkTreeViewItem)SelectedItem)?.SaveFolderItem;
        }

        /// <summary>
        /// 选中项新建文件夹，返回是否成功
        /// </summary>
        /// <param name="errorMsg">失败时的消息</param>
        /// <returns></returns>
        public bool AddFolder(out string errorMsg)
        {
            var selected = (BookmarkTreeViewItem)SelectedItem;
            if (selected == null)
            {
                errorMsg = "请选择要新建文件夹的路径";
                return false;
            }

            if (BookmarkManager.Instance.Save(selected.SaveFolderItem.Key, "新建文件夹", out var newFolder))
            {
                CreateItemWithOutBookmark(selected, newFolder, newFolder.Id, false);
                errorMsg = string.Empty;
                return true;
            }
            else
            {
                errorMsg = "保存失败";
                return false;
            }
        }

        #region 设置数据源
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="folders">数据源</param>
        /// <param name="withBookmark">是否带书签</param>
        public void SetItems(IReadOnlyCollection<FolderInfo> folders, Guid expandedId, bool withBookmark)
        {
            Items.Clear();
            if (withBookmark)
            {
                foreach (var item in folders)
                {
                    CreateItemWithBookmark(this, item);
                }
            }
            else
            {
                foreach (var item in folders)
                {
                    CreateItemWithOutBookmark(this, item, expandedId, true);
                }
            }
        }

        /// <summary>
        /// 创建带书签的数据项
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        private BookmarkTreeViewItem CreateItemWithBookmark(ItemsControl parent, FolderInfo folder)
        {
            var result = new BookmarkTreeViewItem(folder);
            parent.Items.Add(result);

            if (folder.Items.Count > 0)
            {
                foreach (var item in folder.Items)
                {
                    if (item is FolderInfo folderInfo)
                    {
                        CreateItemWithBookmark(result, folderInfo);
                    }
                    else if (item is BookmarkInfo bookmarkInfo)
                    {
                        result.Items.Add(new BookmarkTreeViewItem(bookmarkInfo));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 创建不带书签的数据项
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="folder"></param>
        /// <param name="expandedId"></param>
        /// <param name="expandedAll"></param>
        private BookmarkTreeViewItem CreateItemWithOutBookmark(ItemsControl parent, FolderInfo folder, Guid expandedId, bool expandedAll)
        {
            var result = new BookmarkTreeViewItem(folder);
            parent.Items.Add(result);

            if (folder.Items.Count > 0)
            {
                var folders = folder.Items.OfType<FolderInfo>();
                foreach (var item in folders)
                {
                    CreateItemWithOutBookmark(result, item, expandedId, expandedAll);
                }
            }
            if (folder.Id == expandedId)
            {
                Expanded(result, expandedAll);
            }

            return result;
        }

        private void Expanded(BookmarkTreeViewItem item, bool expandedAll)
        {
            if (expandedAll)
            {
                var parent = item.Parent as BookmarkTreeViewItem;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.Parent as BookmarkTreeViewItem;
                }
            }
            else
            {
                var parent = item.Parent as BookmarkTreeViewItem;
                if (parent != null) parent.IsExpanded = true;
            }
        }
        #endregion
    }

    internal class BookmarkTreeViewItem : TreeViewItem
    {
        private readonly FolderInfo? folder;
        private readonly BookmarkInfo? bookmark;

        public SaveFolderItem SaveFolderItem { get; private set; }

        public BookmarkTreeViewItem(FolderInfo folder)
        {
            this.folder = folder;
            Header = folder.Name;
            SaveFolderItem = new SaveFolderItem(folder.Id, folder.Name);
        }

        public BookmarkTreeViewItem(BookmarkInfo bookmark)
        {
            this.bookmark = bookmark;
            Header = bookmark.Name;
            SaveFolderItem = new SaveFolderItem(bookmark.Id, bookmark.Name, bookmark.Location);
        }
    }
}
