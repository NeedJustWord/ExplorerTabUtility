using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Managers
{
    internal static class BookmarkManager
    {
        /// <summary>
        /// 书签集合
        /// </summary>
        public static IReadOnlyList<FolderInfo> Bookmarks { get; }

        /// <summary>
        /// 书签
        /// </summary>
        public static FolderInfo Folder => folderInfo;

        /// <summary>
        /// 其他书签
        /// </summary>
        public static FolderInfo OtherFolder => otherFolderInfo;

        /// <summary>
        /// 溢出书签
        /// </summary>
        public static FolderInfo OverflowFolder => overflowFolderInfo;

        /// <summary>
        /// 上次保存路径
        /// </summary>
        public static IReadOnlyList<SaveFolderInfo> LastSaveFolders
        {
            get
            {
                var list = new List<SaveFolderInfo>(lastSaveFolders.Count + 2);
                for (int i = lastSaveFolders.Count - 1; i >= 0; i--)
                {
                    list.Add(lastSaveFolders[i]);
                }
                list.Add(saveFolderInfo);
                list.Add(otherSaveFolderInfo);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// 最后保存路径Id
        /// </summary>
        public static Guid LastSaveFolderId { get; private set; }

        private static readonly FolderInfo bookmarks;
        private static readonly FolderInfo folderInfo;
        private static readonly FolderInfo otherFolderInfo;
        private static readonly FolderInfo overflowFolderInfo;
        private static readonly List<SaveFolderInfo> lastSaveFolders;
        private static readonly SaveFolderInfo saveFolderInfo;
        private static readonly SaveFolderInfo otherSaveFolderInfo;
        private static readonly BookmarkConfig config;
        private static readonly string configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.AppName,
            Constants.BookmarksFileName);

        static BookmarkManager()
        {
            lastSaveFolders = new List<SaveFolderInfo>(5);
            folderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000001"), "书签栏");
            otherFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000002"), "其他书签");
            overflowFolderInfo = new FolderInfo(Guid.Parse("00000000-0000-0000-0000-000000000003"), ">>");
            bookmarks = new FolderInfo(Guid.Empty, string.Empty, folderInfo, otherFolderInfo);
            Bookmarks = new ReadOnlyCollection<FolderInfo>([folderInfo, otherFolderInfo]);
            saveFolderInfo = new SaveFolderInfo(folderInfo);
            otherSaveFolderInfo = new SaveFolderInfo(otherFolderInfo);
            config = InitBookmarkConfig();

            LoadBookmark();
        }

        private static BookmarkConfig InitBookmarkConfig()
        {
            var directory = Path.GetDirectoryName(configFilePath);
            Directory.CreateDirectory(directory!);

            if (!File.Exists(configFilePath))
            {
                return new BookmarkConfig();
            }

            try
            {
                var json = File.ReadAllText(configFilePath);
                return JsonSerializer.Deserialize<BookmarkConfig>(json) ?? new BookmarkConfig();
            }
            catch (Exception)
            {
                return new BookmarkConfig();
            }
        }

        /// <summary>
        /// 判断是否其他书签或溢出书签
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsOtherOrOverflowFolder(FolderInfo info)
        {
            return info.Id == otherFolderInfo.Id || info.Id == overflowFolderInfo.Id;
        }

        /// <summary>
        /// 恢复配置
        /// </summary>
        public static void RecoverConfig()
        {
            lastSaveFolders.Clear();
            folderInfo.Items.Clear();
            otherFolderInfo.Items.Clear();

            LoadBookmark();
        }

        private static void LoadBookmark()
        {
            var folder = config.Bookmarks.FirstOrDefault(t => t.Id == folderInfo.Id);
            if (folder?.Items.Count > 0)
            {
                folderInfo.AddRange(folder.Items);
            }

            folder = config.Bookmarks.FirstOrDefault(t => t.Id == otherFolderInfo.Id);
            if (folder?.Items.Count > 0)
            {
                otherFolderInfo.AddRange(folder.Items);
            }

            lastSaveFolders.AddRange(config.LastSaveFolders);

            LastSaveFolderId = config.LastSaveFolderId;
        }

        /// <summary>
        /// 保存书签
        /// </summary>
        /// <param name="isEdit">是否是编辑</param>
        /// <param name="lastParentId">上次的父节点id</param>
        /// <param name="currentParentId">这次的父节点id</param>
        /// <param name="bookmark">书签</param>
        /// <param name="newName">新书签名称</param>
        /// <param name="newLocation">新书签路径</param>
        /// <returns></returns>
        public static bool Save(bool isEdit, Guid lastParentId, Guid currentParentId, BookmarkInfo bookmark, string newName, string newLocation)
        {
            if (GetTargetFolderInfoFault(currentParentId, out var currentParentFolder)) return false;

            bookmark.Name = newName;
            bookmark.Location = newLocation;
            if (isEdit)
            {
                if (lastParentId != currentParentId)
                {
                    if (GetTargetFolderInfoFault(lastParentId, out var lastParentFolder) == false)
                    {
                        lastParentFolder.Remove(bookmark.Id);
                    }
                    currentParentFolder.Add(bookmark);
                }
            }
            else
            {
                bookmark.Id = Guid.NewGuid();
                currentParentFolder.Add(bookmark);
            }

            UpdateLastSaveFolders(currentParentFolder);
            SaveConfig();
            return true;
        }

        /// <summary>
        /// 保存文件夹，返回是否成功
        /// </summary>
        /// <param name="parentId">父节点id</param>
        /// <param name="saveFolder">要保存的文件夹</param>
        /// <param name="newName">新文件夹名</param>
        /// <param name="saveConfig">是否保存配置</param>
        /// <returns></returns>
        public static bool Save(Guid parentId, FolderInfo saveFolder, string newName, bool saveConfig)
        {
            if (saveFolder.Id == Guid.Empty)
            {
                if (GetTargetFolderInfoFault(parentId, out var parentFolder))
                {
                    return false;
                }

                saveFolder.Id = Guid.NewGuid();
                saveFolder.Name = newName;
                parentFolder.Add(saveFolder);
            }
            else
            {
                saveFolder.Name = newName;
                UpdateLastSaveFolderName(saveFolder);
            }

            if (saveConfig)
            {
                SaveConfig();
            }

            return true;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="folders"></param>
        public static void Save(List<FolderInfo> folders)
        {
            foreach (var item in folders)
            {
                if (item.Id == folderInfo.Id)
                {
                    folderInfo.Items = item.Items;
                }
                else if (item.Id == otherFolderInfo.Id)
                {
                    otherFolderInfo.Items = item.Items;
                }
            }

            SaveConfig();
        }

        /// <summary>
        /// 删除书签
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public static void Delete(FolderInfo parent, BookmarkInfo current)
        {
            parent.Remove(current.Id);
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        public static void Delete(FolderInfo parent, FolderInfo current)
        {
            if (parent == overflowFolderInfo)
            {
                folderInfo.Remove(current.Id);
            }
            else
            {
                parent.Remove(current.Id);
            }

            var deleteIds = current.GetFolderIds().ToList();
            for (int i = lastSaveFolders.Count - 1; i >= 0; i--)
            {
                if (deleteIds.Contains(lastSaveFolders[i].Id))
                {
                    lastSaveFolders.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 查找目标目录是否失败
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool GetTargetFolderInfoFault(Guid folderId, out FolderInfo folder)
        {
            return bookmarks.Search(folderId, out folder) == false;
        }

        private static void UpdateLastSaveFolderName(FolderInfo folder)
        {
            var temp = lastSaveFolders.FirstOrDefault(t => t.Id == folder.Id);
            if (temp != null)
            {
                temp.Name = folder.Name;
            }
        }

        private static void UpdateLastSaveFolders(FolderInfo folder)
        {
            //TODO:使用循环数组结构
            LastSaveFolderId = folder.Id;

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
        public static void SaveConfig()
        {
            try
            {
                config.Bookmarks = Bookmarks.ToList();
                config.LastSaveFolders = lastSaveFolders;
                config.LastSaveFolderId = LastSaveFolderId;

                var json = JsonSerializer.Serialize(config);
                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }
    }

    internal class BookmarkConfig
    {
        public List<FolderInfo> Bookmarks { get; set; } = new List<FolderInfo>();
        public List<SaveFolderInfo> LastSaveFolders { get; set; } = new List<SaveFolderInfo>();
        public Guid LastSaveFolderId { get; set; }
    }
}
