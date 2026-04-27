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

            BookmarkBar.Click += BookmarkBar_Click;
        }

        private async void BookmarkBar_Click(BookmarkBarClickArgs args)
        {
            switch (args.OpenType)
            {
                case BookmarkOpenType.CurrentTab:
                    await explorerWatcher.Open(args.Bookmark.Location, true, windowHandle, inCurrentTab: true);
                    break;
                case BookmarkOpenType.NewTab:
                    await explorerWatcher.Open(args.Bookmark.Location, true, windowHandle, inCurrentTab: false);
                    break;
                case BookmarkOpenType.NewWindow:
                    await explorerWatcher.Open(args.Bookmark.Location, false, windowHandle);
                    break;
            }
            CloseWindow();
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
