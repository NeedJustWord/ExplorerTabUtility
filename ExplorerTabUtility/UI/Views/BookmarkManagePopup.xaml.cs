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

        private void BookmarkBar_FolderHandle(FolderInfo info, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.Rename:
                    break;
            }
        }

        private async void BookmarkBar_BookmarkHandle(BookmarkInfo info, BookmarkBarAction action)
        {
            switch (action)
            {
                case BookmarkBarAction.OpenInCurrentTab:
                    await explorerWatcher.Open(info.Location, true, windowHandle, inCurrentTab: true);
                    CloseWindow();
                    break;
                case BookmarkBarAction.OpenInNewTab:
                    await explorerWatcher.Open(info.Location, true, windowHandle, inCurrentTab: false);
                    CloseWindow();
                    break;
                case BookmarkBarAction.OpenInNewWindow:
                    await explorerWatcher.Open(info.Location, false, windowHandle);
                    CloseWindow();
                    break;
                case BookmarkBarAction.Edit:
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
