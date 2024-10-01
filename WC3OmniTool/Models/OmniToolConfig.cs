namespace WC3OmniTool.Models
{
    public class OmniToolConfig(string Icon, string ButtonText, string MenuText, string ToolTip, string Executable, bool? CreatedByOmniTool)
    {
        public string Icon { get; set; } = Icon;
        public string ButtonText { get; set; } = ButtonText;
        public string MenuText { get; set; } = MenuText;
        public string ToolTip { get; set; } = ToolTip;
        public string Executable { get; set; } = Executable;
        public bool? CreatedByOmniTool { get; set; } = CreatedByOmniTool;
    }
}
