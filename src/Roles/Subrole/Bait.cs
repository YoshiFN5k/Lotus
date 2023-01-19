using TownOfHost.Extensions;
using TownOfHost.GUI;
using UnityEngine;

namespace TownOfHost.Roles;

public class Bait: Subrole
{
    [DynElement(UI.Subrole)]
    private string SubroleIndicator() => RoleColor.Colorize("★");

    [RoleAction(RoleActionType.MyDeath)]
    private void BaitDies(PlayerControl killer) => killer.ReportDeadBody(MyPlayer.Data);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0f, 0.7f, 0.7f));

}