using Lotus.Interactions;
using Lotus.Extensions;
using Lotus.Internals;
using Lotus.Internals.Attributes;
/* TO-DO:
Literally everything
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
    public bool EatedItAll = false;
    public bool WEWINTHESE = false;
    public byte YourDefinitelyMurderer = byte.MaxValue;
    public bool WinCondLost = false;
    
    [RoleAction((RoleActionType.RoundStart))]
    {
        public void InnocentHandlerMeetingEnd() 
        {
            if (EvilAceAttorneyTime)
            {
            EvilAceAttorneyTime = false;
            WinCondLost = true;
            }
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