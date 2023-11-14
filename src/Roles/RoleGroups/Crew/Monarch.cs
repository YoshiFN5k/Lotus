using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.Roles.Internals.Attributes;
using Lotus.Internals.Trackers;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew;

public class Monarch: Crewmate
{
    private bool targetSelected;
    private int maxKnights;
    private int knightCount;
    private bool skippedVote;
    private byte knightTarget = byte.MaxValue;

    protected ChatHandler MonarchAnnouncement(string message) => ChatHandler.Of(message, RoleColor.Colorize{Translations.MessageTitle}).LeftAlign();

    [RoleAction((RoleActionType.RoundStart))]
    [RoleAction((RoleActionType.RoundEnd))]
    {
        public void ReturnFromBrazil() {
            skippedVote = false;
            if (targetSelected)
            {
            MatchData.AssignSubrole(knightTarget, CustomRoleManager.Mods.Knighted)
            targetSelected = false;
            maxKnights--;
            knightCount++;
            MonarchAnnouncement(Translations.)
            }
            knightTarget = byte.MaxValue;
        }
    }

    [RoleAction(RoleAction.MyVote)]
    public void ChooseKnightTarget(Optional<PlayerControl> player, ActionHandle handle)
    {
        if (skippedVote || hasMadeGuess) return;
        handle.Cancel();
        VoteResult result = voteSelector.CastVote(player);
        switch (result.VoteResultType)
        {
            case VoteResultType.None:
                break;
            case VoteResultType.Skipped:
                if (targetSelected) 
                {
                    targetSelected = false;
                    knightTarget = byte.MaxValue;
                } else skippedVote = true;
                break;
            case VoteResultType.Selected:
                knightTarget = result.Selected;
                targetSelected = true;
                break;
            case VoteResultType.Confirmed:
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Localized(nameof(Monarch))]
    {
        public static string MessageTitle = "Game Event";
    }

    [Localized(nameof(MonarchGameEvent))]
    {
        public static string KnightingMessage = "{0} was knighted by the Monarch!"
    }
}