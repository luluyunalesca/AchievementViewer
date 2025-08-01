using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
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

namespace AchievementViewer;

public class CharData
{

    public CharData()
    {

    }

    public Character GetCharData(string playerName, short worldID)
    {
        return GetCharData(playerName, Service.GameData.GetWorld(worldID));
    }

    public Character GetCharData(string playerName, string world)
    {
        
        Service.CharacterCache.AddCharacterToRequested(playerName, world);
        var data = RequestCharacter(playerName.Split(" ")[0], playerName.Split(" ")[1], world);
        Service.CharacterCache.RemoveCharacterFromRequested(playerName, world);
        if (data == "")
        {
            Service.Log.Error($"Could not find {playerName} on Ffxivcollect");
            return new Character(-1);
        }
        else
        {
            return ParseCharacter(data);
        }
    }


    private string RequestCharacter(string name, string surname, string server)
    {

        string lodestoneID = "";
        Service.Log.Debug(name + " " + surname + " " + server);

        Task.Run(async () =>
        {
            lodestoneID = await RequestLodestoneID(name, surname, server);
            Service.Log.Debug(lodestoneID);
            return await RequestAchievements(lodestoneID);
        });
        return "";
        Service.Log.Debug("Task didnt run");
    }

    //parse Achievement json file to deduplicate strings and deserialize it
    private Character ParseCharacter(string data)
    {
        data = Change2ndOccurence(data, "achievements", "achievement_rank");
        data = Change2ndOccurence(data, "mounts", "mount_rank");
        data = Change2ndOccurence(data, "minions", "minion_rank");
        Character character = JsonConvert.DeserializeObject<Character>(data);
        return character;
    }


    private string Change2ndOccurence(string s, string oldString, string newString)
    {
        int i = s.IndexOf(oldString, s.IndexOf(oldString) + 1);
        return s.Substring(0, i) + s.Substring(i, s.Length - i).Replace(oldString, newString);
    }

    //Request LodestoneID via Netstone using Players name and server
    private async Task<string> RequestLodestoneID(string name, string surname, string server)
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

    //Request Achievements as a json from ffxivcollect pertaining to the LodestonedID id
    private async Task<string> RequestAchievements(string id)
    {

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ffxivcollect.com/api/characters/" + id);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return "";
        }


    }
}
