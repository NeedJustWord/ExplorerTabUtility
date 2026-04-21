using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkSavePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkSavePopup : BaseWindow
    {
        private BookmarkSaveType saveType;
        private WindowRecord currentWindowRecord;

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            saveType = BookmarkSaveType.ComboBox;
            currentWindowRecord = GetCurrentTabWindowRecord() ?? new WindowRecord(string.Empty, windowHandle);

            Init();
            SetupEventHandlers();
        }

        private void Init()
        {
            BtnNewFolder.IsEnabled = false;
            SpLocation.Visibility = Visibility.Collapsed;
            BtnNewFolder.Visibility = Visibility.Collapsed;
            TvSelectSavePath.Visibility = Visibility.Collapsed;

            TxtName.Text = Path.GetFileName(currentWindowRecord.DisplayLocation);

            InitCbSelectSavePath();
        }

        private void InitCbSelectSavePath()
        {
            var lastSaveFolders = BookmarkManager.Instance.LastSaveFolders
                .Select(t => new SaveFolderItem(t.Id, t.Name))
                .ToList();
            CbSelectSavePath.ItemsSource = lastSaveFolders;
            CbSelectSavePath.SelectedItem = lastSaveFolders.FirstOrDefault();
        }

        private void InitTvSelectSavePath()
        {
            var lastSaveFolders = BookmarkManager.Instance.Bookmarks
                .Select(t => new BookmarkTreeViewItem(t))
                .ToList();
            TvSelectSavePath.ItemsSource = new ObservableCollection<BookmarkTreeViewItem>(lastSaveFolders);
        }

        private SaveFolderItem? GetSaveFolder()
        {
            switch (saveType)
            {
                case BookmarkSaveType.ComboBox:
                    return (SaveFolderItem)CbSelectSavePath.SelectedItem;
                case BookmarkSaveType.TreeView:
                    return (SaveFolderItem)TvSelectSavePath.SelectedItem;
                default:
                    return null;
            }
        }

        private string GetSaveLocation()
        {
            switch (saveType)
            {
                case BookmarkSaveType.ComboBox:
                    return currentWindowRecord.DisplayLocation;
                default:
                    return TxtLocation.Text;
            }
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
            TvSelectSavePath.SelectedItemChanged += TvSelectSavePath_SelectedItemChanged;
        }

        private void TvSelectSavePath_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BtnNewFolder.IsEnabled = TvSelectSavePath.SelectedItem != null;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var saveFolder = GetSaveFolder();
            if (saveFolder == null)
            {
                ShowMessage("请选择要保存的路径", Constants.AppName);
                return;
            }

            BookmarkManager.Instance.Save(saveFolder.Key, TxtName.Text, GetSaveLocation());
            CloseWindow();
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            var selected = TvSelectSavePath.SelectedItem as BookmarkTreeViewItem;
            if (selected == null)
            {
                ShowMessage("请选择要新建文件夹的路径", Constants.AppName);
                return;
            }

            selected.Add(new FolderInfo(Guid.Empty, "新建文件夹"));
        }

        private void CbSelectSavePath_SelectOtherFolderClick(object sender, RoutedEventArgs e)
        {
            SpLocation.Visibility = Visibility.Visible;
            BtnNewFolder.Visibility = Visibility.Visible;
            TvSelectSavePath.Visibility = Visibility.Visible;
            CbSelectSavePath.Visibility = Visibility.Collapsed;

            saveType = BookmarkSaveType.TreeView;
            TxtLocation.Text = currentWindowRecord.DisplayLocation;
            TxtLocation.ToolTip = currentWindowRecord.DisplayLocation;

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
