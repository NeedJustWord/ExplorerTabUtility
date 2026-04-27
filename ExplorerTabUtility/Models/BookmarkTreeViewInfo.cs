using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.Models
{
    public class BookmarkTreeViewInfo : BindableBase
    {
        #region 属性
        private bool isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        private bool isExpanded;
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private bool isEditMode;
        /// <summary>
        /// 是否编辑模式
        /// </summary>
        public bool IsEditMode
        {
            get { return isEditMode; }
            set { SetProperty(ref isEditMode, value); }
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

        private string expandedIcon;
        /// <summary>
        /// 展开的图标
        /// </summary>
        public string ExpandedIcon
        {
            get { return expandedIcon; }
            set { SetProperty(ref expandedIcon, value); }
        }

        private ClickMode clickMode;
        /// <summary>
        /// 展开模式
        /// </summary>
        public ClickMode ClickMode
        {
            get { return clickMode; }
            set { SetProperty(ref clickMode, value); }
        }

        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private ObservableCollection<BookmarkTreeViewInfo> children;
        /// <summary>
        /// 子集合
        /// </summary>
        public ObservableCollection<BookmarkTreeViewInfo> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        /// <summary>
        /// 父节点
        /// </summary>
        public BookmarkTreeViewInfo? Parent { get; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public SaveFolderItem SaveFolderItem { get; }

        /// <summary>
        /// 当前文件夹
        /// </summary>
        public FolderInfo CurrentFolder { get; private set; }

        /// <summary>
        /// 当前书签
        /// </summary>
        public BookmarkInfo CurrentBookmark { get; }

        /// <summary>
        /// 节点等级
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// 是否需要保存
        /// </summary>
        public bool IsNeedSave => IsAdd || name != oldName;

        /// <summary>
        /// 是否是新增
        /// </summary>
        public bool IsAdd => Id == Guid.Empty;

        /// <summary>
        /// 主键
        /// </summary>
        public Guid Id => SaveFolderItem.Key;
        #endregion

        private string oldName;

        public BookmarkTreeViewInfo(BookmarkInfo bookmarkInfo, int level, BookmarkTreeViewInfo? parent)
        {
            Parent = parent;
            SaveFolderItem = new SaveFolderItem(bookmarkInfo.Id, bookmarkInfo.Name, bookmarkInfo.Location);
            CurrentFolder = new FolderInfo(Guid.Empty, string.Empty);
            CurrentBookmark = bookmarkInfo;
            Level = level;

            children = new ObservableCollection<BookmarkTreeViewInfo>();
            icon = GetIcon(true, false, false);
            expandedIcon = icon;
            oldName = name = bookmarkInfo.Name;
        }

        public BookmarkTreeViewInfo(FolderInfo folderInfo, int level, BookmarkTreeViewInfo? parent, bool isSpecil)
        {
            Parent = parent;
            SaveFolderItem = new SaveFolderItem(folderInfo.Id, folderInfo.Name);
            CurrentFolder = folderInfo;
            CurrentBookmark = new BookmarkInfo(Guid.Empty, string.Empty, string.Empty);
            Level = level;

            children = new ObservableCollection<BookmarkTreeViewInfo>();
            icon = GetIcon(false, false, isSpecil);
            expandedIcon = GetIcon(false, true, isSpecil);
            oldName = name = folderInfo.Name;
        }

        /// <summary>
        /// 恢复名称
        /// </summary>
        public void RecoverName()
        {
            Name = oldName;
        }

        /// <summary>
        /// 更新文件夹信息
        /// </summary>
        public void UpdateFolder()
        {
            SaveFolderItem.Key = CurrentFolder.Id;
            SaveFolderItem.Display = CurrentFolder.Name;
        }

        public void Delete(BookmarkTreeViewInfo info)
        {
            Children.Remove(info);
        }

        private string GetIcon(bool isBookmark, bool isExpanded, bool isSpecil)
        {
            if (isBookmark) return "📄";
            if (isSpecil) return "⭐";
            return isExpanded ? "📂" : "📁";
        }
    }
}
