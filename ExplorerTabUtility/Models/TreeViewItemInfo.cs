using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExplorerTabUtility.Models
{
    internal class TreeViewItemInfo<TKey> : ComboBoxItemInfo<TKey>
    {
        private ObservableCollection<TreeViewItemInfo<TKey>>? items;

        public ObservableCollection<TreeViewItemInfo<TKey>>? Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public TreeViewItemInfo(TKey key, string display, string? toolTip = null) : base(key, display, toolTip)
        {
        }
    }

    internal class BookmarkTreeViewItem : TreeViewItemInfo<Guid>
    {
        public FolderInfo FolderInfo { get; private set; }

        public BookmarkTreeViewItem(FolderInfo folder) : base(folder.Id, folder.Name)
        {
            FolderInfo = folder;
            var folders = folder.Items.OfType<FolderInfo>().ToList();
            if (folders.Count > 0)
            {
                Items = new ObservableCollection<TreeViewItemInfo<Guid>>(folders.Select(t => new BookmarkTreeViewItem(t)).ToList());
            }
        }

        public void Add(FolderInfo folder)
        {
            var item = new BookmarkTreeViewItem(folder);
            if (Items == null)
            {
                Items = new ObservableCollection<TreeViewItemInfo<Guid>>([item]);
            }
            else
            {
                Items.Add(item);
            }
        }
    }
}
