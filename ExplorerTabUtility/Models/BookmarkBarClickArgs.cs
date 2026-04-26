namespace ExplorerTabUtility.Models
{
    /// <summary>
    /// 书签栏点击事件参数
    /// </summary>
    public class BookmarkBarClickArgs
    {
        /// <summary>
        /// 点击的书签
        /// </summary>
        public BookmarkInfo Bookmark { get; }

        /// <summary>
        /// 书签打开方式
        /// </summary>
        public BookmarkOpenType OpenType { get; }

        public BookmarkBarClickArgs(BookmarkInfo info, BookmarkOpenType type)
        {
            Bookmark = info;
            OpenType = type;
        }
    }
}
