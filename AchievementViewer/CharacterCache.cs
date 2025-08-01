
using System.Collections.Generic;

namespace AchievementViewer;

public class CharacterCache
{

    private List<string[]> alreadyRequested = new List<string[]>();
    private List<Character> cachedPlayers = new List<Character>();

    private short maxCacheSize = 24;
    private short maxRequests = 24;

    public CharacterCache()
    {

    }

    public bool StillStored(Character c)
    {
        return true;
    }

    public int GetIndex(string name, short worldID)
    {
        return 0;
    }

    public Character RetrieveCharacter()
    {
        return new Character(-1);
    }

    public bool IsAlreadyRequested(string name, string world)
    {
        return alreadyRequested.Contains(new[] {name, world});
    }

    public void AddCharacterToCache(Character c) { cachedPlayers.Add(c); }

    public void RemoveCharacterFromCache(Character c) { cachedPlayers.Remove(c); }

    public void AddCharacterToRequested(string name, short id)
    {
        alreadyRequested.Add(new[] {name, id.ToString()});
        
    }

    public void AddCharacterToRequested(string name, string id)
    {
        alreadyRequested.Add(new[] { name, id });

    }

    public void RemoveCharacterFromRequested(string name, short id) 
    { 
        alreadyRequested.Remove(new[] {name, id.ToString()});
    }

    public void RemoveCharacterFromRequested(string name, string id)
    {
        alreadyRequested.Remove(new[] { name, id });
    }
}

