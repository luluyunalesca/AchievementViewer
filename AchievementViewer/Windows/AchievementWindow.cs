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

    public AchievementWindow()
        : base("AchievementViewer", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    { }
    public void Dispose() { }

    public override void Draw() { }
}

    

