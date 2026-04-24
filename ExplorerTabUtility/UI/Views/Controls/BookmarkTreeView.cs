using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views.Controls
{
    internal class BookmarkTreeView : TreeView
    {
        public BookmarkTreeView()
        {
            SetupEventHandlers();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            KeyDown += BookmarkTreeView_KeyDown;
        }

        private void BookmarkTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Rename();
            }
        }
        #endregion

        #region 功能
        /// <summary>
        /// 重命名
        /// </summary>
        public void Rename()
        {
            var info = (BookmarkTreeViewInfo)SelectedItem;
            if (info == null || info.Parent == null) return;

            info.IsEditMode = true;
        }

        /// <summary>
        /// 获取选中项的SaveFolderItem
        /// </summary>
        /// <returns></returns>
        public SaveFolderItem? GetSaveFolderItem()
        {
            return ((BookmarkTreeViewInfo)SelectedItem)?.SaveFolderItem;
        }

        /// <summary>
        /// 选中项新建文件夹，返回是否成功
        /// </summary>
        /// <param name="errorMsg">失败时的消息</param>
        /// <returns></returns>
        public bool AddFolder(out string errorMsg)
        {
            var selected = (BookmarkTreeViewInfo)SelectedItem;
            if (selected == null)
            {
                errorMsg = "请选择要新建文件夹的路径";
                return false;
            }

            var newFolder = new FolderInfo(Guid.Empty, "新建文件夹");
            var newInfo = new BookmarkTreeViewInfo(newFolder, selected.Level + 1, selected, false)
            {
                IsEditMode = true
            };
            selected.Children.Add(newInfo);
            selected.IsExpanded = true;
            errorMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// 新建文件夹，返回是否成功
        /// </summary>
        /// <param name="newInfo"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool AddFolder(BookmarkTreeViewInfo newInfo, out string errorMsg)
        {
#pragma warning disable CS8602 // 解引用可能出现空引用。
            if (BookmarkManager.Instance.Save(newInfo.Parent.Id, newInfo.CurrentFolder, newInfo.Name))
#pragma warning restore CS8602 // 解引用可能出现空引用。
            {
                newInfo.UpdateFolder();
                newInfo.IsEditMode = false;

                errorMsg = string.Empty;
                return true;
            }
            else
            {
                errorMsg = "保存失败";
                return false;
            }
        }
        #endregion

        #region 设置数据源
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="folders">数据源</param>
        /// <param name="expandedId">要显示的id</param>
        /// <param name="withBookmark">是否带书签</param>
        public void SetItemsSource(IReadOnlyCollection<FolderInfo> folders, Guid expandedId, bool withBookmark)
        {
            var datas = new ObservableCollection<BookmarkTreeViewInfo>();
            if (withBookmark)
            {
                int index = 0;
                foreach (var item in folders)
                {
                    datas.Add(CreateItemWithBookmark(item, 0, null, index == 0));
                    index++;
                }
            }
            else
            {
                int index = 0;
                foreach (var item in folders)
                {
                    datas.Add(CreateItemWithOutBookmark(item, 0, null, index == 0, expandedId, true));
                    index++;
                }
            }
            ItemsSource = datas;
        }

        /// <summary>
        /// 创建带书签的数据项
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="isSpecil"></param>
        /// <returns></returns>
        private BookmarkTreeViewInfo CreateItemWithBookmark(FolderInfo folder, int level, BookmarkTreeViewInfo? parent, bool isSpecil)
        {
            var result = new BookmarkTreeViewInfo(folder, level, parent, isSpecil);
            if (folder.Items.Count > 0)
            {
                level++;
                foreach (var item in folder.Items)
                {
                    if (item is FolderInfo folderInfo)
                    {
                        result.Children.Add(CreateItemWithBookmark(folderInfo, level, result, false));
                    }
                    else if (item is BookmarkInfo bookmarkInfo)
                    {
                        result.Children.Add(new BookmarkTreeViewInfo(bookmarkInfo, level, result));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 创建不带书签的数据项
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="isSpecil"></param>
        /// <param name="expandedId"></param>
        /// <param name="expandedAll"></param>
        /// <returns></returns>
        private BookmarkTreeViewInfo CreateItemWithOutBookmark(FolderInfo folder, int level, BookmarkTreeViewInfo? parent, bool isSpecil, Guid expandedId, bool expandedAll)
        {
            var result = new BookmarkTreeViewInfo(folder, level, parent, isSpecil);
            if (folder.Items.Count > 0)
            {
                level++;
                var folders = folder.Items.OfType<FolderInfo>();
                foreach (var folderInfo in folders)
                {
                    result.Children.Add(CreateItemWithOutBookmark(folderInfo, level, result, false, expandedId, expandedAll));
                }
            }
            if (folder.Id == expandedId)
            {
                result.IsSelected = true;
                Expanded(result, expandedAll);
            }

            return result;
        }

        private void Expanded(BookmarkTreeViewInfo item, bool expandedAll)
        {
            if (expandedAll)
            {
                var parent = item.Parent;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.Parent;
                }
            }
            else
            {
                var parent = item.Parent;
                if (parent != null) parent.IsExpanded = true;
            }
        }
        #endregion
    }
}
