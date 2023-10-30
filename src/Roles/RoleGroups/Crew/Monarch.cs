using Lotus.API;
using Lotus.Roles.Internals.Attributes;
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
    private int maxKnights;
    private int knightCount;
    private bool skippedVote;

    [RoleAction(RoleAction.MyVote)]
    if (result.VoteResultType.Skipped) {
        skippedVote = true;
        return;
    }
    if (!targetSelected && !skippedVote) {
        targetSelected =
        return;
    }
    if (maxKnights == knightCount) return;

}