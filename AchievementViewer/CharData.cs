using AchievementViewer.Data;
using NetStone;
using NetStone.Search.Character;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace AchievementViewer;

public class CharData
{
    Semaphore idSem = new Semaphore(1, 1);
    Semaphore achDataSem = new Semaphore(1, 1);
    private List<string[]> requestedIDs = new List<string[]>();
    private List<int> requestedAchData = new List<int>();
    private int invalidLodestoneRequests = 0;

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
                
                if (ParseID(lodestoneID) == -1)
                {
                    return;
                }

                string data = await RequestAchievements(lodestoneID);
                Character c = ParseCharacter(lodestoneID, data);

                if (c.Id > 0)
                {
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
        if (data == "") return newId > 0 ? new Character(newId, false, true) : new Character(newId, false, false);
        data = Change2ndOccurence(data, "achievements", "achievement_rank");
        data = Change2ndOccurence(data, "mounts", "mount_rank");
        data = Change2ndOccurence(data, "minions", "minion_rank");
        Character character = JsonConvert.DeserializeObject<Character>(data) ?? new Character(-1,false,false);
        character.foundOnCollect = true;
        character.foundOnLodestone = true;
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

                if (!(searchResponse?.HasResults ?? false))
                {
                    int id = -2 - invalidLodestoneRequests;

                    Service.CharacterCache.AddToMapping(name, server, id);
                    Service.CharacterCache.AddCharacterToCache(new Character(id, false, false));
                    invalidLodestoneRequests++;
                    RemoveIDFromRequested(name, server);
                    return "-1";
                }

                var lodestoneCharacter =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == firstname + " " + surname);
                string lodestoneId = lodestoneCharacter?.Id ?? "-1";

                //If Lodestone id is known
                await lodestoneClient.GetCharacter(lodestoneId);


                Service.CharacterCache.AddToMapping(name, server, lodestoneId);
                //Temporary Addition to stop duplicate requests to Receice lodestone id
                Service.CharacterCache.AddCharacterToCache(new Character(ParseID(lodestoneId), false, true));

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
        return "-1";
    }

    //Request Achievements as a json from ffxivcollect pertaining to the LodestonedID id
    public async Task<string> RequestAchievements(string id)
    {
        int newId = ParseID(id);
        if (newId < 0) return "";
        
        
        AddAchDataToRequested(newId);

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
