using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ExplorerTabUtility.Models
{
    [JsonDerivedType(typeof(SaveFolderInfo), typeDiscriminator: "SaveFolder")]
    [JsonDerivedType(typeof(BookmarkInfo), typeDiscriminator: "Bookmark")]
    [JsonDerivedType(typeof(FolderInfo), typeDiscriminator: "Folder")]
    public abstract class BaseBookmarkInfo
    {
        /// <summary>
        /// 唯一主键
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 名称，书签名称或文件夹名
        /// </summary>
        public string Name { get; set; }

        public BaseBookmarkInfo(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        [JsonConstructor]
        private BaseBookmarkInfo() : this(Guid.Empty, string.Empty)
        {
        }
    }

    public class BookmarkInfo : BaseBookmarkInfo
    {
        public static readonly BookmarkInfo Empty = new BookmarkInfo(Guid.Empty, string.Empty, string.Empty);

        /// <summary>
        /// 父节点Id
        /// </summary>
        [JsonIgnore]
        public Guid ParentId { get; set; }

        /// <summary>
        /// 书签路径
        /// </summary>
        public string Location { get; set; }

        public BookmarkInfo(Guid id, string name, string location) : base(id, name)
        {
            Location = location;
        }

        [JsonConstructor]
        private BookmarkInfo() : this(Guid.Empty, string.Empty, string.Empty)
        {
        }
    }

    public class FolderInfo : BaseBookmarkInfo
    {
        public static readonly FolderInfo Empty = new FolderInfo(Guid.Empty, string.Empty);

        /// <summary>
        /// 子项
        /// </summary>
        public List<BaseBookmarkInfo> Items { get; set; }

        public FolderInfo(Guid id, string name) : base(id, name)
        {
            Items = new List<BaseBookmarkInfo>();
        }

        public FolderInfo(Guid id, string name, FolderInfo folder, FolderInfo otherFolder) : base(id, name)
        {
            Items = [folder, otherFolder];
        }

        [JsonConstructor]
        private FolderInfo() : this(Guid.Empty, string.Empty)
        {
        }

        public void AddRange(IEnumerable<BaseBookmarkInfo> items)
        {
            Items.AddRange(items);
        }

        public void Add(BaseBookmarkInfo item)
        {
            Items.Add(item);
        }

        public void Remove(Guid id)
        {
            var index = Items.FindIndex(t => t.Id == id);
            if (index != -1) Items.RemoveAt(index);
        }

        public bool Search(Guid id, out FolderInfo folder)
        {
            var folders = Items.OfType<FolderInfo>();
            foreach (var item in folders)
            {
                if (item.Id == id)
                {
                    folder = item;
                    return true;
                }

                if (item.Search(id, out folder))
                {
                    return true;
                }
            }

            folder = Empty;
            return false;
        }

        /// <summary>
        /// 获取文件夹节点id
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> GetFolderIds()
        {
            yield return Id;

            foreach (var item in Items)
            {
                if (item is FolderInfo folder)
                {
                    foreach (var id in folder.GetFolderIds())
                    {
                        yield return id;
                    }
                }
            }
        }
    }

    public class SaveFolderInfo : BaseBookmarkInfo
    {
        public SaveFolderInfo(FolderInfo folder) : base(folder.Id, folder.Name)
        {
        }

        [JsonConstructor]
        private SaveFolderInfo() : base(Guid.Empty, string.Empty)
        {
        }
    }
}
