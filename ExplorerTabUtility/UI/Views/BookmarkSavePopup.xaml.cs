using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
        private bool isEdit;
        private Guid parentId;
        private BookmarkSaveType saveType;
        private BookmarkInfo currentBookmarkInfo;
        private FolderInfo currentFolderInfo;

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            var location = GetLocation();
            currentBookmarkInfo = new BookmarkInfo(Guid.Empty, GetName(location), location);
            currentFolderInfo = FolderInfo.Empty;
            saveType = BookmarkSaveType.ComboBox;

            Init();
            SetupEventHandlers();
        }

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle, FolderInfo folderInfo) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            TxtTitle.Text = "重命名";
            currentBookmarkInfo = BookmarkInfo.Empty;
            currentFolderInfo = folderInfo;
            saveType = BookmarkSaveType.FolderRename;

            Init();
            SetupBaseEventHandlers();
        }

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle, BookmarkInfo bookmarkInfo, Guid parentId) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            this.parentId = parentId;
            isEdit = true;
            TxtTitle.Text = "编辑";
            currentBookmarkInfo = bookmarkInfo;
            currentFolderInfo = FolderInfo.Empty;
            saveType = BookmarkSaveType.TreeView;

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
            switch (saveType)
            {
                case BookmarkSaveType.FolderRename:
                    TxtName.Text = currentFolderInfo.Name;
                    SpLocation.Visibility = Visibility.Collapsed;
                    BtnNewFolder.Visibility = Visibility.Collapsed;
                    TxtFolder.Visibility = Visibility.Collapsed;
                    CbSelectSavePath.Visibility = Visibility.Collapsed;
                    TvSelectSavePath.Visibility = Visibility.Collapsed;
                    break;
                case BookmarkSaveType.ComboBox:
                    BtnNewFolder.IsEnabled = false;
                    SpLocation.Visibility = Visibility.Collapsed;
                    BtnNewFolder.Visibility = Visibility.Collapsed;
                    TvSelectSavePath.Visibility = Visibility.Collapsed;

                    TxtName.Text = GetName(currentBookmarkInfo.Location);

                    InitCbSelectSavePath();
                    break;
                case BookmarkSaveType.TreeView:
                    BtnNewFolder.IsEnabled = false;
                    CbSelectSavePath.Visibility = Visibility.Collapsed;

                    TxtName.Text = currentBookmarkInfo.Name;
                    TxtLocation.Text = currentBookmarkInfo.Location;
                    TxtLocation.ToolTip = currentBookmarkInfo.Location;

                    InitTvSelectSavePath(parentId);
                    break;
            }
        }

        private void InitCbSelectSavePath()
        {
            var lastSaveFolders = BookmarkManager.Instance.LastSaveFolders
                .Select(t => new SaveFolderItem(t.Id, t.Name))
                .ToList();
            CbSelectSavePath.ItemsSource = lastSaveFolders;
            CbSelectSavePath.SelectedItem = lastSaveFolders.First();
        }

        private void InitTvSelectSavePath(Guid parentId)
        {
            TvSelectSavePath.SetItemsSource(BookmarkManager.Instance.Bookmarks, parentId, false);
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
                    return currentBookmarkInfo.Location;
                default:
                    return TxtLocation.Text;
            }
        }

        private string GetLocation()
        {
            var record = explorerWatcher.GetCurrentTabWindowRecord(windowHandle);
            return record == null ? string.Empty : record.DisplayLocation;
        }

        private string GetName(string location)
        {
            string name;
            switch (location)
            {
                case "shell:::{20D04FE0-3AEA-1069-A2D8-08002B30309D}":
                    name = "此电脑";
                    break;
                default:
                    if (location.EndsWith(":"))
                    {
                        name = $"{location.TrimEnd(':')}盘";
                    }
                    else
                    {
                        name = Path.GetFileName(location);
                    }
                    break;
            }
            return name;
        }

        private void CloseWindow(bool isCancel)
        {
            if (isCancel && TvSelectSavePath.HaveSave)
            {
                BookmarkManager.Instance.RecoverConfig();
            }

            CloseWindow();
        }

        #region 事件注册
        private void SetupBaseEventHandlers()
        {
            KeyDown += BookmarkSavePopup_KeyDown;
            BtnSave.Click += BtnSave_Click;
            BtnCancel.Click += BtnCancel_Click;
        }

        private void SetupEventHandlers()
        {
            SetupBaseEventHandlers();

            SizeChanged += BookmarkSavePopup_SizeChanged;
            BtnNewFolder.Click += BtnNewFolder_Click;
            TvSelectSavePath.SelectedItemChanged += TvSelectSavePath_SelectedItemChanged;

            if (saveType == BookmarkSaveType.ComboBox)
            {
                CbSelectSavePath.SelectOtherFolderClick += CbSelectSavePath_SelectOtherFolderClick;
            }
        }

        private void TvSelectSavePath_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BtnNewFolder.IsEnabled = TvSelectSavePath.SelectedItem != null;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(true);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveType == BookmarkSaveType.FolderRename)
            {
                BookmarkManager.Instance.FolderRename(currentFolderInfo, TxtName.Text);
                DialogResult = true;
            }
            else
            {
                var saveFolder = GetSaveFolder();
                if (saveFolder == null)
                {
                    ShowMessage("请选择要保存的路径", Constants.AppName);
                    return;
                }

                if (BookmarkManager.Instance.Save(isEdit, parentId, saveFolder.Key, currentBookmarkInfo, TxtName.Text, GetSaveLocation()) == false)
                {
                    ShowMessage("保存失败", Constants.AppName);
                    return;
                }

                currentBookmarkInfo.ParentId = saveFolder.Key;
                if (isEdit) DialogResult = true;
            }

            CloseWindow(false);
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
            TxtLocation.Text = currentBookmarkInfo.Location;
            TxtLocation.ToolTip = currentBookmarkInfo.Location;

            InitTvSelectSavePath(BookmarkManager.Instance.LastSaveFolders.First().Id);
        }

        private void BookmarkSavePopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != 0) Top -= (e.NewSize.Height - e.PreviousSize.Height) / 2;
        }

        private void BookmarkSavePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow(true);
            }
        }
        #endregion
    }
}
