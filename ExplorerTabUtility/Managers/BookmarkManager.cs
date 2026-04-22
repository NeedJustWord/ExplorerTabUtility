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
        /// 书签
        /// </summary>
        public IReadOnlyList<FolderInfo> Bookmarks { get; }

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
        private readonly List<SaveFolderInfo> lastSaveFolders;

        private BookmarkManager()
        {
            lastSaveFolders = new List<SaveFolderInfo>(5);
            folderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), "书签栏");
            otherFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000002"), "其他书签");
            bookmarks = new FolderInfo(Guid.Empty, string.Empty, folderInfo, otherFolderInfo);
            Bookmarks = new ReadOnlyCollection<FolderInfo>([folderInfo, otherFolderInfo]);

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
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <param name="location"></param>
        public void Save(Guid folderId, string name, string location)
        {
            if (GetTargetFolderInfoFault(folderId, out var targetFolder)) return;

            var info = new BookmarkInfo(Guid.NewGuid(), name, location);
            targetFolder.Add(info);
            UpdateLastSaveFolders(targetFolder);
            SaveBookmarks();
        }

        /// <summary>
        /// 保存文件夹，返回是否成功及新建文件夹信息
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Save(Guid folderId, string name, out FolderInfo info)
        {
            if (GetTargetFolderInfoFault(folderId, out var targetFolder))
            {
                info = FolderInfo.Empty;
                return false;
            }

            info = new FolderInfo(Guid.NewGuid(), name);
            targetFolder.Add(info);
            return true;
        }

        private bool GetTargetFolderInfoFault(Guid folderId, out FolderInfo folder)
        {
            return bookmarks.Search(folderId, out folder) == false;
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

        private void SaveBookmarks()
        {
            try
            {
                var json = JsonSerializer.Serialize(Bookmarks);
                SettingsManager.Bookmarks = json;

                json = JsonSerializer.Serialize(lastSaveFolders);
                SettingsManager.LastSaveFolders = json;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }
    }
}
