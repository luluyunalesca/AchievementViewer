using System;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System.Numerics;

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

    public override void Draw() {
        UIFunctions.DrawProgressBar(200, 300, true,  250 , 20 ,"Points",true);
    }
}

    

