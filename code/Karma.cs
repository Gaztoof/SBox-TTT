using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

// do config stuff?
public static class Karma
{
	private static Dictionary<ulong, float> RememberedPlayers = new Dictionary<ulong, float>();
	private static bool DebugOn = false;

	public static bool IsEnabled()
	{
		return Game.ConVars.KarmaEnabled;
	}
	public static float GetHurtPenalty( float victimKarma, float damage )
	{
		return victimKarma * Math.Clamp( damage * Game.ConVars.KarmaRatio, 0, 1 );
	}
	public static float GetKillPenalty( float victimKarma )
	{
		return GetHurtPenalty( victimKarma, Game.ConVars.KarmaKillPenalty );
	}
	public static float GetHurtReward( float damage )
	{
		return Game.ConVars.KarmaMax * Math.Clamp( damage * Game.ConVars.KarmaTraitorDmgRatio, 0, 1 );
	}
	public static float GetKillReward()
	{
		return GetHurtReward( Game.ConVars.KarmaTraitorKillBonus );
	}
	public static void GivePenalty( Player player, float penalty )
	{
		player.karma = Math.Max( player.karma - penalty, 0 );
	}
	public static void GiveReward( Player player, float reward )
	{
		reward *= DecayedMultiplier( player );
		player.karma = Math.Min( player.karma + reward, Game.ConVars.KarmaMax );
	}
	// ctrl f applykarma to get info about karma
	// ctrl f roundincrement to get info about karma
	// ctrl f damagefactor
	public static void ApplyKarma( Player player )
	{
		float df = 1;

		if ( player.karma < 1000 && Game.ConVars.KarmaEnabled )
		{
			float k = player.karma - 1000;

			if ( Game.ConVars.KarmaStrict )
				df = 1f + (0.0007f * k) + (-0.000002f * (float)Math.Pow( k, 2f ));
			else
				df = 1f + -0.0000025f * (float)Math.Pow( k, 2 );
		}

		player.damageFactor = (float)Math.Clamp( df, 0.1, 1.0 );

		if ( DebugOn && player.GetClientOwner() != null )
			Log.Info( $"{player.GetClientOwner().Name} has karma {player.karma} and gets df {df}" );
	}
	public static void Hurt( Player attacker, Player victim, float damage )
	{
		if ( attacker == victim ) return;
		if ( Game.Instance.Round is not TTTRound || Game.Instance.Round is TTTRound round && round.GetRoundPhase() != TTTRound.RoundPhase.InGame ) return;

		float hurtAmount = Math.Min( victim.Health, damage );

		if ( (attacker.role != Player.Role.Traitor) && (victim.role != Player.Role.Traitor) ) // if they're both not traits
		{
			float penalty = GetHurtPenalty( victim.karma, hurtAmount );
			GivePenalty( attacker, penalty );
			attacker.karmaCleanRound = false;
			if ( DebugOn )
				Log.Info( $"{attacker.GetClientOwner().Name} attacked {victim.GetClientOwner().Name} for {damage} and got penalised for {penalty}" );
		}
		else if ( victim.role == Player.Role.Traitor && attacker.role != Player.Role.Traitor ) // if attacker isnt traitor and victim is traitor
		{
			float reward = GetHurtReward( damage );
			GiveReward( attacker, reward );
			Log.Info( $"{attacker.GetClientOwner().Name} attacked {victim.GetClientOwner().Name} for {damage} and got REWARDED for {reward}" ); ;
		}
	}
	public static void Killed( Player attacker, Player victim )
	{
		if ( attacker == victim ) return;
		if ( Game.Instance.Round is not TTTRound || Game.Instance.Round is TTTRound round && round.GetRoundPhase() != TTTRound.RoundPhase.InGame ) return;
		if ( (attacker.role != Player.Role.Traitor) && (victim.role != Player.Role.Traitor) ) // if they're both not traits
		{
			float penalty = GetKillPenalty( victim.karma );
			GivePenalty( attacker, penalty );
			attacker.karmaCleanRound = false;
			if ( DebugOn )
				Log.Info( $"{attacker.GetClientOwner().Name} killed {victim.GetClientOwner().Name} for {0} and got penalised for {penalty}" );
		}
		else if ( attacker.role != Player.Role.Traitor && victim.role == Player.Role.Traitor ) // if attacker isnt traitor and victim is traitor
		{
			float reward = GetKillReward();
			GiveReward( attacker, reward );
			if ( DebugOn )
				Log.Info( $"{attacker.GetClientOwner().Name} killed {victim.GetClientOwner().Name} for {0} and got REWARDED for {reward}" ); ;
		}
	}
	public static float DecayedMultiplier( Player ply )
	{
		float max = Game.ConVars.KarmaMax;
		float start = Game.ConVars.KarmaStarting;
		float k = ply.karma;
		if ( Game.ConVars.KarmaFallOff <= 0 || k <= start )
			return 1f;
		// i'm lazy to implement fully
		/*if ( k <= max )
		{
			float basediff = max - start;
			float plydiff = k - start;
			float half = Math.Clamp( Game.KarmaFallOff, 0.01f, 0.99f );
			return Math.Pow(a * 1-b, 2)
		}*/
		return 1f;
	}
	public static void RoundIncrement( List<Player> players )
	{
		float healbonus = Game.ConVars.KarmaRoundHeal;
		float cleanbonus = Game.ConVars.KarmaCleanBonus;

		foreach ( var ply in players )
		{
			if ( ply.IsDead() && ply.deathType != Player.DeathType.Suicide )
			{
				float bonus = healbonus + (ply.karmaCleanRound ? cleanbonus : 0f);
				GiveReward( ply, bonus );

				if ( DebugOn )
					Log.Info( $"{ply.GetClientOwner().Name} gets round increase: {bonus}" );
			}
		}
	}
	public static void ApplyKarmaAll( List<Player> players )
	{
		foreach ( var ply in players )
		{
			ApplyKarma( ply );
		}
	}
	public static void RoundEnd( List<Player> players )
	{
		if ( IsEnabled() )
		{
			RoundIncrement( players );

			// rememberall() // basically save

			if ( Game.ConVars.KarmaAutoKick )
			{

			}
		}
	}
	public static void RoundBegin( List<Player> players )
	{
		//initState()
		if ( IsEnabled() )
		{
			ApplyKarmaAll( players );
			// rememberall() // basically save
			if ( Game.ConVars.KarmaAutoKick )
			{

			}
		}
	}
	public static void InitPlayer(Player ply)
	{
		float k = Recall( ply ) == -1f ? Math.Clamp( Game.ConVars.KarmaStarting, 0, Game.ConVars.KarmaMax ) : Recall( ply );

		ply.karmaCleanRound = true;
		ply.damageFactor = 1f;
		ApplyKarma( ply );
	}
	public static float Recall(Player ply)
	{
		if(Game.ConVars.KarmaPersist )
		{
			/*
			 ply.delay_karma_recall = not ply:IsFullyAuthenticated()

			 if ply:IsFullyAuthenticated() then
			    local k = tonumber(ply:GetPData("karma_stored", nil))
			    if k then
			       return k
			    end
			 end
			*/
		}
		if ( ply.GetClientOwner() != null && RememberedPlayers.ContainsKey( ply.GetClientOwner().SteamId ) )
			return RememberedPlayers[ply.GetClientOwner().SteamId];
		else return -1f;
	}
	public static void RememberAll( List<Player> players )
	{
		foreach ( var ply in players )
		{
			Remember( ply );
		}
	}
	public static void CheckAutoKick(Player ply)
	{
		if(ply.karma <= Game.ConVars.KarmaKickLevel && Game.ConVars.KarmaAutoKick )
		{
			Log.Info( ply.GetClientOwner().Name + " was kicked/banned for low karma." );

			/*
			if config.persist:GetBool() then
			    local k = math.Clamp(config.starting:GetFloat() * 0.8, config.kicklevel:GetFloat() * 1.1, config.max:GetFloat())
			    ply:SetPData("karma_stored", k)
			    KARMA.RememberedPlayers[ply:SteamID()] = k
			end
			*/
		}
		if(Game.ConVars.KarmaAutoBan )
		{
			// ban
		}
		else
		{
			// kick
		}
	}
	public static void CheckAutoKickAll( List<Player> players )
	{
		foreach ( var ply in players )
		{
			CheckAutoKick( ply );
		}
	}
	public static void Remember(Player ply)
	{
		RememberedPlayers[ply.GetClientOwner().SteamId] = ply.karma;
	}
}
