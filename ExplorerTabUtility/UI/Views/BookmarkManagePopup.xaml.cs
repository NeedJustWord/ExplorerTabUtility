using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkManagePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkManagePopup : BaseWindow
    {
        private bool needDialogResult;
        private string initBookmarkJson;

        public BookmarkManagePopup(ExplorerWatcher explorerWatcher, nint windowHandle, bool needDialogResult) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            Init(needDialogResult);
            SetupEventHandlers();

            initBookmarkJson = GetJson(TvFolder.CopyFolderInfos());
        }

        public void AddFolder(BookmarkTreeViewInfo info)
        {
            if (TvFolder.AddFolder(info, out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        #region 私有函数
        private void Init(bool needDialogResult)
        {
            this.needDialogResult = needDialogResult;
            TvFolder.SetItemsSource(BookmarkManager.Bookmarks, BookmarkManager.Folder.Id, true);
        }

        private void CloseWindow(bool isCancel)
        {
            var dialogResult = false;
            var infos = TvFolder.CopyFolderInfos();
            var afterJson = GetJson(infos);

            if (afterJson != initBookmarkJson)
            {
                if (isCancel)
                {
                    BookmarkManager.RecoverConfig();
                }
                else
                {
                    dialogResult = true;
                    BookmarkManager.Save(infos);
                }
            }

            if (needDialogResult)
            {
                DialogResult = dialogResult;
            }

            CloseWindow();
        }

        private string GetJson<T>(T t)
        {
            return JsonSerializer.Serialize(t);
        }

        private void DeleteSelectedItems()
        {
            var items = LbChildren.SelectedItems.Cast<BookmarkTreeViewInfo>().ToList();
            if (items.Count > 0) TvFolder.Delete(items);
        }

        private void Edit(BookmarkTreeViewInfo info)
        {
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, info);
            popup.ShowDialog();
        }
        #endregion

        #region 事件注册
        private void SetupEventHandlers()
        {
            KeyDown += BookmarkManagePopup_KeyDown;
            BtnCancel.Click += BtnCancel_Click;
            BtnSave.Click += BtnSave_Click;
            BtnNewFolder.Click += BtnNewFolder_Click;
            TvFolder.SelectedItemChanged += TvFolder_SelectedItemChanged;
            TvFolder.BookmarkHandle += TvFolder_BookmarkHandle;
            TvFolder.FolderHandle += TvFolder_FolderHandle;
            LbChildren.KeyDown += LbChildren_KeyDown;
            TxtSearch.GotFocus += TxtSearch_GotFocus;
            TxtSearch.TextChanged += TxtSearch_TextChanged;
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtSearch.SelectAll();
        }

        private void LbChildren_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelectedItems();
                    e.Handled = true;
                    break;
                case Key.F2:
                    if (LbChildren.SelectedItems.Count == 1)
                    {
                        var info = (BookmarkTreeViewInfo)LbChildren.SelectedItem;
                        Edit(info);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void TvFolder_FolderHandle(BookmarkTreeViewInfo info, FolderInfo folder, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.Delete:
                    DeleteSelectedItems();
                    break;
                case BookmarkBarAction.Rename:
                    Edit(info);
                    break;
            }
        }

        private async void TvFolder_BookmarkHandle(BookmarkTreeViewInfo info, BookmarkInfo bookmark, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.OpenInCurrentTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: true);
                    break;
                case BookmarkBarAction.OpenInNewTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: false);
                    break;
                case BookmarkBarAction.OpenInNewWindow:
                    await explorerWatcher.Open(bookmark.Location, false, windowHandle);
                    break;
                case BookmarkBarAction.Delete:
                    DeleteSelectedItems();
                    break;
                case BookmarkBarAction.Edit:
                    Edit(info);
                    break;
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var key = TxtSearch.Text;
            if (string.IsNullOrEmpty(key))
            {
                var info = (BookmarkTreeViewInfo)TvFolder.SelectedItem;
                LbChildren.ItemsSource = info.Children;
            }
            else
            {
                LbChildren.ItemsSource = TvFolder.Search(key);
            }
        }

        private void TvFolder_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var info = (BookmarkTreeViewInfo)e.NewValue;
            if (info != null) LbChildren.ItemsSource = info.Children;
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (TvFolder.AddFolder(out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(false);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(true);
        }

        private void BookmarkManagePopup_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    CloseWindow(true);
                    break;
                case Key.F:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        TxtSearch.Focus();
                    }
                    break;
                case Key.S:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        CloseWindow(false);
                    }
                    break;
            }
        }
        #endregion
    }
}
