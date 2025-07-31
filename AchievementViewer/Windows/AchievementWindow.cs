using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using Lumina.Excel.Sheets.Experimental;
using NetStone;
using NetStone.Search;
using NetStone.Search.Character;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AchievementViewer.Windows;

public class AchievementWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;

    private Dictionary<string, string> worlds = new Dictionary<string, string>();

    private string lastSeenName = "";
    private string lastSeenServer = "";

    private short offsetY = 5;

    Player lastSeenPlate = new Player(-1);

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public AchievementWindow(Plugin plugin)
        : base("", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        Configuration = plugin.Configuration;

        parseCSV("World.csv");
    }

    //Parses Server ID to World Mapping from World.csv file into a dictionary
    private void parseCSV(String path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream? stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + path))
        {
            if (stream == null)
            {
                return;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lsplit = line?.Split(',');
                    if (lsplit?.Length > 1)
                    {
                        var newkey = lsplit[0];
                        var newval = lsplit[2];
                        worlds[newkey] = newval;
                    }
                }


            }
        }

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


    }

    public unsafe void UpdatePosition(AddonEvent type, AddonArgs args)
    { 
        var addon = (AtkUnitBase*) Plugin.GameGui.GetAddonByName("CharaCard", 1);
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

    public override async void Update()  
    {
        var cardData = getCardData();
        
        if (cardData.Count == 2)
        {
            
            var playerName = cardData[0];
            var world = cardData[1];
               

            if (playerName != lastSeenName || (playerName == lastSeenName && world != lastSeenServer) )
            {
                if (playerName == "" || world == "")
                {
                    return;
                }
                lastSeenName = playerName;
                lastSeenServer = world;

                //var charId = card->AccountId.ToString();
                Plugin.Log.Debug(playerName);
                var data = await requestData(playerName.Split(" ")[0], playerName.Split(" ")[1], world);
                Player p;
                if (data == "")
                {
                    Plugin.Log.Debug($"Could not find {playerName} on Ffxivcollect");
                    p = new Player(-1);
                }
                else
                {
                    p = parseData(data);
                    Plugin.Log.Debug(p.rankings.achievement_Rank.Server.ToString());
                }
                     

            }
        }
        
        
    }

    private unsafe List<string> getCardData()
    {

        List<string> data = new List<string>();

        if (AgentCharaCard.Instance() == null || AgentCharaCard.Instance()->Data == null)
        {
            return data;
        }

        
        var card = AgentCharaCard.Instance()->Data;
        data.Add(card->Name.ToString());
        data.Add(worlds[card->WorldId.ToString()].Replace("\"", ""));

        return data;
    }

    private async Task<string> requestData(string name, string surname, string server)
    {
        string lodestoneID = "";

        lodestoneID = await requestLodestoneID(name, surname, server);

        var response = await requestAchievements(lodestoneID);

        return response;
    }

    private Player parseData(string data)
    {
        data = change2ndOccurence(data, "achievements", "achievement_rank");
        data = change2ndOccurence(data, "mounts", "mount_rank");
        data = change2ndOccurence(data, "minions", "minion_rank");
        Player player = JsonConvert.DeserializeObject<Player>(data);
        return player;
    }

    private string change2ndOccurence(string s, string oldString, string newString)
    {
        int i = s.IndexOf(oldString, s.IndexOf(oldString) + 1);
        return s.Substring(0, i) + s.Substring(i, s.Length - i).Replace(oldString, newString);
    }

    private async Task<string> requestLodestoneID(string name, string surname, string server)
    {
        try
        {
            var lodestoneClient = await LodestoneClient.GetClientAsync();

            try
            {
                //Get Lodestone Id if not known
                var searchResponse = await lodestoneClient.SearchCharacter(new CharacterSearchQuery()
                {
                    CharacterName = name + " " + surname,
                    World = server
                });
                var lodestoneCharacter =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == name + " " + surname);
                string lodestoneId = lodestoneCharacter.Id;

                //If Lodestone id is known
                await lodestoneClient.GetCharacter(lodestoneId);

                return lodestoneId;
            }
            catch (HttpRequestException e)
            {
                //Handle potential errors in web request

            }
        }
        catch (HttpRequestException ex)
        {

        }
        return null;
    }

    private async Task<string> requestAchievements(string id)
    {

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ffxivcollect.com/api/characters/" + id);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        } else
        {
            return "";
        }
        

    }
}

