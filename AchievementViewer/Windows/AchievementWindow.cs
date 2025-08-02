using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace AchievementViewer.Windows;
public class AchievementWindow : Window, IDisposable
{
    public bool IsPartyView = false;

    public AchievementWindow()
        : base("AchievementViewer", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    { }

    public void Open()
    {
        this.IsOpen = true;
    }

    public void Dispose() { }

    public override void Draw() { }
}

    

