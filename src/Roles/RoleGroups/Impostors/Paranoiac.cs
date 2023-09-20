using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Paranoiac : Impostor
{
    [UIComponent(UI.Cooldown)]
    private Cooldown paranoiacCooldown;
    private Cooldown paranoiacDuration;

    private int totalAlerts;
    private int remainingAlerts;
    private bool canKillImpostors;
    private bool canKillWhileTransported;
    private bool canKillRangedAttackers;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        remainingAlerts = totalAlerts;
    }

    [UIComponent(UI.Counter)]
    private string ParanoiacAlertCounter() => RoleUtils.Counter(remainingAlerts, totalAlerts);

    [UIComponent(UI.Indicator)]
    private string GetAlertedString() => paranoiacDuration.IsReady() ? "" : RoleColor.Colorize("♣");

    [RoleAction(RoleActionType.OnPet)]
    public void AssumeAlert()
    {
        if (remainingAlerts <= 0 || paranoiacCooldown.NotReady() || paranoiacDuration.NotReady()) return;
        ParanoiacAlertCounter().DebugLog("Paranoiac Alert Counter: ");
        paranoiacDuration.Start();
        Async.Schedule(() => paranoiacCooldown.Start(), paranoiacDuration.Duration);
        remainingAlerts--;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void ParanoiacInteraction(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (paranoiacDuration.IsReady()) return;

        switch (interaction)
        {
            case Transporter.TransportInteraction when !canKillWhileTransported:
            case IRangedInteraction when !canKillRangedAttackers:
            case IDelayedInteraction:
            case IndirectInteraction:
                return;
        }

        if (actor.GetCustomRole().Faction.Relationship(this.Faction) is Relation.FullAllies && !canKillImpostors) return;
        handle.Cancel();
        Game.MatchData.GameHistory.AddEvent(new VettedEvent(MyPlayer, actor));
        IDeathEvent deathEvent = new CustomDeathEvent(MyPlayer, actor, ModConstants.DeathNames.Parried);
        MyPlayer.InteractWith(actor, new LotusInteraction(new FatalIntent(interaction is not LotusInteraction, () => deathEvent), this));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Color(RoleColor)
            .SubOption(sub => sub.Name("Number of Alerts")
                .Bind(v => totalAlerts = (int)v)
                .AddIntRange(1, 10, 1, 9).Build())
            .SubOption(sub => sub.Name("Alert Cooldown")
                .Bind(v => paranoiacCooldown.Duration = (float)v)
                .AddFloatRange(2.5f, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub.Name("Alert Duration")
                .Bind(v => paranoiacDuration.Duration = (float)v)
                .AddFloatRange(1, 20, 0.25f, 10, GeneralOptionTranslations.SecondsSuffix).Build())
            .SubOption(sub => sub.Name("Kill Impostors")
                .Bind(v => canKillImpostors = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub.Name("Kill While Transported")
                .Bind(v => canKillWhileTransported = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub.Name("Kill Ranged Attackers")
                .BindBool(v => canKillRangedAttackers = v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Impostor)
            .RoleColor(new Color(1.0f, 0.0f, 0.0f));





    private class VettedEvent : KillEvent, IRoleEvent
    {
        public VettedEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }
    }
}
