using System;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ExplorerTabUtility.UI.Commands;

namespace ExplorerTabUtility.Models
{
    class BookmarkBarInfo : BindableBase
    {
        #region 属性
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string icon;
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }

        private PlacementMode placementMode;
        /// <summary>
        /// 弹出方向
        /// </summary>
        public PlacementMode PlacementMode
        {
            get { return placementMode; }
            set { SetProperty(ref placementMode, value); }
        }

        private ObservableCollection<BookmarkBarInfo> children;
        /// <summary>
        /// 子集合
        /// </summary>
        public ObservableCollection<BookmarkBarInfo> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        /// <summary>
        /// 是否显示子项图标
        /// </summary>
        public bool IsShowSubIcon => IsFolder && FirstLevel == false && children.Count > 0;

        /// <summary>
        /// 是否显示
        /// 书签、非溢出书签文件夹和其他书签文件夹：始终显示
        /// 溢出书签文件夹和其他书签文件夹：有子项时显示
        /// </summary>
        public bool IsVisibility => IsFolder == false || isNotOverflowAndOtherFolder || children.Count > 0;

        /// <summary>
        /// 是否是文件夹
        /// </summary>
        public bool IsFolder { get; private set; }

        /// <summary>
        /// 节点等级
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 是否是第一级
        /// </summary>
        public bool FirstLevel { get; private set; }

        /// <summary>
        /// 点击事件
        /// </summary>
        public ICommand? ClickCommand { get; private set; }
        #endregion

        /// <summary>
        /// 非溢出书签和其他书签文件夹
        /// </summary>
        private bool isNotOverflowAndOtherFolder;
        private BookmarkBarInfo? parent;
        private BookmarkInfo? bookmark;
        private FolderInfo? folder;
        private static ObservableCollection<BookmarkBarInfo> emptyChildren = new ObservableCollection<BookmarkBarInfo>();

        /// <summary>
        /// 书签
        /// </summary>
        /// <param name="bookmarkInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        public BookmarkBarInfo(BookmarkInfo bookmarkInfo, int level, BookmarkBarInfo? parent, Action<BookmarkInfo> action)
        {
            isNotOverflowAndOtherFolder = true;
            this.parent = parent;
            bookmark = bookmarkInfo;
            name = bookmarkInfo.Name;
            icon = "📄";
            Init(false, level);

            children = emptyChildren;
            ClickCommand = new RelayCommand((args) =>
            {
                action.Invoke(bookmark);
            });
        }

        /// <summary>
        /// 文件夹
        /// </summary>
        /// <param name="folderInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        /// <param name="isNotOverflowAndOtherFolder">非溢出书签和其他书签</param>
        public BookmarkBarInfo(FolderInfo folderInfo, int level, BookmarkBarInfo? parent, Action<BookmarkInfo> action, bool isNotOverflowAndOtherFolder)
        {
            this.isNotOverflowAndOtherFolder = isNotOverflowAndOtherFolder;
            this.parent = parent;
            folder = folderInfo;
            name = folderInfo.Name;
            icon = "📁";
            Init(true, level);

            children = new ObservableCollection<BookmarkBarInfo>();
            CreateChildren(folderInfo, level, action);
        }

        /// <summary>
        /// 溢出文件夹
        /// </summary>
        /// <param name="folderInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        public BookmarkBarInfo(FolderInfo folderInfo, int level, BookmarkBarInfo? parent)
        {
            isNotOverflowAndOtherFolder = false;
            this.parent = parent;
            folder = folderInfo;
            name = folderInfo.Name;
            icon = "";
            Init(true, level);

            children = new ObservableCollection<BookmarkBarInfo>();
        }

        private void Init(bool isFolder, int level)
        {
            Level = level;
            FirstLevel = level <= 0;
            IsFolder = isFolder;
            PlacementMode = FirstLevel ? PlacementMode.Bottom : PlacementMode.Right;
        }

        private void CreateChildren(FolderInfo folder, int level, Action<BookmarkInfo> action)
        {
            if (folder.Items.Count > 0)
            {
                level++;
                foreach (var item in folder.Items)
                {
                    if (item is FolderInfo folderInfo)
                    {
                        children.Add(new BookmarkBarInfo(folderInfo, level, this, action, true));
                    }
                    else if (item is BookmarkInfo bookmarkInfo)
                    {
                        children.Add(new BookmarkBarInfo(bookmarkInfo, level, this, action));
                    }
                }
            }
        }
    }
}
