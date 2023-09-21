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
using Lotus.Roles.Interfaces;
using Lotus.Roles.Overrides;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Wraith : Impostor
{
    private Cooldown dashCooldown;
    private Cooldown dashDuration;

    private float dashSpeed = 1f;
    private Remote<GameOptionOverride>? overrideRemote;

    [RoleAction(RoleActionType.OnPet)]
    public void AssumeDash()
    {
        if (dashCooldown.NotReady() || dashDuration.NotReady()) return;
        dashDuration.Start();
        Async.Schedule(() => dashCooldown.Start(), dashDuration.Duration);
        AdditiveOverride additiveOverride = new(Override.PlayerSpeedMod, playerSpeedIncrease);
        overrideRemote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, additiveOverride);
        if (dashDuration = 0) return;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Color(RoleColor)
            .SubOption(sub => sub
                .KeyName("Dash Movement Speed", Translations.Options.dashSpeed)
                .AddFloatRange(0.25f, 2.5f, 0.25f, 3)
                .BindFloat(f => dashSpeed = f)
                .Build())
            .SubOption(sub => sub.Name("Dash Cooldown")
                .Bind(v => dashCooldown.Duration = (float)v)
                .AddFloatRange(2.5f, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub.Name("Dash Duration")
                .Bind(v => dashDuration.Duration = (float)v)
                .AddFloatRange(1, 20, 0.25f, 10, GeneralOptionTranslations.SecondsSuffix).Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Impostor)
            .RoleColor(new Color(1.0f, 0.0f, 0.0f));
}
