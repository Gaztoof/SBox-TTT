using Sandbox;
using System.Collections.Generic;

public partial class Player
{
	public PlayerRagdoll Ragdoll { get; set; }
	public bool BodyFound { get { return (Ragdoll is not null) ? Ragdoll.IsFound : false; } }
#nullable enable
	[Net, Local] public PlayerRagdoll? CurrentlyInspectingRagdoll { get; set; }

	private void BecomeRagdollOnServer( Vector3 force, int forceBone )
	{
		PlayerRagdoll ragdoll = new PlayerRagdoll
		{
			Position = Position,
			Rotation = Rotation
		};

		if ( LastAttackerWeapon is Weapon )
			ragdoll.KillerWeapon = LastAttackerWeapon as Weapon;

		ragdoll.DeathType = deathType;
		ragdoll.Role = role;

		if ( LastAttacker is not null )
		{
			ragdoll.DistanceFromKiller = LastAttacker.Position.Distance( Position );
		}

		ragdoll.Owner = this;
		ragdoll.CopyFrom( this );
		ragdoll.ApplyForceToBone( force, forceBone );
		
		Ragdoll = ragdoll;
	}
	private void TickInspectRagdoll()
	{
		using ( Prediction.Off() )
		{
			PlayerRagdoll? nullableRagdoll = LookAt<PlayerRagdoll?>( 80.0f );

			if ( nullableRagdoll is PlayerRagdoll ragdoll )
			{
				if ( IsServer && Input.Pressed( InputButton.Use ) && LifeState == LifeState.Alive )
				{
					CurrentlyInspectingRagdoll = ragdoll;
				}
			}
		}
	}
}
#nullable disable


public partial class PlayerRagdoll : ModelEntity
{
	public Player Player { get; set; }
	public List<Particles> Ropes = new();
	public List<PhysicsJoint> RopeSprings = new();
	[Net] public Weapon KillerWeapon { get; set; }
	[Net] public bool IsIdentified { get; set; } = false;
	[Net] public bool IsFound { get; set; } = false;
#nullable enable
	[Net] public Player ?Finder { get; set; }
#nullable disable
	[Net] public Player.DeathType DeathType { get; set; } = Player.DeathType.Weapon;
	[Net] public float DistanceFromKiller { get; set; } = 0f;
	[Net] public TimeSince TimeSinceDeath { get; private set; }
	[Net] public Player.Role Role { get; set; }

	public PlayerRagdoll()
	{
		MoveType = MoveType.Physics;
		UsePhysicsCollision = true;

		SetInteractsAs( CollisionLayer.Debris );
		SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		SetInteractsExclude( CollisionLayer.Player );

		TimeSinceDeath = 0;
	}
	public void FindRagdoll( Player finder )
	{
		IsFound = true;
		Finder = finder;
	}

	public void CopyFrom( Player player )
	{
		Player = player;
		Owner = player;

		SetModel( player.GetModelName() );
		TakeDecalsFrom( player );

		this.CopyBonesFrom( player );
		this.SetRagdollVelocityFrom( player );

		foreach ( Entity child in player.Children )
		{
			if ( child is ModelEntity e )
			{
				string model = e.GetModelName();

				if ( model == null || !model.Contains( "clothes" ) )
				{
					continue;
				}

				ModelEntity clothing = new();
				clothing.SetModel( model );
				clothing.SetParent( this, true );
			}
		}
	}

	public void ApplyForceToBone( Vector3 force, int forceBone )
	{
		PhysicsGroup.AddVelocity( force );

		if ( forceBone < 0 )
		{
			return;
		}

		PhysicsBody corpse = GetBonePhysicsBody( forceBone );

		if ( corpse != null )
		{
			corpse.ApplyForce( force * 1000 );
		}
		else
		{
			PhysicsGroup.AddVelocity( force );
		}
	}

	public void ClearAttachments()
	{
		foreach ( Particles rope in Ropes )
		{
			rope.Destroy( true );
		}

		foreach ( PhysicsJoint spring in RopeSprings )
		{
			spring.Remove();
		}

		Ropes.Clear();
		RopeSprings.Clear();
	}

	protected override void OnDestroy()
	{
		ClearAttachments();
	}
}
