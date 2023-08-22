using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Stats;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Prosecutor.ProsecutorTranslations;
using static Lotus.Roles.RoleGroups.Crew.Prosecutor.ProsecutorTranslations.ProsecutorOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Prosecutor: Crewmate
{
    private static IAccumulativeStatistic<int> _playersEjected = Statistic<int>.CreateAccumulative("Roles.Prosecutor.PlayersEjected", () => PlayersEjectedStat);
    public override List<Statistic> Statistics() => new() { _playersEjected };

    private bool suicideIfVoteCrewmate;
    private int totalProsecutes;

    private int currentProsecutes;
    private bool showProsecutionsVoteAtEnd;

    private GameData.PlayerInfo? prosecutedPlayer;
    private ChatHandler? prosecuteMessage;
    private bool shouldSuicide;

    [UIComponent(UI.Counter, ViewMode.Replace, GameState.InMeeting)]
    private string ProsecuteCounter() => RoleUtils.Counter(currentProsecutes, totalProsecutes, RoleColor);

    protected override void PostSetup() => currentProsecutes = totalProsecutes;

    [RoleAction(RoleActionType.MyVote)]
    private void ProsecutorVote(Optional<PlayerControl> target, MeetingDelegate meetingDelegate)
    {
        if (!target.Exists()) return;
        PlayerControl player = target.Get();
        prosecutedPlayer = player.Data;

        prosecuteMessage = ChatHandler
            .Of(TranslationUtil.Colorize(DictateMessage.Formatted(player.name, RoleName), RoleColor))
            .Title(t => t.Color(RoleColor).Text(RoleName).Build())
            .LeftAlign();

        _playersEjected.Increment(MyPlayer.UniquePlayerId());
        Game.MatchData.GameHistory.AddEvent(new ProsecutorVoteEvent(MyPlayer, player));

        if (--currentProsecutes <= 0) shouldSuicide = false;
        else if (suicideIfVoteCrewmate && Relationship(player) is not Relation.FullAllies)
        {
            shouldSuicide = false;
            return;
        }
        else return;

        FinalizeProsecution();
        meetingDelegate.EndVoting(prosecutedPlayer);
    }

    [RoleAction(RoleActionType.VotingComplete, priority: Priority.High)]
    private void OverrideProsecutedPlayer(MeetingDelegate meetingDelegate)
    {
        if (showProsecutionVoteAtEnd && prosecutedPlayer != null && currentProsecutes > 0)
        {
            meetingDelegate.ExiledPlayer = prosecutedPlayer;
            FinalizeProsecution();
        }
        prosecuteMessage = null;
        prosecutedPlayer = null;
        shouldSuicide = false;
    }

    private void FinalizeProsecution()
    {
        prosecuteMessage?.Send();
        if (!shouldSuicide) return;

        ProtectedRpc.CheckMurder(MyPlayer, MyPlayer);
        Game.MatchData.GameHistory.AddEvent(new SuicideEvent(MyPlayer));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Number of Prosecutions", NumberOfDictates)
                .AddIntRange(1, 15)
                .BindInt(i => totalDictates = i)
                .ShowSubOptionPredicate(i => (int)i > 1)
                .SubOption(sub2 => sub2
                    .KeyName("Show Prosecution at End of Meeting", ShowDictatorVoteAtMeetingEnd)
                    .BindBool(b => showDictatorVoteAtEnd = b)
                    .AddOnOffValues()
                    .Build())
                .Build())
            .SubOption(sub => sub.KeyName("Suicide if Crewmate Prosecuted", TranslationUtil.Colorize(SuicideIfVoteCrewmate, ModConstants.Palette.CrewmateColor))
                .AddOnOffValues(false)
                .BindBool(b => suicideIfVoteCrewmate = b)
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.87f, 0.61f, 0f));

    private class DictatorVoteEvent : KillEvent, IRoleEvent
    {
        public DictatorVoteEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }

        public override string Message() => TranslationUtil.Colorize(prosecuteMessage.Formatted(Player().name, Target().name), Player().GetRoleColor(), Target().GetRoleColor());
    }

    [Localized(nameof(Prosecutor))]
    internal static class ProsecutorTranslations
    {
        [Localized(nameof(LynchEventMessage))]
        public static string LynchEventMessage = "{0}::0 lynched {1}::1.";

        [Localized(nameof(prosecuteMessage))]
        public static string prosecuteMessage = "{0} was voted out by the {1}::0";

        [Localized(nameof(PlayersEjectedStat))]
        public static string PlayersEjectedStat = "Players Ejected";

        [Localized(ModConstants.Options)]
        public static class ProsecutorOptionTranslations
        {
            [Localized(nameof(NumberOfProsecutions))]
            public static string NumberOfProsecutions = "Number of Prosecutions";

            [Localized(nameof(SuicideIfVoteCrewmate))]
            public static string SuicideIfVoteCrewmate = "Suicide if Crewmate::0 Executed";

            [Localized(nameof(ShowProsecutionVoteAtMeetingEnd))]
            public static string ShowProsecutionVoteAtMeetingEnd = "Show Prosecution at End of Meeting";
        }
    }
}
