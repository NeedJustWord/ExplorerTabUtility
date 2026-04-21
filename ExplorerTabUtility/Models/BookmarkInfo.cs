using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExplorerTabUtility.Models
{
    [JsonDerivedType(typeof(SaveFolderInfo), typeDiscriminator: "SaveFolder")]
    [JsonDerivedType(typeof(BookmarkInfo), typeDiscriminator: "Bookmark")]
    [JsonDerivedType(typeof(FolderInfo), typeDiscriminator: "Folder")]
    internal abstract class BaseBookmarkInfo
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

    internal class BookmarkInfo : BaseBookmarkInfo
    {
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

    internal class FolderInfo : BaseBookmarkInfo
    {
        /// <summary>
        /// 子项
        /// </summary>
        public List<BaseBookmarkInfo> Items { get; set; }

        public FolderInfo(Guid id, string name) : base(id, name)
        {
            Items = new List<BaseBookmarkInfo>();
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
    }

    internal class SaveFolderInfo : BaseBookmarkInfo
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
