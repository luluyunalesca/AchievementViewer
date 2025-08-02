using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;

namespace AchievementViewer;

public class ContextMenu
{
    public static void Enable()
    {
        Service.ContextMenu.OnMenuOpened += OnOpenContextMenu;
    }

    public static void Disable()
    {
        Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
    }

    private static bool IsMenuValid(IMenuArgs menuOpenedArgs)
    {
        if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return false;
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (menuOpenedArgs.AddonName)
        {
            case null: // Nameplate/Model menu
            case "LookingForGroup":
            case "PartyMemberList":
            case "FriendList":
            case "FreeCompany":
            case "SocialList":
            case "ContactList":
            case "ChatLog":
            case "_PartyList":
            case "LinkShell":
            case "CrossWorldLinkshell":
            case "ContentMemberList": // Eureka/Bozja/...
            case "BeginnerChatList":
                return menuTargetDefault.TargetName != string.Empty && Service.GameData.IsServerValid(Convert.ToUInt16(menuTargetDefault.TargetHomeWorld.RowId));

            case "BlackList":
            case "MuteList":
                return menuTargetDefault.TargetName != string.Empty;
        }

        return false;
    }

    private static void SearchPlayerFromMenu(IMenuArgs menuArgs)
    {
        if (menuArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }

        string playerName;
        if (menuArgs.AddonName == "BlackList")
        {
            playerName = GetBlacklistSelectFullName();
        }
        else if (menuArgs.AddonName == "MuteList")
        {
            playerName = GetMuteListSelectFullName();
        }
        else
        {
            ushort serverID = Convert.ToUInt16(menuTargetDefault.TargetHomeWorld.RowId);
            if (!Service.GameData.IsServerValid(serverID))
            {
                return;
            }
            var server = Service.GameData.GetServer(serverID);
            playerName = $"{menuTargetDefault.TargetName}@{server}";
        }

        if (Service.Configuration is { OpenInBrowser: true, ContextMenuStreamer: false })
        {
            //Service.CharData.OpenCharInBrowser(playerName);
        }
        else
        {
            Service.AchievementWindow.Open();
            if (Service.Configuration.AlwaysOpenContextMenuInPartyView
                || (Service.Configuration.OpenContextMenuInPartyView && IsPartyAddon(menuArgs.AddonName)))
            {
                Service.AchievementWindow.IsPartyView = true;
                Service.CharData.UpdatePartyMembers();
                Service.CharData.GetCharData(playerName, Convert.ToUInt16(menuTargetDefault.TargetHomeWorld.RowId));
            }
            else
            {
                Service.CharData.GetCharData(playerName, Convert.ToUInt16(menuTargetDefault.TargetHomeWorld.RowId));
            }
        }
    }

    private static void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
    {
        if (!Service.PluginInterface.UiBuilder.ShouldModifyUi || !IsMenuValid(menuOpenedArgs))
        {
            return;
        }

        if (Service.Configuration.ContextMenuStreamer)
        {
            if (!Service.AchievementWindow.IsOpen)
            {
                return;
            }

            SearchPlayerFromMenu(menuOpenedArgs);
        }
        else
        {
            menuOpenedArgs.AddMenuItem(new MenuItem
            {
                PrefixChar = 'A',
                Name = "View Achievements",
                OnClicked = Search,
            });
        }
    }

    private static void Search(IMenuItemClickedArgs menuItemClickedArgs)
    {
        if (!IsMenuValid(menuItemClickedArgs))
        {
            return;
        }

        SearchPlayerFromMenu(menuItemClickedArgs);
    }

    private static unsafe string GetBlacklistSelectFullName()
    {
        var agentBlackList = AgentBlacklist.Instance();
        if (agentBlackList != null)
        {
            return MemoryHelper.ReadSeString(&agentBlackList->SelectedPlayerFullName).TextValue;
        }

        return string.Empty;
    }

    private static unsafe string GetMuteListSelectFullName()
    {
        var agentMuteList = AgentMutelist.Instance();
        if (agentMuteList != null)
        {
            return MemoryHelper.ReadSeString(&agentMuteList->SelectedPlayerFullName).TextValue;
        }

        return string.Empty;
    }

    private static bool IsPartyAddon(string? menuArgsAddonName)
    {
        return menuArgsAddonName switch
        {
            "PartyMemberList" => true,
            "_PartyList" => true,
            _ => false,
        };
    }
}
