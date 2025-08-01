
using System.Collections.Generic;

namespace AchievementViewer;

public class CharacterCache
{

    private List<string[]> alreadyRequested = new List<string[]>();
    private List<Character> cachedPlayers = new List<Character>();

    private short maxCacheSize = 10;
    private short maxRequests = 10;

    public CharacterCache()
    {
        
    }

    public bool stillStored(Character c)
    {
        return true;
    }
    
    public int getIndex(string name, short worldID)
    {
        return 0;
    }

    public Character retrieveCharacter()
    {
        return new Character(-1);
    }

    public bool isAlreadyRequested(string name, short worldID)
    {
        return true;
    }

    public void addCharacterToCache(Character c)
    {
        cachedPlayers.Add(c);

    }
}
