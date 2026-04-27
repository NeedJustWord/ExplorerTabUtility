using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.UI.Commands;

namespace ExplorerTabUtility.Models
{
    public class BookmarkBarInfo : BindableBase
    {
        #region 属性
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                SetProperty(ref name, value);
                DisplayName = GetDisplayName(name);
            }
        }

        private string displayName;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get { return displayName; }
            set { SetProperty(ref displayName, value); }
        }

        private string? nameTooltip;
        /// <summary>
        /// 名称提示
        /// </summary>
        public string? NameTooltip
        {
            get { return nameTooltip; }
            set { SetProperty(ref nameTooltip, value); }
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
        public ICommand? ClickCommand { get; }

        /// <summary>
        /// 菜单点击事件
        /// </summary>
        public ICommand? MenuClickCommand { get; }

        /// <summary>
        /// 当前书签
        /// </summary>
        public BookmarkInfo CurrentBookmark { get; }

        /// <summary>
        /// 当前文件夹
        /// </summary>
        public FolderInfo CurrentFolder { get; }

        /// <summary>
        /// 父节点
        /// </summary>
        public BookmarkBarInfo? Parent { get; private set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 是否显示右键菜单
        /// </summary>
        public bool IsShowContextMenu { get; } = true;
        #endregion

        /// <summary>
        /// 非溢出书签和其他书签文件夹
        /// </summary>
        private bool isNotOverflowAndOtherFolder;
        private static ObservableCollection<BookmarkBarInfo> emptyChildren = new ObservableCollection<BookmarkBarInfo>();

        /// <summary>
        /// 书签
        /// </summary>
        /// <param name="bookmarkInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="clickAction"></param>
        public BookmarkBarInfo(BookmarkInfo bookmarkInfo,
                               int level,
                               BookmarkBarInfo? parent,
                               Action<BookmarkBarInfo, BookmarkInfo> clickAction,
                               Action<BookmarkBarInfo, BookmarkInfo, BookmarkBarAction> menuClickAction)
        {
            isNotOverflowAndOtherFolder = true;
            Parent = parent;
            CurrentBookmark = bookmarkInfo;
            CurrentFolder = FolderInfo.Empty;
            name = bookmarkInfo.Name;
            displayName = GetDisplayName(name);
            icon = "📄";
            Init(false, level);

            children = emptyChildren;
            ClickCommand = new RelayCommand((args) =>
            {
                clickAction.Invoke(this, CurrentBookmark);
            });
            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkBarAction action)
                {
                    menuClickAction.Invoke(this, CurrentBookmark, action);
                }
            });
        }

        /// <summary>
        /// 文件夹
        /// </summary>
        /// <param name="folderInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="bookmarkClickAction"></param>
        /// <param name="isNotOverflowAndOtherFolder">非溢出书签和其他书签</param>
        public BookmarkBarInfo(FolderInfo folderInfo,
                               int level,
                               BookmarkBarInfo? parent,
                               Action<BookmarkBarInfo, BookmarkInfo> bookmarkClickAction,
                               Action<BookmarkBarInfo, BookmarkInfo, BookmarkBarAction> bookmarkMenuClickAction,
                               Action<BookmarkBarInfo, FolderInfo, BookmarkBarAction> folderMenuClickAction,
                               bool isNotOverflowAndOtherFolder)
        {
            IsShowContextMenu = BookmarkManager.Instance.IsOtherOrOverflowFolder(folderInfo) == false;
            this.isNotOverflowAndOtherFolder = isNotOverflowAndOtherFolder;
            Parent = parent;
            CurrentBookmark = BookmarkInfo.Empty;
            CurrentFolder = folderInfo;
            name = folderInfo.Name;
            displayName = GetDisplayName(name);
            icon = "📁";
            Init(true, level);

            children = new ObservableCollection<BookmarkBarInfo>();
            CreateChildren(folderInfo, level, bookmarkClickAction, bookmarkMenuClickAction, folderMenuClickAction);

            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkBarAction action)
                {
                    folderMenuClickAction.Invoke(this, CurrentFolder, action);
                }
            });
        }

        /// <summary>
        /// 溢出文件夹
        /// </summary>
        /// <param name="folderInfo"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        public BookmarkBarInfo(FolderInfo folderInfo, int level, BookmarkBarInfo? parent)
        {
            IsShowContextMenu = false;
            isNotOverflowAndOtherFolder = false;
            Parent = parent;
            CurrentBookmark = BookmarkInfo.Empty;
            CurrentFolder = folderInfo;
            name = folderInfo.Name;
            displayName = GetDisplayName(name);
            icon = "";
            Init(true, level);

            children = new ObservableCollection<BookmarkBarInfo>();
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        /// <summary>
        /// 搜索专用
        /// </summary>
        /// <param name="child"></param>
        public BookmarkBarInfo(IEnumerable<BookmarkBarInfo> child)
        {
            CurrentFolder = FolderInfo.Empty;
            children = new ObservableCollection<BookmarkBarInfo>(child);
        }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

        /// <summary>
        /// 查找指定id父节点
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public BookmarkBarInfo? SearchFolder(Guid folderId)
        {
            if (CurrentFolder.Id == folderId)
            {
                return this;
            }

            foreach (var item in Children)
            {
                if (item.IsFolder)
                {
                    var temp = item.SearchFolder(folderId);
                    if (temp != null)
                    {
                        return temp;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="info"></param>
        public void Add(BookmarkBarInfo info)
        {
            Children.Add(info);
            info.Parent = this;
            //todo:更新属性
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="deleteInfo"></param>
        public void Delete(BookmarkBarInfo deleteInfo)
        {
            Children.Remove(deleteInfo);
            //todo:更新属性
        }

        private void Init(bool isFolder, int level)
        {
            Level = level;
            FirstLevel = level <= 0;
            IsFolder = isFolder;
            PlacementMode = FirstLevel ? PlacementMode.Bottom : PlacementMode.Right;
        }

        private void CreateChildren(FolderInfo folder,
                                    int level,
                                    Action<BookmarkBarInfo, BookmarkInfo> bookmarkClickAction,
                                    Action<BookmarkBarInfo, BookmarkInfo, BookmarkBarAction> bookmarkMenuClickAction,
                                    Action<BookmarkBarInfo, FolderInfo, BookmarkBarAction> folderMenuClickAction)
        {
            if (folder.Items.Count > 0)
            {
                level++;
                foreach (var item in folder.Items)
                {
                    if (item is FolderInfo folderInfo)
                    {
                        children.Add(new BookmarkBarInfo(folderInfo, level, this, bookmarkClickAction, bookmarkMenuClickAction, folderMenuClickAction, true));
                    }
                    else if (item is BookmarkInfo bookmarkInfo)
                    {
                        children.Add(new BookmarkBarInfo(bookmarkInfo, level, this, bookmarkClickAction, bookmarkMenuClickAction));
                    }
                }
            }
        }

        private string GetDisplayName(string name)
        {
            var checkCount = 30;
            var array = GetByteCount(name);
            if (array.Sum() > checkCount)
            {
                NameTooltip = name;
                return $"{name.Substring(0, GetSubLength(array, checkCount))}...";
            }
            else
            {
                NameTooltip = null;
                return name;
            }
        }

        /// <summary>
        /// 获取截断长度
        /// </summary>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int GetSubLength(List<int> list, int count)
        {
            int index;
            for (index = 0; index < list.Count; index++)
            {
                count -= list[index];
                if (count <= 0)
                {
                    break;
                }
            }
            return index + 1;
        }

        /// <summary>
        /// 获取字符数，大于255当汉字两个字符
        /// </summary>
        /// <returns></returns>
        private List<int> GetByteCount(string str)
        {
            return str.Select(c => c > 255 ? 2 : 1).ToList();
        }
    }
}
