namespace Lotus.Roles.Subroles.Knighted;

public class Knighted: Subrole
{
    [RoleAction(RoleActionType.MyVote)]
    private void IncreasedVoting(Optional<PlayerControl> votedFor, MeetingDelegate meetingDelegate)
    {
        if (!votedFor.Exists()) return;
        for (int i = 0; i < ExtraVotes; i++) meetingDelegate.CastVote(MyPlayer, votedFor);
    }
}