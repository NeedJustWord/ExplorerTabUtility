using System.Windows;
using System.Windows.Controls;

namespace ExplorerTabUtility.UI.Views.Controls
{
    internal class BookmarkComboBox : ComboBox
    {
        #region 自定义事件
        public static readonly RoutedEvent SelectOtherFolderClickEvent =
            EventManager.RegisterRoutedEvent(
                "SelectOtherFolderClick",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(BookmarkComboBox));

        public event RoutedEventHandler SelectOtherFolderClick
        {
            add { AddHandler(SelectOtherFolderClickEvent, value); }
            remove { RemoveHandler(SelectOtherFolderClickEvent, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var btn = GetTemplateChild("BtnSelectOtherFolder") as Button;
            if (btn != null)
            {
                btn.Click += (sender, e) =>
                {
                    RoutedEventArgs args = new RoutedEventArgs(SelectOtherFolderClickEvent);
                    RaiseEvent(args);
                };
            }
        }
    }
}
