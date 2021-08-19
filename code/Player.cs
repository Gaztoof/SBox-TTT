using Sandbox;
using System.Collections.Generic;
using System;
using System.Linq;

public class Ammo : NetworkComponent
{

}
public partial class Player : Sandbox.Player
{
	public enum Role
	{
		Spectator,
		Innocent,
		Detective,
		Traitor
	}
	public enum Team
	{
		Terror,
		Spectator,
	}
	public enum DeathType
	{
		Weapon,
		Melee,
		Suicide,
		Fall,
		Burn,
		Explode,
		Vehicle,
	}
	public enum ScoreboardGroup
	{
		NotFound,
		Found,
		Terror,
		Spectator
	}
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net, Predicted] public ICamera VehicleCamera { get; set; }
	[Net, Predicted] public Entity Vehicle { get; set; }
	[Net, Predicted] public ICamera MainCamera { get; set; }
	[Net, Local] public Role role {get;set;}

	[Net, Local] public Role previousRole { get; set; } = Role.Spectator;
	[Net] public Team team { get; set; }
	private TimeSince timeSinceFall;

	//[Net, Predicted] public Dictionary<AmmoType, int> ammos { get; set; } = new();

	[ConVar.ClientData( "ttt_spectator_mode", Help = "Spectator-only mode. True or false", Saved = true )]
	public string spectatorOnlyCVar { get; set; } = "false";
	public bool spectatorOnly { get {
			var owner = GetClientOwner();
			if ( owner == null ) return false;
			string state = owner.GetUserString( "ttt_spectator_mode" );
			return state == null ? false : state.ToLower() == "true"; 
		} }


	[Net] public float karma { get; set; } = 1000f;
	[Net, Local] public bool avoidDetective { get; set; } = false;
	[Net, Local] public float damageFactor { get; set; } = 1f;
	[Net, Local] public bool karmaCleanRound { get; set; } = true;
	[Net, Local] public DeathType deathType { get; set; }
	[Net, Local] public int maxHealth { get; set; } = 100;
	[Net] public bool MissingInAction { get; set; } = false;
	[Net] public int Deaths { get; set; } = 0;
	[Net] public ScoreboardGroup Scoreboardgroup {
		get
		{
			// to everyone, missing players show as alive // except traitors
			// Local.Pawn is not player wtfff?? i had to comment out the line...
			if ( Ragdoll is not null && Ragdoll.IsFound )
				return ScoreboardGroup.Found;
			else if ( Ragdoll is not null && !Ragdoll.IsFound /*Local.Pawn is Player localPlayer && (localPlayer.IsSpectator() || localPlayer.role == Role.Traitor) */)
				return ScoreboardGroup.NotFound;
			else if ( team == Team.Spectator )
				return ScoreboardGroup.Spectator;
			else
				return ScoreboardGroup.Terror;

		} }


	public ICamera LastCamera { get; set; }
	[Net, Local] public bool shouldBeSpec { get; set; } = false;
	public void RemoveRagdoll()
	{
		if ( Ragdoll == null || !Ragdoll.IsValid() )
		{
			return;
		}

		Ragdoll.Delete();
		Ragdoll = null;
	}

	public Player()
	{
		Inventory = new Inventory( this );
		InitAmmo();
	}

	public override void Spawn()
	{
		MainCamera = new FirstPersonCamera();
		LastCamera = MainCamera;

		Karma.InitPlayer( this );
		team = GetNewPlayerTeam();

		base.Spawn();
	}

	public override void Respawn()
	{
		if ( !ShouldRespawnAsSpec())
		{
			SetModel( "models/citizen/citizen.vmdl" );

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Dress();
			LifeState = LifeState.Alive;

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();

			MainCamera = new FirstPersonCamera();
			Camera = MainCamera;

			if ( DevController is NoclipController )
			{
				DevController = null;
			}

			Inventory = new Inventory( this );

			Inventory.Add( new Pistol(), true );
			Inventory.Add( new PumpShotgun() );
			Inventory.Add( new Flashlight() );
			Inventory.Add( new PipeShotgun() );
			Inventory.Add( new Crossbow() );
			Inventory.Add( new SMG() );
			Inventory.Add( new Knife() );
			//Inventory.Add( new LR300() );

			ClearAmmo();
			GiveAmmo( AmmoType.Pistol, 32 );
			GiveAmmo( AmmoType.Shotgun, 32 );
			GiveAmmo( AmmoType.SMG, 32 );

			Radar.Clear( To.Single( this ) );

			MissingInAction = false;
			RemoveRagdoll();
		}
		else
		{
			MakeSpectator( false, spectatorOnly );
		}

		Game.Instance.Round?.OnPlayerSpawn( this );
		base.Respawn();
	}

	public override void OnKilled()
	{
		// server only
		base.OnKilled();
		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;

		BecomeRagdollOnServer( Velocity, GetHitboxBone( lastDamage.HitboxIndex ) );

		LastCamera = MainCamera;
		MainCamera = new SpectateRagdollCamera();
		Camera = MainCamera;
		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		Inventory.DropActive();
		Inventory.DeleteContents();
		ClearAmmo();

		Radar.Clear( To.Single(this));

		if ( shouldBeSpec || !spectatorOnly)
		{
			MissingInAction = true;
			Deaths++;
		}
		
		Game.Instance.Round?.OnPlayerKilled( this );
	}
	[ClientRpc]
	public void OnDeath()
	{
		// make this get called everytime anyone dies, and then if ur spectating deadplayer, update observed player
		if ( !Host.IsClient ) return;


	}
	public override void TakeDamage( DamageInfo info )
	{
		if ( !Host.IsServer ) return;

		if ( Game.Instance.Round is TTTRound round && (round.GetRoundPhase() == TTTRound.RoundPhase.Preparation) ) return;

		info.Damage = ScaleDamage( info.Damage, (HitboxGroup)GetHitboxGroup( info.HitboxIndex ), LastAttackerWeapon );

		if ( lastDamage.Attacker is Player attacker )
		{
			Karma.Hurt( attacker, this, info.Damage );
			info.Damage *= attacker.damageFactor;
		}

		lastDamage = info;


		bool isMelee = (LastAttackerWeapon is Weapon weapon) && weapon.IsMelee;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );
		
		if ( info.Flags.HasFlag( DamageFlags.Fall ) )
			deathType = DeathType.Fall;
		else if ( info.Flags.HasFlag( DamageFlags.Burn ) )
			deathType = DeathType.Burn;
		else if ( info.Flags.HasFlag( DamageFlags.Vehicle ) )
			deathType = DeathType.Vehicle;
		else if ( info.Flags.HasFlag( DamageFlags.Blast ) )
			deathType = DeathType.Explode;
		else if ( info.Flags.HasFlag( DamageFlags.PhysicsImpact ) )
			deathType = DeathType.Fall;
		else if ( isMelee )
			deathType = DeathType.Melee;
		else if ( lastDamage.Attacker == null )
			deathType = DeathType.Suicide;
		else deathType = DeathType.Weapon;

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null ) return VehicleController;
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	public ICamera GetActiveCamera()
	{
		if ( VehicleCamera != null ) return VehicleCamera;

		return MainCamera;
	}

	public override void Simulate( Client cl )
	{
		// called on client and server
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickInspectRagdoll();

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( MainCamera is not FirstPersonCamera )
			{
				MainCamera = new FirstPersonCamera();
			}
			else
			{
				MainCamera = new ThirdPersonCamera();
			}
		}

		Camera = GetActiveCamera();

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();

			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}
		using ( Prediction.Off() )
		{
			float FallSpeed = (Velocity * Rotation.Down).z;
			float FallDamage = (float)Math.Pow( 0.05f * (FallSpeed - 420.0f), 1.75f );
			if ( timeSinceFall > 0.02f && FallSpeed > 450 && IsOnGround() && !controller.HasTag( "noclip" ) && FallDamage > 0 )
			{
				var dmg = new DamageInfo()
				{
					Position = Position,
					Damage = FallDamage,
					Flags = DamageFlags.Fall
				};

				PlaySound( "dm.ui_attacker" );
				TakeDamage( dmg );
				timeSinceFall = 0;
			}
		}

		// noclip if double jump
		/*if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}*/

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public bool IsDead()
	{
		if ( Health <= 0 || LifeState == LifeState.Dead )
		{
			return true;
		}
		return false;
	}
	public bool IsOnGround()
	{
		var tr = Trace.Ray( Position, Position + Vector3.Down * 20 )
				.Radius( 1 )
				.Ignore( this )
				.Run();

		return tr.Hit;
	}

	// TODO

	//public override bool HasPermission( string mode )
	//{
	//	if ( mode == "noclip" ) return true;
	//	if ( mode == "devcam" ) return true;
	//	if ( mode == "suicide" ) return true;
	//
	//	return base.HasPermission( mode );
	//	}
}
