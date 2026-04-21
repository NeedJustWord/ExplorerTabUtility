using ExplorerTabUtility.Hooks;
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

            SetupEventHandlers();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkManagePopup_Deactivated;
        }

        private void BookmarkManagePopup_Deactivated(object? sender, System.EventArgs e)
        {
            if (CanClose()) CloseWindow();
        }
        #endregion
    }
}
