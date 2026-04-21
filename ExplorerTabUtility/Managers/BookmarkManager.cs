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

        private readonly FolderInfo folderInfo;
        private readonly FolderInfo otherFolderInfo;
        private readonly List<SaveFolderInfo> lastSaveFolders;

        private BookmarkManager()
        {
            folderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), "书签栏");
            otherFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000002"), "其他书签");
            lastSaveFolders = new List<SaveFolderInfo>(5);

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

        public void Save(Guid folderId, string name, string location)
        {
            var folder = Bookmarks.FirstOrDefault(t => t.Id == folderId);
            if (folder == null) return;

            var info = new BookmarkInfo(Guid.NewGuid(), name, location);
            folder.Add(info);
            UpdateLastSaveFolders(folder);
            SaveBookmarks();
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
