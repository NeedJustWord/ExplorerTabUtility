using System.Windows;
using System.Windows.Input;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkManagePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkManagePopup : BaseWindow
    {
        public BookmarkManagePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            Top = Left = 10;
            BookmarkBar.Width = Width = SystemParameters.WorkArea.Width - Top * 2;
            BookmarkBar.InitBookmark(BookmarkManager.Instance.Folder, BookmarkManager.Instance.OtherFolder);

            SetupEventHandlers();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkManagePopup_Deactivated;
            KeyDown += BookmarkManagePopup_KeyDown;

            BookmarkBar.BookmarkHandle += BookmarkBar_BookmarkHandle;
            BookmarkBar.FolderHandle += BookmarkBar_FolderHandle;
        }

        private void BookmarkBar_FolderHandle(BookmarkBarInfo info, FolderInfo folder, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.Rename:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, folder);
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkBar.RenameFolder(info, folder.Name);
                    }
                    ExitDialog();
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
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, info.Parent.CurrentFolder.Id);
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkBar.EditBookmark(info, bookmark);
                    }
                    ExitDialog();
                    break;
            }
        }

        private void BookmarkManagePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }

        private void BookmarkManagePopup_Deactivated(object? sender, System.EventArgs e)
        {
            if (CanClose()) CloseWindow();
        }
        #endregion
    }
}
