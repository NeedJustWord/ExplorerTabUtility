using System.Windows;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.UI.Views.Controls;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkSavePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkSavePopup : BaseWindow
    {
        public BookmarkSavePopup(ExplorerWatcher explorerWatcher) : base(explorerWatcher)
        {
            InitializeComponent();

            Init();
            SetupEventHandlers();
        }

        private void Init()
        {
            SpLocation.Visibility = Visibility.Collapsed;
            BtnNewFolder.Visibility = Visibility.Collapsed;
            TvSelectSavePath.Visibility = Visibility.Collapsed;

            InitCbSelectSavePath();
        }

        private void InitCbSelectSavePath()
        {
        }

        private void InitTvSelectSavePath()
        {
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkSavePopup_Deactivated;
            SizeChanged += BookmarkSavePopup_SizeChanged;
            CbSelectSavePath.SelectOtherFolderClick += CbSelectSavePath_SelectOtherFolderClick;
            BtnNewFolder.Click += BtnNewFolder_Click;
            BtnSave.Click += BtnSave_Click;
            BtnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("保存", "title");
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CbSelectSavePath_SelectOtherFolderClick(object sender, RoutedEventArgs e)
        {
            SpLocation.Visibility = Visibility.Visible;
            BtnNewFolder.Visibility = Visibility.Visible;
            TvSelectSavePath.Visibility = Visibility.Visible;
            CbSelectSavePath.Visibility = Visibility.Collapsed;
            InitTvSelectSavePath();
        }

        private void BookmarkSavePopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != 0) Top -= (e.NewSize.Height - e.PreviousSize.Height) / 2;
        }

        private void BookmarkSavePopup_Deactivated(object? sender, System.EventArgs e)
        {
            if (CanClose()) CloseWindow();
        }
        #endregion
    }
}
