using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AchievementViewer;


public class UIFunctions
{
    public static void DrawProgressBar(int progress, int total, bool showPercent, int width, int height, string metric, bool useMetric)
    {
        string percentage = showPercent ? "(" + (progress * 1.0f) / (total * 1.0f) * 100 + "%)" : "";
        string usedMetric = useMetric ? metric : "";
        ImGui.ProgressBar((progress * 1.0f)/(total  * 1.0f), new Vector2(width,height), $"{progress}/{total} {percentage} {usedMetric}");
    }
}
