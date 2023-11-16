using Lotus.Interactions;
using Lotus.Extensions;
/* TO-DO:
Literally everything lmao
rough todo:
read through pb, sheri/guesser, controlling role, exe, and s cat code
rewrite relevant code (current)
cause kill to control suicide instead
code win condition modification
write in the long list of scripts to borrow
~~hate myself~~
*/

namespace Lotus.Roles.RoleGroups.Neutral;

public class Innocent : NeutralKillingBase
{
    public bool EvilAceAttorneyTime;
    public bool EatedItAll;

    [RoleAction(RoleActionType.Attack)]
    public override void TrySelfPortugal(PlayerControl target)
    {
        target.InteractWIth(MyPlayer, LotusInteraction.FatalInteraction.Create(this));
        EvilAceAttorneyTime = true;
        return false;
    
    }
    [RoleAction(RoleActionType.RoundEnd)]
}