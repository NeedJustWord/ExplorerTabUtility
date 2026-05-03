using System.Windows;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
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
        private bool needDialogResult;

        public BookmarkManagePopup(ExplorerWatcher explorerWatcher, nint windowHandle, bool needDialogResult) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            Init(needDialogResult);
            SetupEventHandlers();
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
            TvFolder.SetItemsSource(BookmarkManager.Instance.Bookmarks, BookmarkManager.Instance.Folder.Id, true);
        }

        private void CloseWindow(bool isCancel)
        {
            if (isCancel && TvFolder.HaveSave)
            {
                BookmarkManager.Instance.RecoverConfig();
            }

            CloseWindow();
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
            TxtSearch.TextChanged += TxtSearch_TextChanged;
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
            LbChildren.ItemsSource = info.Children;
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
            if (needDialogResult)
            {
                DialogResult = false;
                //todo:set DialogResult
            }

            CloseWindow(false);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(true);
        }

        private void BookmarkManagePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow(true);
            }
        }
        #endregion
    }
}
