namespace ExplorerTabUtility.Models
{
    internal class ComboBoxItemInfo<TKey>(TKey key, string value, string? tooltip = null)
    {
        public TKey Key { get; set; } = key;
        public string Value { get; set; } = value;
        public string? ToolTip { get; set; } = tooltip;
    }

    internal class ComboBoxItemInfo : ComboBoxItemInfo<string>
    {
        public ComboBoxItemInfo(string key, string value, string? tooltip = null) : base(key, value, tooltip)
        {
        }
    }
}
