using Lotus.Interactions;
using Lotus.Extensions;
using Lotus.Internals;
using Lotus.Internals.Attributes;
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
    public bool EvilAceAttorneyTime = false;
    public bool EatedItAll;
    public bool WEWINTHESE = false;
    public byte YourDefinitelyMurderer = byte.MaxValue;
    [RoleAction((RoleActionType.RoundStart, ))]
    {
        public void NahNvm() 
        {
            EvilAceAttorneyTime = false;
        }
    }

    [RoleAction((RoleActionType.MeetingEnd))]
    {
        public void EvilAceAttorney() {
            if (EvilAceAttorneyTime = false) return;
            
        }
    }


    [RoleAction(RoleActionType.Attack)]
    public override void TrySelfPortugal(PlayerControl target)
    {
        target.InteractWIth(MyPlayer, LotusInteraction.FatalInteraction.Create(this));
        EvilAceAttorneyTime = true;
        YourDefinitelyMurderer = target.PlayerId;
        return false;
    
    }
}