using System;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkNavigatePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkNavigatePopup : BaseBookmarkWindow
    {
        public BookmarkNavigatePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            var margin = 10;
            var currentScreenWorkingArea = ScreenHelper.MouseCurrentScreen.WorkingArea;
            Top = margin + currentScreenWorkingArea.Top;
            Left = margin + currentScreenWorkingArea.Left;
            BookmarkMenu.Width = Width = currentScreenWorkingArea.Width - margin * 2;
            BookmarkMenu.InitLayout();

            SetupEventHandlers();
        }

        private void BookmarkManager()
        {
            var popup = new BookmarkManagePopup(explorerWatcher, windowHandle, true);
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.Refresh();
            }
            ExitDialog();
        }

        private void NewBookmark(Guid parentId)
        {
            var bookmark = CreateCurrentLocationBookmark();
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, parentId);
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.NewBookmark(bookmark);
            }
            ExitDialog();
        }

        private void NewFolder(BookmarkMenuInfo info)
        {
            var newFolder = new FolderInfo(Guid.Empty, "新建文件夹");
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, newFolder, info.GetCurrentFolderId());
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.NewFolder(info, newFolder);
            }
            ExitDialog();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkNavigatePopup_Deactivated;
            KeyDown += BookmarkNavigatePopup_KeyDown;

            BookmarkMenu.BookmarkHandle += BookmarkMenu_BookmarkHandle;
            BookmarkMenu.FolderHandle += BookmarkMenu_FolderHandle;
        }

        private void BookmarkMenu_FolderHandle(BookmarkMenuInfo info, FolderInfo folder, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.Rename:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, folder, Guid.Empty);
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkMenu.RenameFolder(info, folder.Name);
                    }
                    ExitDialog();
                    break;
                case BookmarkAction.BookmarkManage:
                    BookmarkManager();
                    break;
                case BookmarkAction.NewBookmark:
                    NewBookmark(folder.Id);
                    break;
                case BookmarkAction.NewFolder:
                    NewFolder(info);
                    break;
            }
        }

        private async void BookmarkMenu_BookmarkHandle(BookmarkMenuInfo info, BookmarkInfo bookmark, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.OpenInCurrentTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: true);
                    CloseWindow();
                    break;
                case BookmarkAction.OpenInNewTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: false);
                    CloseWindow();
                    break;
                case BookmarkAction.OpenInNewWindow:
                    await explorerWatcher.Open(bookmark.Location, false, windowHandle);
                    CloseWindow();
                    break;
                case BookmarkAction.Edit:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, info.GetParentId());
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkMenu.EditBookmark(info, bookmark);
                    }
                    ExitDialog();
                    break;
                case BookmarkAction.BookmarkManage:
                    BookmarkManager();
                    break;
                case BookmarkAction.NewBookmark:
                    NewBookmark(info.GetParentId());
                    break;
                case BookmarkAction.NewFolder:
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    NewFolder(info.Parent);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                    break;
            }
        }

        private void BookmarkNavigatePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }

        private void BookmarkNavigatePopup_Deactivated(object? sender, System.EventArgs e)
        {
            if (CanClose()) CloseWindow();
        }
        #endregion
    }
}
