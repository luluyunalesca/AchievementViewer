using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AchievementViewer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool ShowAchievements { get; set; } = true;
    public bool ShowMounts { get; set; } = true;
    public bool ShowMinions { get; set; } = true;
    public bool ShowPoints { get; set; } = true;
    public bool ShowRanked { get; set; } = true;
    public bool OpenInBrowser { get; set; } = false;
    public bool ContextMenuStreamer { get; set; } = false;
    public bool OpenContextMenuInPartyView { get; set; } = false;
    public bool AlwaysOpenContextMenuInPartyView { get; set; } = false;



    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
