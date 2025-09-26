using AchievementViewer.Data;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using System;
using System.Drawing;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Delegates;

namespace AchievementViewer.Windows;

public class CharaCardWindow : Window, IDisposable
{
    private Character lastSeenPlate = new Character(-1, false, false);
    private string lastSeenName = "";
    private string lastSeenServer = "";
    private int offsetY = 5;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public CharaCardWindow()
        : base("", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize 
            | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(0, 0),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        lastSeenPlate = new Character(-1, false, false);
    }

    
    public override void Update()
    {

        var cardData = Service.GameData.GetCardData();

        if (cardData.Count != 2)
        {
            lastSeenPlate = new Character(-1, false, false);
            return;
        }

        var playerName = cardData[0];
        var server = cardData[1];
        lastSeenName = playerName;
        lastSeenServer = server;

        var request = Service.CharData.GetCharData(playerName, server);

        if (request.Id == -1)
        {
            lastSeenPlate = new Character(-1, false, false);
            return;
        }
        
        lastSeenPlate = request;

    }

    public void Dispose() { }

    
    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class
        var alreadyRequested = Service.CharData.IsAchDataRequested((int)lastSeenPlate.Id);


        // can't ref a property, so use a local copy
        if (alreadyRequested || lastSeenPlate.Id == -1)
        {
            ImGui.TextUnformatted("Loading...");
            return;
        } else if (!lastSeenPlate.foundOnLodestone) {
            ImGui.TextUnformatted("Character could not be found on The Lodestone");
            return;
        }
        else if (!lastSeenPlate.foundOnCollect)
        {
            ImGui.TextUnformatted("Character could not be found on FFXIVCollect");
            return;
        }
        else if (!alreadyRequested)
        {
            //tried fixing progressbar width to window size but had some weird resizing stuff going on
            var addon = Service.GameGui.GetAddonByName("CharaCard", 1);
            var width = (int)(addon.ScaledWidth * 0.25);

            if (lastSeenPlate.achievements.Public && Service.Configuration.ShowAchievements)
            {
                ImGui.TextUnformatted("Achievements");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.achievement_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.achievement_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.achievement_Rank.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                    int pointAmount = (Service.Configuration.ShowRanked ? lastSeenPlate.achievements.Ranked_Points : lastSeenPlate.achievements.Points) ?? default(int);
                    int pointTotal = (Service.Configuration.ShowRanked ? lastSeenPlate.achievements.Ranked_Points_Total : lastSeenPlate.achievements.Points_Total) ?? default(int);
                    var percentage = (int)Math.Round((double)(100 * pointAmount) / pointTotal);


                    UIFunctions.DrawProgressBar((float)percentage / 100, width, 20, $"{pointAmount} of {pointTotal} Points ({percentage}%)");
                }
            }

            if (lastSeenPlate.mounts.Public && Service.Configuration.ShowMounts)
            {
                ImGui.TextUnformatted("Mounts");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.mount_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.mount_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.mount_Rank.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                    int count = (Service.Configuration.ShowRanked ? lastSeenPlate.mounts.Ranked_Count : lastSeenPlate.mounts.Count) ?? default(int);
                    int total = (Service.Configuration.ShowRanked ? lastSeenPlate.mounts.Ranked_Total : lastSeenPlate.mounts.Total) ?? default(int);
                    var percentage = (int)Math.Round((double)(100 * count) / total);


                    UIFunctions.DrawProgressBar((float)percentage / 100, width, 20, $"{count}/{total} ({percentage}%)");
                }
            }

            if (lastSeenPlate.minions.Public && Service.Configuration.ShowMinions)
            {
                ImGui.TextUnformatted("Minions");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.minion_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.minion_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.minion_Rank.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                    int count = (Service.Configuration.ShowRanked ? lastSeenPlate.minions.Ranked_Count : lastSeenPlate.minions.Count) ?? default(int);
                    int total = (Service.Configuration.ShowRanked ? lastSeenPlate.minions.Ranked_Total : lastSeenPlate.minions.Total) ?? default(int);
                    var percentage = (int)Math.Round((double)(100 * count) / total);


                    UIFunctions.DrawProgressBar((float)percentage / 100, width - 1, 20, $"{count}/{total} ({percentage}%)");
                }
            }
        }
    }

    public unsafe void UpdatePosition(AddonEvent type, AddonArgs args)
    {
        var addon = Service.GameGui.GetAddonByName("CharaCard", 1);
        if (addon == null)
        {
            return;
        }

        var x = addon.X;
        var y = addon.Y;
        var width = addon.ScaledWidth;
        var height = addon.ScaledHeight;

        this.Position = new Vector2(x, y + height + offsetY);
    }


}

