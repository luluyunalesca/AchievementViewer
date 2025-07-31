using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace AchievementViewer.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public ConfigWindow(Plugin plugin)
        : base("Achievement Viewer", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class

        // can't ref a property, so use a local copy
        var showAchievements = Configuration.ShowAchievements;
        if (ImGui.Checkbox("Show Achievements", ref showAchievements))
        {
            Configuration.ShowAchievements = showAchievements;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var showMounts = Configuration.ShowMounts;
        if (ImGui.Checkbox("Show Mounts", ref showMounts))
        {
            Configuration.ShowMounts = showMounts;
            Configuration.Save();
        }

        var showMinions = Configuration.ShowMinions;
        if (ImGui.Checkbox("Show Minions", ref showMinions))
        {
            Configuration.ShowMinions = showMinions;
            Configuration.Save();
        }

        var showLogs = Configuration.ShowLogs;
        if (ImGui.Checkbox("Show fflogs", ref showLogs))
        {
            Configuration.ShowLogs = showLogs;
            Configuration.Save();
        }
    }
}
