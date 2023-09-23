using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Chat;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Marshal.Translations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Marshal: Crewmate
{
    private bool ExtraMarshalMeetings;

    private int additionalVotes;

    private int totalVotes;
    private int remainingVotes;

    private bool revealToVote;
    public bool revealed;

    private FixedUpdateLock updateLock = new(0.25f);

    [UIComponent(UI.Counter)]
    private string PocketCounter() => RoleUtils.Counter(remainingVotes, totalVotes);

    // Removes meeting use counter component if the option is disabled
    protected override void PostSetup()
    {
        remainingVotes = totalVotes;
        if (!ExtraMarshalMeetings) MyPlayer.NameModel().GetComponentHolder<CounterHolder>().RemoveLast();
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void MarshalExtraMeetings()
    {
        if (!updateLock.AcquireLock()) return;
        if (SabotagePatch.CurrentSabotage != null) return;
        if (!ExtraMarshalMeetings || remainingVotes <= 0) return;
        if (revealed = false) return;
        remainingVotes--;
        MyPlayer.CmdReportDeadBody(null);
        MeetingApi.StartMeeting(creator => creator.QuickCall(MyPlayer));
    }

    [RoleAction(RoleActionType.MyVote)]
    private void MarshalVotes(Optional<PlayerControl> voted, MeetingDelegate meetingDelegate, ActionHandle handle)
    {
        if (revealToVote && !revealed)
        {
            if (!voted.Map(p => p.PlayerId == MyPlayer.PlayerId).OrElse(false)) return;
            handle.Cancel();
            revealed = true;
            ChatHandler.Of(MarshalRevealMessage.Formatted(MyPlayer.name)).Title(t => t.Color(RoleColor).Text(MarshalRevealTitle).Build()).Send();
            List<PlayerControl> allPlayers = Game.GetAllPlayers().ToList();
            MyPlayer.NameModel().GetComponentHolder<RoleHolder>().Last().SetViewerSupplier(() => allPlayers);
            return;
        }
        if (!voted.Exists()) return;
        for (int i = 0; i < additionalVotes; i++) meetingDelegate.CastVote(MyPlayer, voted);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void MarshalNotify()
    {
       if (revealToVote && !revealed)
           ChatHandler.Of(RevealMessage).Title(t => t.Color(RoleColor).Text(MarshalRevealTitle).Build()).Send(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Reveal for Votes", Translations.Options.MarshalReveal)
                .AddOnOffValues(false)
                .BindBool(b => revealToVote = b)
                .Build())
            .SubOption(sub => sub.KeyName("Marshal Additional Votes", Translations.Options.MarshalAdditionalVotes)
                .AddIntRange(0, 10, 1, 1)
                .BindInt(i => additionalVotes = i)
                .Build())
            .SubOption(sub => sub.KeyName("Extra Marshal Meetings", Translations.Options.ExtraMarshalMeetings)
                .AddOnOffValues()
                .BindBool(b => ExtraMarshalMeetings = b)
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2.KeyName("Number of Uses", Translations.Options.NumberOfUses)
                    .AddIntRange(1, 20, 1, 2)
                    .BindInt(i => totalVotes = i)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.47f, 0.22f, 0.13f));

    [Localized(nameof(Marshal))]
    internal static class Translations
    {
        [Localized(nameof(RevealMessage))]
        internal static string RevealMessage = "You must reveal yourself to gain additional votes. Currently you can vote normally, but if you vote yourself you'll reveal your role to everyone and gain more votes!";

        [Localized(nameof(MarshalRevealTitle))]
        public static string MarshalRevealTitle = "Marshal Reveal";

        [Localized(nameof(MarshalRevealMessage))]
        public static string MarshalRevealMessage = "{0} revealed themself as the marshal!";

        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(MarshalReveal))]
            public static string Marshal = "Reveal for Votes";

            [Localized(nameof(MarshalAdditionalVotes))]
            public static string MarshalAdditionalVotes = "Marshal Additional Votes";

            [Localized(nameof(ExtraMarshalMeetings))]
            public static string ExtraMarshalMeetings = "Extra Marshal Meetings";

            [Localized(nameof(NumberOfUses))]
            public static string NumberOfUses = "Number of Uses";
        }
    }
}
