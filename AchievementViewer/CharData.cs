using NetStone;
using NetStone.Search.Character;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AchievementViewer.Data;

namespace AchievementViewer;

public class CharData
{
    Semaphore sem = new Semaphore(1, 1);

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


    private void RequestCharacter(string playerName, string server)
    {
        Task.Run(async () =>
        {
            sem.WaitOne();
            bool alreadyRequested = Service.CharacterCache.IsAlreadyRequested(playerName, server);
            sem.Release();
            bool alreadyStored = Service.CharacterCache.IsAlreadyStored(playerName, server);
            if (!alreadyRequested && !alreadyStored)
            {
                sem.WaitOne();
                Service.CharacterCache.AddCharacterToRequested(playerName, server);
                sem.Release();
                string lodestoneID = "";


                lodestoneID = await RequestLodestoneID(playerName.Split(" ")[0], playerName.Split(" ")[1], server);
                string data = await RequestAchievements(lodestoneID);
                
                Character c = ParseCharacter(data);
                if (c.Id > 0)
                {
                    Service.CharacterCache.AddCharacterToCache(c);
                }
                sem.WaitOne();
                Service.CharacterCache.RemoveCharacterFromRequested(playerName, server);
                sem.Release();
            }
        });
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
