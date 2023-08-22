using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Trackers;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Agent: Impostor
{
    private float arrowUpdateRate;

    private byte trackedPlayer = byte.MaxValue;
    private MeetingPlayerSelector meetingPlayerSelector = new();
    private FixedUpdateLock fixedUpdateLock;

    private string arrowCache = "";

    [UIComponent(UI.Indicator)]
    public string DisplayArrow()
    {
        PlayerControl? agent = Players.FindPlayerById(trackedPlayer);
        if (agent == null) return "";
        if (arrowUpdateRate == 0 || fixedUpdateLock.AcquireLock())
            return arrowCache = $"<size=3>{RoleUtils.CalculateArrow(MyPlayer, agent, RoleColor)}</size>";
        return arrowCache;
    }

    protected override void PostSetup()
    {
        fixedUpdateLock = new FixedUpdateLock(arrowUpdateRate);
    }

    [RoleAction(RoleActionType.MyVote)]
    public void SelectTrackedPlayer(Optional<PlayerControl> player, ActionHandle handle)
    {
        VoteResult result = meetingPlayerSelector.CastVote(player);
        if (result.VoteResultType is not VoteResultType.None) handle.Cancel();
        if (result.VoteResultType is VoteResultType.Confirmed) trackedPlayer = result.Selected;
        if (result.VoteResultType is not VoteResultType.Skipped)
            result.Message().Title(RoleColor.Colorize(RoleName)).Send(MyPlayer);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    public void ResetTrackedPlayer()
    {
        meetingPlayerSelector.Reset();
        Async.Schedule(() => ChatHandler.Of(Translations.AgentMessage, RoleColor.Colorize(RoleName)).Send(MyPlayer), 2f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Arrow Update Rate", Translations.Options.ArrowUpdateRate)
                .Value(v => v.Text(Translations.Options.RealtimeText).Value(0f).Build())
                .AddFloatRange(0.25f, 10, 0.25f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => arrowUpdateRate = f)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(1.0f, 0.0f, 0.0f));

    private static class Translations
    {
        [Localized(nameof(AgentMessage))]
        public static string AgentMessage ="You are an Agent. Select a player each meeting (by voting them twice) to track them. After meeting, you will have an arrow point towards your tracked player.";

        public static class Options
        {
            [Localized(nameof(ArrowUpdateRate))]
            public static string ArrowUpdateRate = "Arrow Update Rate";

            [Localized(nameof(RealtimeText))]
            public static string RealtimeText = "Realtime";
        }
    }

}