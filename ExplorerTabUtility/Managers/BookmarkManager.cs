using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Managers
{
    internal class BookmarkManager
    {
        public static BookmarkManager Instance { get; } = new BookmarkManager();

        /// <summary>
        /// 书签集合
        /// </summary>
        public IReadOnlyList<FolderInfo> Bookmarks { get; }

        /// <summary>
        /// 书签
        /// </summary>
        public FolderInfo Folder => folderInfo;

        /// <summary>
        /// 其他书签
        /// </summary>
        public FolderInfo OtherFolder => otherFolderInfo;

        /// <summary>
        /// 溢出书签
        /// </summary>
        public FolderInfo OverflowFolder => overflowFolderInfo;

        /// <summary>
        /// 上次保存路径
        /// </summary>
        public IReadOnlyList<SaveFolderInfo> LastSaveFolders
        {
            get
            {
                var list = new List<SaveFolderInfo>(lastSaveFolders.Count + 2);
                for (int i = lastSaveFolders.Count - 1; i >= 0; i--)
                {
                    list.Add(lastSaveFolders[i]);
                }
                list.Add(new SaveFolderInfo(folderInfo));
                list.Add(new SaveFolderInfo(otherFolderInfo));
                return list.AsReadOnly();
            }
        }

        private readonly FolderInfo bookmarks;
        private readonly FolderInfo folderInfo;
        private readonly FolderInfo otherFolderInfo;
        private readonly FolderInfo overflowFolderInfo;
        private readonly List<SaveFolderInfo> lastSaveFolders;

        private BookmarkManager()
        {
            lastSaveFolders = new List<SaveFolderInfo>(5);
            folderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), "书签栏");
            otherFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000002"), "其他书签");
            overflowFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000003"), ">>");
            bookmarks = new FolderInfo(Guid.Empty, string.Empty, folderInfo, otherFolderInfo);
            Bookmarks = new ReadOnlyCollection<FolderInfo>([folderInfo, otherFolderInfo]);

            LoadBookmark();
        }

        /// <summary>
        /// 判断是否其他书签或溢出书签
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool IsOtherOrOverflowFolder(FolderInfo info)
        {
            return info.Id == otherFolderInfo.Id || info.Id == overflowFolderInfo.Id;
        }

        /// <summary>
        /// 文件夹重命名
        /// </summary>
        /// <param name="info"></param>
        /// <param name="newName"></param>
        public void FolderRename(FolderInfo info, string newName)
        {
            info.Name = newName;
            SaveConfig();
        }

        /// <summary>
        /// 恢复配置
        /// </summary>
        public void RecoverConfig()
        {
            lastSaveFolders.Clear();
            folderInfo.Items.Clear();
            otherFolderInfo.Items.Clear();

            LoadBookmark();
        }

        private void LoadBookmark()
        {
            try
            {
                var bookmarks = JsonSerializer.Deserialize<List<FolderInfo>>(SettingsManager.Bookmarks);
                if (bookmarks != null)
                {
                    var folder = bookmarks.FirstOrDefault(t => t.Id == folderInfo.Id);
                    if (folder?.Items.Count > 0)
                    {
                        folderInfo.AddRange(folder.Items);
                    }

                    folder = bookmarks.FirstOrDefault(t => t.Id == otherFolderInfo.Id);
                    if (folder?.Items.Count > 0)
                    {
                        otherFolderInfo.AddRange(folder.Items);
                    }
                }

                var lastSaveFolders = JsonSerializer.Deserialize<List<SaveFolderInfo>>(SettingsManager.LastSaveFolders);
                if (lastSaveFolders != null)
                {
                    this.lastSaveFolders.AddRange(lastSaveFolders);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 保存书签
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <param name="location"></param>
        public bool Save(Guid parentId, string name, string location)
        {
            if (GetTargetFolderInfoFault(parentId, out var parentFolder)) return false;

            var info = new BookmarkInfo(Guid.NewGuid(), name, location);
            parentFolder.Add(info);
            UpdateLastSaveFolders(parentFolder);
            SaveConfig();
            return true;
        }

        /// <summary>
        /// 保存文件夹，返回是否成功
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="updateFolder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Save(Guid parentId, FolderInfo updateFolder, string name)
        {
            if (updateFolder.Id == Guid.Empty)
            {
                if (GetTargetFolderInfoFault(parentId, out var parentFolder))
                {
                    return false;
                }

                updateFolder.Id = Guid.NewGuid();
                updateFolder.Name = name;
                parentFolder.Add(updateFolder);
            }
            else
            {
                updateFolder.Name = name;
                UpdateLastSaveFolderName(updateFolder);
            }

            return true;
        }

        /// <summary>
        /// 删除书签
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public void Delete(FolderInfo parent, BookmarkInfo current)
        {
            parent.Remove(current.Id);
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public void Delete(FolderInfo parent, FolderInfo current)
        {
            parent.Remove(current.Id);

            var deleteIds = current.GetFolderIds().ToList();
            for (int i = lastSaveFolders.Count - 1; i >= 0; i--)
            {
                var folder = lastSaveFolders[i];
                if (deleteIds.Contains(folder.Id))
                {
                    lastSaveFolders.RemoveAt(i);
                }
            }
        }

        private bool GetTargetFolderInfoFault(Guid folderId, out FolderInfo folder)
        {
            return bookmarks.Search(folderId, out folder) == false;
        }

        private void UpdateLastSaveFolderName(FolderInfo folder)
        {
            foreach (var item in lastSaveFolders)
            {
                if (item.Id == folder.Id)
                {
                    item.Name = folder.Name;
                    break;
                }
            }
        }

        private void UpdateLastSaveFolders(FolderInfo folder)
        {
            //TODO:使用循环数组结构
            if (folder.Id == folderInfo.Id || folder.Id == otherFolderInfo.Id) return;

            SaveFolderInfo info;
            var index = lastSaveFolders.FindIndex(t => t.Id == folder.Id);
            if (index == -1)
            {
                info = new SaveFolderInfo(folder);
            }
            else
            {
                info = lastSaveFolders[index];
                lastSaveFolders.RemoveAt(index);
            }

            if (lastSaveFolders.Count == 5)
            {
                lastSaveFolders.RemoveAt(0);
            }

            lastSaveFolders.Add(info);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                var bookmarks = JsonSerializer.Serialize(Bookmarks);
                var lastSaveFolders = JsonSerializer.Serialize(this.lastSaveFolders);
                SettingsManager.SetBookmarksAndLastSaveFolders(bookmarks, lastSaveFolders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }
    }
}
