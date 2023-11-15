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
    private bool targetLocked;
    private int maxKnights;
    private int knightCount;
    private bool skippedVote;
    private byte knightTarget = byte.MaxValue;

    protected ChatHandler MonarchMessage(string message) => ChatHandler.Of(message, RoleColor.Colorize{Translations.MessageTitle}).LeftAlign();

    [RoleAction((RoleActionType.RoundStart))]
    [RoleAction((RoleActionType.RoundEnd))]
    {
        public void ReturnFromBrazil() {
            skippedVote = false;
            if (targetLocked)
            {
            MatchData.AssignSubrole(knightTarget, CustomRoleManager.Mods.Knighted)
            targetLocked = false;
            knightCount++;
            MonarchMessage(Translations.MonarchGameEvent.Formatted(Players.FindPlayerById(knightTarget)?.name))
            }
            knightTarget = byte.MaxValue;
        }
    }

    [RoleAction(RoleAction.MyVote)]
    public void ChooseKnightTarget(Optional<PlayerControl> player, ActionHandle handle)
    {
        if (skippedVote || maxKnights = knightCount) return;
        if (targetLocked) return;
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
                    MonarchMessage(Translations.MonarchSkipExplanation.Formatted(Players.FindPlayerById(knightTarget)?.name)).Send(MyPlayer)
                } else 
                {
                    skippedVote = true;
                    MonarchMessage(Translations.MonarchSkipped)
                }
                break;
            case VoteResultType.Selected:
                knightTarget = result.Selected;
                targetSelected = true;
                MonarchMessage(Translations.MonarchAntiIdiot.Formatted(Players.FindPlayerById(knightTarget)?.name)).Send(MyPlayer)
                break;
            case VoteResultType.Confirmed:
                targetLocked = true;
                targetSelected = false;
                MonarchMessage(Translations.KnightConfirmed.Formatted(Players.FindPlayerById(knightTarget)?.name)).Send(MyPlayer)
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Localized(nameof(Monarch))]
    private class Translations
    {
        [Localized(nameof(Monarch))]
        public static string MessageTitle = "Game Event";

        [Localized(nameof(MonarchGameEvent))]
        public static string MonarchGameEvent = "{0} was knighted by the Monarch!";

        [Localized(nameof(MonarchAntiIdiot))]
        public static string MonarchAntiIdiot = "You are about to knight {0} in the next meeting. Vote {0} again to confirm your choice.";

        [Localized(nameof(MonarchSkipExplanation))]
        public static string MonarchSkipExplanation = "You have deselected {0}. Skip again to vote normally, otherwise vote again to choose another player.";
        
        [Localized(nameof(MonarchSkipped))]
        public static string MonarchSkipped = "You hare decided to skip knighting a player in this meeting. You may now vote normally.";
        
        [Localized(nameof(KnightConfirmed))]
        public static string KnightConfirmed = "You have decided to knight {0}. You may now vote normally.";
    }
}