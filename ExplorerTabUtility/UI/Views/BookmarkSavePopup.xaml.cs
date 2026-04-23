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

        public void AddFolder(BookmarkTreeViewInfo info)
        {
            if (TvSelectSavePath.AddFolder(info, out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        private void Init()
        {
            BtnNewFolder.IsEnabled = false;
            SpLocation.Visibility = Visibility.Collapsed;
            BtnNewFolder.Visibility = Visibility.Collapsed;
            TvSelectSavePath.Visibility = Visibility.Collapsed;

            TxtName.Text = GetName();

            InitCbSelectSavePath();
        }

        private void InitCbSelectSavePath()
        {
            var lastSaveFolders = BookmarkManager.Instance.LastSaveFolders
                .Select(t => new SaveFolderItem(t.Id, t.Name))
                .ToList();
            CbSelectSavePath.ItemsSource = lastSaveFolders;
            CbSelectSavePath.SelectedItem = lastSaveFolders.First();
        }

        private void InitTvSelectSavePath()
        {
            TvSelectSavePath.SetItemsSource(BookmarkManager.Instance.Bookmarks, BookmarkManager.Instance.LastSaveFolders.First().Id, false);
        }

        private SaveFolderItem? GetSaveFolder()
        {
            switch (saveType)
            {
                case BookmarkSaveType.ComboBox:
                    return (SaveFolderItem)CbSelectSavePath.SelectedItem;
                case BookmarkSaveType.TreeView:
                    return TvSelectSavePath.GetSaveFolderItem();
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

        private string GetName()
        {
            string name;
            switch (currentWindowRecord.DisplayLocation)
            {
                case "shell:::{20D04FE0-3AEA-1069-A2D8-08002B30309D}":
                    name = "此电脑";
                    break;
                default:
                    if (currentWindowRecord.DisplayLocation.EndsWith(":"))
                    {
                        name = $"{currentWindowRecord.DisplayLocation.TrimEnd(':')}盘";
                    }
                    else
                    {
                        name = Path.GetFileName(currentWindowRecord.DisplayLocation);
                    }
                    break;
            }
            return name;
        }

        private void CloseWindow(bool save)
        {
            save |= TvSelectSavePath.HaveSave;
            if (save) BookmarkManager.Instance.SaveConfig();

            CloseWindow();
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
            CloseWindow(false);
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
            CloseWindow(true);
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (TvSelectSavePath.AddFolder(out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
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
            //if (CanClose()) CloseWindow(false);
        }
        #endregion
    }
}
