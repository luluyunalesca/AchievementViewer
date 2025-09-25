using AchievementViewer.Data;
using NetStone;
using NetStone.Search.Character;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using static FFXIVClientStructs.ThisAssembly.Git;

namespace AchievementViewer;

public class CharData
{
    Semaphore idSem = new Semaphore(1, 1);
    Semaphore achDataSem = new Semaphore(1, 1);
    private List<string[]> requestedIDs = new List<string[]>();
    private List<int> requestedAchData = new List<int>();

    public CharData()
    {

    }

    public Character GetCharData(string playerName, ushort serverID)
    {
        return GetCharData(playerName, Service.GameData.GetServer(serverID));
    }

    public Character GetCharData(string playerName, string server)
    {
        RequestCharacter(playerName, server);
        return Service.CharacterCache.GetCharacter(playerName, server);
    }

    public void UpdatePartyMembers()
    {

    }


    public void RequestCharacter(string name, string server)
    {
        Task.Run(async () =>
        {
            bool alreadyStored = Service.CharacterCache.IsAlreadyStored(name, server);

            if (alreadyStored)
            {
                return;
            }




            bool idRequested = IsIDRequested(name, server);
            bool achDataRequested = IsAchDataRequested(Service.CharacterCache.GetID(name, server));

            if (!idRequested && !achDataRequested) { 
                string lodestoneID = await RequestLodestoneID(name, server);
                
                
                

                Service.Log.Debug($"Lodestone ID = {lodestoneID}");

                string data = await RequestAchievements(lodestoneID);
                Character c = ParseCharacter(lodestoneID, data);

                if (c.foundOnCollect) {
                    Service.Log.Debug($"Achievement Data of Char {c.Id} received");
                } else
                {
                    Service.Log.Debug("Char not found on Collect");

                }
                if (c.Id > 0)
                {
                    Service.Log.Debug($"Adding character {c.Id} to cache");
                    Service.CharacterCache.RemoveCharacterFromCache(Service.CharacterCache.GetCharacter(name, server));
                    Service.CharacterCache.AddCharacterToCache(c);
                }
            }
        });
    }

    //parse Achievement json file to deduplicate strings and deserialize it
    private Character ParseCharacter(string id, string data)
    {
        int newId = ParseID(id);
        if (data == "") return new Character(newId, false);
        data = Change2ndOccurence(data, "achievements", "achievement_rank");
        data = Change2ndOccurence(data, "mounts", "mount_rank");
        data = Change2ndOccurence(data, "minions", "minion_rank");
        Character character = JsonConvert.DeserializeObject<Character>(data);
        character.foundOnCollect = true;
        return character;
    }

    //Deduplicate string
    private string Change2ndOccurence(string s, string oldString, string newString)
    {
        if (s.Length < oldString.Length) return s;

        int i = s.IndexOf(oldString, s.IndexOf(oldString) + 1);
        return s.Substring(0, i) + s.Substring(i, s.Length - i).Replace(oldString, newString);
    }

    //Request LodestoneID via Netstone using Players name and server
    public async Task<string> RequestLodestoneID(string name, string server)
    {
        
        AddIDToRequested(name, server);


        Service.Log.Debug("LodestoneID Requested");

        var s = name.Split(" ");
        var firstname = s[0];
        var surname = s[1];

        try
        {
            var lodestoneClient = await LodestoneClient.GetClientAsync();

            try
            {
                //Get Lodestone Id if not known
                var searchResponse = await lodestoneClient.SearchCharacter(new CharacterSearchQuery()
                {
                    CharacterName = firstname + " " + surname,
                    World = server
                });
                var lodestoneCharacter =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == firstname + " " + surname);
                string lodestoneId = lodestoneCharacter.Id;

                //If Lodestone id is known
                await lodestoneClient.GetCharacter(lodestoneId);


                Service.CharacterCache.AddToMapping(name, server, lodestoneId);
                //Temporary Addition to stop duplicate requests to Receice lodestone id
                Service.CharacterCache.AddCharacterToCache(new Character(ParseID(lodestoneId), false));

                RemoveIDFromRequested(name, server);
       
                return lodestoneId;
            }
            catch (HttpRequestException e)
            {
                //Handle potential errors in web request
                Service.Log.Error(e.ToString());
            }
        }
        catch (HttpRequestException ex)
        {
            Service.Log.Error(ex.ToString());
        }

        
        RemoveIDFromRequested(name, server);
        return null;
    }

    //Request Achievements as a json from ffxivcollect pertaining to the LodestonedID id
    public async Task<string> RequestAchievements(string id)
    {
        int newId = ParseID(id);
        if (newId < 0) return "";
        
        
        AddAchDataToRequested(newId);


        Service.Log.Debug($"AchData Requested with ID: {id}");

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ffxivcollect.com/api/characters/" + id);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            
            RemoveAchDataFromRequested(newId);
            
            return result;
        }
        else
        {
            Service.Log.Debug("Achievement Request Failed");
            RemoveAchDataFromRequested(newId);
            
            return "";
        }

        
    }

    public bool IsIDRequested(string name, string server)
    {
        idSem.WaitOne();
        var res = requestedIDs.Exists(x => x[0] == name && x[1] == server);
        idSem.Release();
        return res;

    }

    public void AddIDToRequested(string name, string server)
    {
        if (!IsIDRequested(name, server))
        {
            idSem.WaitOne();
            requestedIDs.Add(new[] { name, server });
            idSem.Release();
        }
    }

    public void RemoveIDFromRequested(string name, string server)
    {
        idSem.WaitOne();
        requestedIDs.Remove(requestedIDs.Find(x => x[0] == name && x[1] == server));
        idSem.Release();
    }

    public bool IsAchDataRequested(int id)
    {
        achDataSem.WaitOne();
        var res = requestedAchData.Exists(x => x == id);
        achDataSem.Release();
        return res;
       
    }

    public void AddAchDataToRequested(int id)
    {

        if (!IsAchDataRequested(id))
        {
            achDataSem.WaitOne();
            requestedAchData.Add(id);
            achDataSem.Release();
        }

    }

    public void RemoveAchDataFromRequested(int id) 
    {
        achDataSem.WaitOne();
        requestedAchData.Remove(requestedAchData.Find(x => x == id));
        achDataSem.Release();
    }

    public int ParseID(string id)
    {
        int newId = -1;
        Int32.TryParse(id, out newId);
        return newId;
    }
}
