using AchievementViewer.Data;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Delegates;

namespace AchievementViewer.Windows;

public class CharaCardWindow : Window, IDisposable
{
    private Character lastSeenPlate = new(-1, false, false);
    private string lastSeenName = "";
    private string lastSeenServer = "";
    private readonly int offsetY = 5;

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

            var achievements = lastSeenPlate.achievements;
            var rankings = lastSeenPlate.rankings;
            var mounts = lastSeenPlate.mounts;
            var minions = lastSeenPlate.minions;
            ImGui.TextUnformatted("Achievements");
            if (!(achievements?.Public ?? false)) { 
                ImGui.TextUnformatted("Set to private.");
            } else if (Service.Configuration.ShowAchievements) {

                var achievementrank = rankings?.achievement_Rank;
                ImGui.TextUnformatted($"#{achievementrank?.Server} {lastSeenPlate.Server}  " +
                    $"#{achievementrank?.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{achievementrank?.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                var pointAmount = (Service.Configuration.ShowRanked ? achievements.Ranked_Points : achievements.Points) ?? default;
                var pointTotal = (Service.Configuration.ShowRanked ? achievements.Ranked_Points_Total : achievements.Points_Total) ?? default;
                var percentage = (int)Math.Round((double)(100 * pointAmount) / pointTotal);


                UIFunctions.DrawProgressBar((float)percentage / 100, width, 20, $"{pointAmount} of {pointTotal} Points ({percentage}%)");
                }
            }
        

            ImGui.TextUnformatted("Mounts");
            if (!(mounts?.Public ?? false))
            {
                ImGui.TextUnformatted("Set to private.");
            }
            else if (Service.Configuration.ShowMounts)
            {
                
                var mountrank = rankings?.mount_Rank;
                ImGui.TextUnformatted($"#{mountrank?.Server} {lastSeenPlate.Server}  " +
                    $"#{mountrank?.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{mountrank?.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                    int count = (Service.Configuration.ShowRanked ? mounts.Ranked_Count : mounts.Count) ?? default(int);
                    int total = (Service.Configuration.ShowRanked ? mounts.Ranked_Total : mounts.Total) ?? default(int);
                    var percentage = (int)Math.Round((double)(100 * count) / total);


                    UIFunctions.DrawProgressBar((float)percentage / 100, width, 20, $"{count}/{total} ({percentage}%)");
                }
            }


            ImGui.TextUnformatted("Minions");
            if (!(minions?.Public ?? false))
            {
                ImGui.TextUnformatted("Set to private.");
            }
            else if (Service.Configuration.ShowMinions)
            {
                
                var minionrank = rankings?.minion_Rank;
                ImGui.TextUnformatted($"#{minionrank?.Server} {lastSeenPlate.Server}  " +
                    $"#{minionrank?.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{minionrank?.Global} Global");
                if (Service.Configuration.ShowPoints)
                {
                    int count = (Service.Configuration.ShowRanked ? minions.Ranked_Count : minions.Count) ?? default(int);
                    int total = (Service.Configuration.ShowRanked ? minions.Ranked_Total : minions.Total) ?? default(int);
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
        var height = addon.ScaledHeight;

        this.Position = new Vector2(x, y + height + offsetY);
    }


}

