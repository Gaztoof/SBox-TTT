using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerInfo : Panel
{
	public Label timeLeft;

	public Panel jobBar;
	public Label jobLabel;

	public Panel healthBack;
	public Panel healthFill;
	public Label healthLabel;

	public Panel ammoBack;
	public Panel ammoFill;
	public Label ammoLabel;
	private void SetAmmoBarState(bool state)
	{
		ammoBack.SetClass( "active", state );
	}
	private void SetHealthBarState(bool state)
	{
		healthBack.SetClass( "active", state );
	}
	private void SetBackState(bool state)
	{
		SetClass( "active", state );
	}
	private void SetHUDRoundPhase( TTTRound round, bool isSpectating)
	{
		if ( round.GetRoundPhase() == TTTRound.RoundPhase.InGame )
		{
			timeLeft.SetText( $"{round.TimeLeftString}" );
			if( isSpectating )
			SetInProgress( isSpectating );
		}
		else if ( round.GetRoundPhase() == TTTRound.RoundPhase.Preparation )
		{
			timeLeft.SetText( round.PreparationTimeString );
			SetPreparing( isSpectating );
		}
		else if ( round.GetRoundPhase() == TTTRound.RoundPhase.RoundOver )
		{
			timeLeft.SetText( round.RoundOverLeftString );
			SetRoundOver( isSpectating );
		}
	}
	private void SetNoRound(bool isSpectating)
	{
		jobBar.SetClass( "traitor", false );
		jobBar.SetClass( "detective", false );
		jobBar.SetClass( "innocent", false );
		jobBar.SetClass( "noRound", true );
		if ( isSpectating )
		{
			SetHealthBarState( false );
			SetAmmoBarState( false );
		}
	}
	private void SetInProgress(bool isSpectating)
	{
		SetBackState( !isSpectating );
		SetNoRound( isSpectating );
			jobLabel.SetText( "In Progress" );
	}
	private void SetWaiting(bool isSpectating )
	{
		SetBackState( !isSpectating );
		SetNoRound( isSpectating ); 
		jobLabel.SetText( "Waiting" );
		timeLeft.SetText( $"00:00" );
	}
	private void SetRoundOver(bool isSpectating )
	{
		SetBackState( !isSpectating );
		SetNoRound( isSpectating ); 
		jobLabel.SetText( "Round over" );
	}
	private void SetPreparing( bool isSpectating )
	{
		SetBackState( !isSpectating );
		SetNoRound( isSpectating );
		jobLabel.SetText( "Preparing" );
	}
	private void SetDetective()
	{
		jobBar.SetClass( "traitor", false );
		jobBar.SetClass( "detective", true );
		jobBar.SetClass( "innocent", false );
		jobBar.SetClass( "noRound", false );
		jobLabel.SetText( "Detective" );
		SetBackState( true );
	}
	private void SetInnocent()
	{
		jobBar.SetClass( "traitor", false );
		jobBar.SetClass( "detective", false );
		jobBar.SetClass( "innocent", true );
		jobBar.SetClass( "noRound", false );
		jobLabel.SetText( "Innocent" );
		SetBackState( true );
	}
	private void SetTraitor()
	{
		jobBar.SetClass( "traitor", true );
		jobBar.SetClass( "detective", false );
		jobBar.SetClass( "innocent", false );
		jobBar.SetClass( "noRound", false );
		jobLabel.SetText( "Traitor" );
		SetBackState( true );
	}
	public PlayerInfo()
	{
		jobBar = Add.Panel( "JobBar" );
		jobBar.SetClass( "traitor", true );
		jobLabel = jobBar.Add.Label( "", "JobName" );
		timeLeft = Add.Label( "", "TimeLeft" );

		healthBack = Add.Panel( "HealthBarBack" );
		healthFill = healthBack.Add.Panel( "HealthBarFill" );
		healthLabel = healthBack.Add.Label( "", "AmmoHealth" );

		ammoBack = Add.Panel( "AmmoBarBack" );
		ammoFill = ammoBack.Add.Panel( "AmmoBarFill" );
		ammoLabel = ammoBack.Add.Label( "", "AmmoHealth" );
	}

	public override void Tick()
	{
		base.Tick();
		var player = Local.Pawn as Player;
		if ( player == null ) return;

		if ( player.IsSpectator() || player.IsDead() )
		{
			// make it so that here it says in progress or round over even if ded or Preparation
			if ( Game.Instance.Round is TTTRound )
			{
				SetHUDRoundPhase( Game.Instance.Round as TTTRound, player.IsDead() );
				return;
			}
		}
		if ( Game.Instance.Round is LobbyRound )
		{
			SetWaiting(false);
		}

		SetHealthBarState( true );
		SetAmmoBarState( true );
		healthLabel.Text = $"{player.Health.CeilToInt()}";
		healthFill.Style.Width = Length.Percent( player.Health );

		healthFill.Style.Dirty();
		ammoFill.Style.Dirty();

		healthBack.SetClass( "active", true );

		if ( player.ActiveChild != null && player.ActiveChild is Weapon weapon && weapon.ClipSize > 0)
		{
			ammoLabel.Text = $"{weapon.AmmoClip} + {player.AmmoCount(weapon.AmmoType)}";
			ammoFill.Style.Width = Length.Percent( ((float)weapon.AmmoClip / (float)weapon.ClipSize) * 100f);
			ammoFill.Style.Dirty();
			SetAmmoBarState( true );
		}
		else
		{
			SetAmmoBarState( false );
		}

		if ( Game.Instance.Round is not TTTRound round )
		{
			SetWaiting( player.IsDead() );
			return;
		}

		// code above will only run if tttround is on
		if(player.role == Player.Role.Innocent)
		SetInnocent();
		else if ( player.role == Player.Role.Detective )
			SetDetective();
		else if ( player.role == Player.Role.Traitor )
			SetTraitor();

		if ( player.Health > 0 )
		{

		}

		SetHUDRoundPhase( round, false );
	}
}
