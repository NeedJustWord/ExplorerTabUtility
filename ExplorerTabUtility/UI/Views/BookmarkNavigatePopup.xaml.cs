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
    public partial class BookmarkNavigatePopup : BaseWindow
    {
        public BookmarkNavigatePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            var margin = 10;
            var currentScreenWorkingArea = ScreenHelper.MouseCurrentScreen.WorkingArea;
            Top = margin + currentScreenWorkingArea.Top;
            Left = margin + currentScreenWorkingArea.Left;
            BookmarkBar.Width = Width = currentScreenWorkingArea.Width - margin * 2;
            BookmarkBar.InitLayout();

            SetupEventHandlers();
        }

        private void BookmarkManager()
        {
            var popup = new BookmarkManagePopup(explorerWatcher, windowHandle, true);
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkBar.Refresh();
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
                BookmarkBar.NewBookmark(bookmark);
            }
            ExitDialog();
        }

        private void NewFolder(BookmarkBarInfo info)
        {
            var newFolder = new FolderInfo(Guid.Empty, "新建文件夹");
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, newFolder, info.GetCurrentFolderId());
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkBar.NewFolder(info, newFolder);
            }
            ExitDialog();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkNavigatePopup_Deactivated;
            KeyDown += BookmarkNavigatePopup_KeyDown;

            BookmarkBar.BookmarkHandle += BookmarkBar_BookmarkHandle;
            BookmarkBar.FolderHandle += BookmarkBar_FolderHandle;
        }

        private void BookmarkBar_FolderHandle(BookmarkBarInfo info, FolderInfo folder, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.Rename:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, folder, Guid.Empty);
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkBar.RenameFolder(info, folder.Name);
                    }
                    ExitDialog();
                    break;
                case BookmarkBarAction.BookmarkManager:
                    BookmarkManager();
                    break;
                case BookmarkBarAction.NewBookmark:
                    NewBookmark(folder.Id);
                    break;
                case BookmarkBarAction.NewFolder:
                    NewFolder(info);
                    break;
            }
        }

        private async void BookmarkBar_BookmarkHandle(BookmarkBarInfo info, BookmarkInfo bookmark, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.OpenInCurrentTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: true);
                    CloseWindow();
                    break;
                case BookmarkBarAction.OpenInNewTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: false);
                    CloseWindow();
                    break;
                case BookmarkBarAction.OpenInNewWindow:
                    await explorerWatcher.Open(bookmark.Location, false, windowHandle);
                    CloseWindow();
                    break;
                case BookmarkBarAction.Edit:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, info.GetParentId());
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkBar.EditBookmark(info, bookmark);
                    }
                    ExitDialog();
                    break;
                case BookmarkBarAction.BookmarkManager:
                    BookmarkManager();
                    break;
                case BookmarkBarAction.NewBookmark:
                    NewBookmark(info.GetParentId());
                    break;
                case BookmarkBarAction.NewFolder:
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
