using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;
using AchievementViewer.Data;

namespace AchievementViewer.Windows;

public class CharaCardWindow : Window, IDisposable
{
    private Character lastSeenPlate = new Character(-1);
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

        lastSeenPlate = new Character(-1);
    }

    

    public override void Update()
    {
        var cardData = Service.GameData.GetCardData();

        if (cardData.Count != 2)
        {
            return;
        }

        var playerName = cardData[0];
        var world = cardData[1];

        lastSeenName = playerName;
        lastSeenServer = world;

        var request = Service.CharData.GetCharData(playerName, world);
        
        if (request.Id == -1)
        {
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

        var alreadyRequested = Service.CharacterCache.IsAlreadyRequested(lastSeenName, lastSeenServer);

        // can't ref a property, so use a local copy
        if (alreadyRequested)
        {
            ImGui.TextUnformatted("Loading...");
            return;
        } 
        else if (lastSeenPlate.Id == -1 && !alreadyRequested)
        {
            ImGui.TextUnformatted("Character could not be found on FFXIVCollect");
            return;
        } 
        else if (!alreadyRequested)
        {
            
            if (lastSeenPlate.achievements.Public)
            {
                ImGui.TextUnformatted("Achievements");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.achievement_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.achievement_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.achievement_Rank.Global} Global");

            }

            if (lastSeenPlate.mounts.Public)
            {
                ImGui.TextUnformatted("Mounts");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.mount_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.mount_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.mount_Rank.Global} Global");
            }

            if (lastSeenPlate.minions.Public)
            {
                ImGui.TextUnformatted("Minions");
                ImGui.TextUnformatted($"#{lastSeenPlate.rankings.minion_Rank.Server} {lastSeenPlate.Server}  " +
                    $"#{lastSeenPlate.rankings.minion_Rank.Data_Center} {lastSeenPlate.Data_Center}  " +
                    $"#{lastSeenPlate.rankings.minion_Rank.Global} Global");
            }
        }
    }

    public unsafe void UpdatePosition(AddonEvent type, AddonArgs args)
    { 
        var addon = (AtkUnitBase*) Service.GameGui.GetAddonByName("CharaCard", 1);
        if(addon == null)
        {
            return;
        }

        var x = addon->X;
        var y = addon->Y;
        var width = addon->RootNode->Width;
        var height = addon->RootNode->Height;
        
        Position = new Vector2(x, y + height + offsetY);
    }

    
}

