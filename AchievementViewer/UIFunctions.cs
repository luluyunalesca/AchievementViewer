using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AchievementViewer;


public class UIFunctions
{
    public static void DrawProgressBar(float percentage, int width, int height, string text)
    {
        ImGui.ProgressBar(percentage, new Vector2(width,height), text);
    }
}
