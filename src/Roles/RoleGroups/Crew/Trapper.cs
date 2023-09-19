using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities.Collections;
using static Lotus.Roles.RoleGroups.Crew.Bastion.BastionTranslations.BastionOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Trapper: Engineer
{
    private int trapsPerGame;
    // Here we can use the vent button as cooldown
    [NewOnSetup] private HashSet<int> trappedVents;

    private float trappedDuration;
    private int currentTraps;
    private Remote<CounterComponent>? counterRemote;

        private byte trappedPlayer = byte.MaxValue;


    protected override void PostSetup()
    {
        if (TrapsPerGame == -1) return;
        CounterHolder counterHolder = MyPlayer.NameModel().GetComponentHolder<CounterHolder>();
        LiveString ls = new(() => RoleUtils.Counter(currentTraps, TrapsPerGame, ModConstants.Palette.GeneralColor2));
        counterRemote = counterHolder.Add(new CounterComponent(ls,new[] { GameState.Roaming }, ViewMode.Additive, MyPlayer));
    }

    [RoleAction(RoleActionType.AnyEnterVent)]
    private void EnterVent(Vent vent, PlayerControl player, ActionHandle handle)
    {
        bool isTrapped = trappedVents.Remove(vent.Id);
        VentLogger.Trace($"Trapped Vent Check: (player={player.name}, isTrapped={isTrapped})", "TrapperAbility");
        if (isTrapped)         trappedPlayer = actor.PlayerId;
        CustomRole actorRole = actor.GetCustomRole();
        Remote<GameOptionOverride> optionOverride = actorRole.AddOverride(new GameOptionOverride(Override.PlayerSpeedMod, 0.01f));
        Async.Schedule(() =>
        {
            optionOverride.Delete();
            actorRole.SyncOptions();
            trappedPlayer = byte.MaxValue;
        }, trappedDuration);
    };
        else if (player.PlayerId == MyPlayer.PlayerId)
        {
            handle.Cancel();
            if (currentTraps == 0) return;
            currentTraps--;
            trappedVents.Add(vent.Id);
        }
    }

    [RoleAction(RoleActionType.Vent)]
    private void ClearCounter() => counterRemote?.Delete();


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Trap cooldown", PlaceTrapCooldown)
                .BindFloat(v => VentCooldown = v)
                .Value(1f)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
                .AddFloatRange(2.5f, 120, 2.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Traps per game", TrapsPerGame)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 20, 1, 0)
                .BindInt(i => TrapsPerGame = i)
                .Build())
            .SubOption(sub => sub
                .KeyName("Trapped Duration", TrappedDuration)
                .Bind(v => trappedDuration = (float)v)
                .AddFloatRange(1, 45, 0.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                .Build());
                

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor("#38761d");

    [Localized(nameof(Trapper))]
    internal static class TrapperTranslations
    {
        [Localized(ModConstants.Options)]
        public static class TrapperOptionTranslations
        {
            [Localized(nameof(PlaceTrapCooldown))]
            public static string PlaceTrapCooldown = "Trap cooldown";

            [Localized(nameof(TrapsPerGame))]
            public static string TrapsPerGame = "Traps per game";
        }
    }
}