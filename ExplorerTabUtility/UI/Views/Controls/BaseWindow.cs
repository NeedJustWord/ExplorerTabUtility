using System.Windows;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;

namespace ExplorerTabUtility.UI.Views.Controls
{
    public class BaseWindow : Window
    {
        protected readonly ExplorerWatcher explorerWatcher;
        private bool _isShowingDialog;
        private bool _isClosing;

        public BaseWindow(ExplorerWatcher explorerWatcher)
        {
            this.explorerWatcher = explorerWatcher;
        }

        public new void Show()
        {
            base.Show();
            if (Activate()) return;

            Helper.BypassWinForegroundRestrictions();
            Activate();
        }

        protected bool CanClose()
        {
            return _isShowingDialog == false;
        }

        protected void CloseWindow()
        {
            if (_isClosing) return;
            _isClosing = true;
            Close();
        }

        protected MessageBoxResult ShowMessage(string message, string title,
            MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultButton = MessageBoxResult.None)
        {
            _isShowingDialog = true;
            var result = CustomMessageBox.Show(message, title, buttons, icon, defaultButton);
            _isShowingDialog = false;
            return result;
        }
    }
}
