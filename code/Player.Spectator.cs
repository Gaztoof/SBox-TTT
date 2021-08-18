using Sandbox;

public partial class Player
{
	public bool IsSpectator()
	{
		return team == Team.Spectator || MainCamera is FreeSpectateCamera;
	}
	public void MakeSpectator( bool useRagdollCamera = false, bool forced = false )
	{
		if ( IsSpectator() ) return;
		EnableAllCollisions = false;
		EnableDrawing = false;
		Controller = null;
		Camera = new FreeSpectateCamera();
		MainCamera = Camera;

		LifeState = LifeState.Dead;
		Health = 0f;
		shouldBeSpec = true;

		//role = Role.Spectator;

		if ( forced )
		{
			MissingInAction = false;
		}
	}
	public Team GetNewPlayerTeam()
	{
		if ( Game.Instance.Round is TTTRound tttRound )
		{
			if ( tttRound.GetRoundPhase() == TTTRound.RoundPhase.Preparation )
				return Team.Terror;
			else
				return Team.Spectator;
		}
		return Team.Terror;
	}
	public bool ShouldRespawnAsSpec()
	{
		//if ( !spectatorOnly && !shouldBeSpec && ((Game.Instance.Round is TTTRound round && round.GetRoundPhase() != TTTRound.RoundPhase.InGame && round.GetRoundPhase() != TTTRound.RoundPhase.RoundOver) || Game.Instance.Round is not TTTRound) )µ
		// if spectatoronly, or shouldbespectator, or (round started already) or round is not gameround
		if ( spectatorOnly || shouldBeSpec || ((Game.Instance.Round is TTTRound round && (round.GetRoundPhase() == TTTRound.RoundPhase.InGame || round.GetRoundPhase() == TTTRound.RoundPhase.RoundOver))) )
			return true;
		if ( Game.Instance.Round is not TTTRound )
			return spectatorOnly;
		return false;
	}
}
