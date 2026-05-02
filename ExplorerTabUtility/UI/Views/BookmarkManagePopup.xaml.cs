using System.Windows;
using System.Windows.Input;
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
            KeyDown += BookmarkManagePopup_KeyDown;
            BtnCancel.Click += BtnCancel_Click;
            BtnSave.Click += BtnSave_Click;
            BtnNewFolder.Click += BtnNewFolder_Click;
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BookmarkManagePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }
        #endregion
    }
}
