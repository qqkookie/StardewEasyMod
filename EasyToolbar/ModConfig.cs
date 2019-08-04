namespace EasyToolbar
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>Key to shift down toolbar row. Default is "Tab" key.</summary>
        public string ShiftDownKey { get; set; } = "Tab";

        /// <summary>Key to shift up toolbar row.</summary>
        public string ShiftUpKey { get; set; }

        /// <summary>Reset to first slot after shifting toolbar row.</summary>
        public bool ResetOnShift { get; set; } = true;

        /// <summary>Deselect current tool after shifting toolbar row.</summary>
        public bool DeselectOnShift { get; set; }

        /// <summary>When mouse wheel scrolls toolbar slots, scroll into empty slot.</summary>
        public bool ScrollToEmpty { get; set; } = true;

        /// <summary>Key to select appropriate tool for the current tool hit location.
        /// You can hit the key or press and hold it to select tool.</summary>
        public string AutoToolKey { get; set; } = "LeftControl";

        /// <summary>Tool to select to cut "Weeds". One of "Axe, "Scythe"," or "Hoe".</summary>
        public string WeedsTool { get; set; } = "Scythe";

        /// <summary>
        /// Default tool to select when nothing is appropriate. 
        /// Can be tool name like "Scythe" or numbered slot ("1"-"12") 
        /// or "none" to select nothing (empty hand), or "keep" for keeping current selected tool.
        /// </summary>
        public string DefaultTool { get; set; } = "keep";

        /// <summary>Key to deselect current tool. (empty hand) Default is "OemTilde"(~) key.</summary>
        public string DeselectToolKey { get; set; } = "OemTilde";

    }
}
