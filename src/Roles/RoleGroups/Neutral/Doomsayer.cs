using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Subroles;
using Lotus.Utilities;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Doomsayer: Guesser
{
    private int doomsayerGuessesToWin;
    private bool doomsayerDiesOnMissguess;

    [UIComponent(UI.Counter, ViewMode.Additive, GameState.Roaming, GameState.InMeeting)]
    public string ShowGuessTotal() => RoleUtils.Counter(CorrectGuesses, doomsayerGuessesToWin, RoleColor);

    [RoleAction(RoleActionType.RoundStart)]
    public void RoundStartCheckWinCondition()
    {
        if (doomsayerGuessesToWin != CorrectGuesses) return;
        ManualWin.Activate(MyPlayer, ReasonType.RoleSpecificWin, 999);
    }

    protected override void HandleBadGuess()
    {
        if (!doomsayerDiesOnMissguess) return;
        base.HandleBadGuess();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Doomsayer Guess Win Amount", TranslationUtil.Colorize(Translations.Options.DoomsayerGuesses, RoleColor))
                .AddIntRange(0, 15, 1, 3)
                .BindInt(i => doomsayerGuessesToWin = i)
                .Build())
            .SubOption(sub => sub.KeyName("Doomsayer Dies on Missguess", TranslationUtil.Colorize(Translations.Options.DoomsayerDiesOnMissGues, RoleColor))
                .BindBool(b => doomsayerDiesOnMissguess = b)
                .AddOnOffValues()
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.29f, 0.87f, 0.72f))
            .Faction(FactionInstances.Neutral)
            .SpecialType(SpecialType.Neutral);


    [Localized(nameof(Doomsayer))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string DoomsayerGuesses = "Doomsayer::0 Guess Win Amount";

            public static string DoomsayerDiesOnMissGues = "Doomsayer::0 Dies on Missguess";
        }
    }
}