using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace AchievementViewer.Windows;

public class ConfigWindow : Window, IDisposable
{

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public ConfigWindow()
        : base("Achievement Viewer Config", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

    }

    public void Dispose() { }

    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class

        // can't ref a property, so use a local copy
        var showAchievements = Service.Configuration.ShowAchievements;
        if (ImGui.Checkbox("Show Achievements", ref showAchievements))
        {
            Service.Configuration.ShowAchievements = showAchievements;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Service.Configuration.Save();
        }

        var showMounts = Service.Configuration.ShowMounts;
        if (ImGui.Checkbox("Show Mounts", ref showMounts))
        {
            Service.Configuration.ShowMounts = showMounts;
            Service.Configuration.Save();
        }

        var showMinions = Service.Configuration.ShowMinions;
        if (ImGui.Checkbox("Show Minions", ref showMinions))
        {
            Service.Configuration.ShowMinions = showMinions;
            Service.Configuration.Save();
        }
    }
}
