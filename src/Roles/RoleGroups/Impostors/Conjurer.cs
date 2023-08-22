using System;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Trackers;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Rolegroups.Impostors;

public class Conjurer: Impostor

    private MeetingPlayerSelector voteSelector = new();

    private bool skippedVote;
    private int MeteorsThisGame;

    protected string? ConjureMessage;

    [RoleAction(RoleActionType.MyVote)]
    public void SelectPlayerToConjure(Optional<PlayerControl> player, ActionHandle handle)
    {
        if (skippedVote || hasConjured) return;
        handle.Cancel();
        VoteResult result = voteSelector.CastVote(player);
        switch (result.VoteResultType)
        {
            case VoteResultType.None:
                break;
            case VoteResultType.Skipped:
                skippedVote = true;
                break;
            case VoteResultType.Selected:
                ConjuringPlayer = result.Selected;
                ConjureHandler(Translations.PickedPlayerText.Formatted(Players.FindPlayerById(result.Selected)?.name)).Send(MyPlayer);
                break;
            case VoteResultType.Confirmed:
                {
                    voteSelector.Reset();
                    voteSelector.CastVote(player);
                } else hasConjured = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!hasConjured) return;

        if (++MeteorsThisGame < MeteorsPerGame)
        {
            hasConjured = false;
            voteSelector.Reset();
        }

        PlayerControl? conjured = Players.FindPlayerById(ConjuringPlayer);
        if (conjured == null || conjuredplayer == null)
        {
            ConjureHandler(Translations.ErrorConjuring).Send(MyPlayer);
            ResetPreppedPlayer();
            return;
        }

        if (conjuredplayer == conjuredplayer.)
        {
            ConjureMessage = Translations.ConjureAnnouncementMessage.Formatted(conjured.name);
            MyPlayer.InteractWith(conjured, LotusInteraction.FatalInteraction.Create(this));
            Conjured++;
        }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Meteors per game", Translations.Options.MeteorsPerGame)
                .AddIntRange(1, 10, 1, 0)
                .BindInt(i => MeteorsPerGame = i)
                .Build());

    protected ChatHandler ConjureHandler(string message) => ChatHandler.Of(message, RoleColor.Colorize(Translations.ConjurerTitle)).LeftAlign();

    [Localized(nameof(Conjurer))]
    private class Translations
    {
        [Localized(nameof(Conjurer))]
        public static string ConjurerTitle = "Conjurer";

        [Localized(nameof(PickedTargetText))]
        public static string ConfirmTargetText = "You are about to conjure a meteor to target {0}. Vote them again to confirm.";

        [Localized(nameof(PickedPlayerText))]
        public static string PickedTargetText = "You are about to conjure a meteor to target {0}.";

        [Localized(nameof(FinishedConjuringText))]
        public static string FinishedConjuringText = "You have conjured a meteor to kill a player. You can now vote normally.";

        [Localized(nameof(ErrorConjuring))]
        public static string ErrorConjuring = "Error conjuring meteor. You may try again.";

        [Localized(nameof(ConjureAnnouncementMessage))]
        public static string ConjureAnnouncementMessage = "The Conjurer has conjured a meteor to target {0}. {0} has died.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string MeteorsPerGame = "Meteors per Game";
        }
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.VanillaRole(RoleTypes.Crewmate);
}