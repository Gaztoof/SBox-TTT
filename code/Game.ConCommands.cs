using Sandbox;
using System.Threading.Tasks;
using System.Linq;
partial class Game
{
	[ServerCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		var owner = ConsoleSystem.Caller?.Pawn;

		if ( ConsoleSystem.Caller == null )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var ent = new Prop();
		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
		ent.SetModel( modelname );
		ent.Position = tr.EndPos - Vector3.Up * ent.CollisionBounds.Mins.z;
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var owner = ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) );
	}
	[ServerCmd( "ttt_roundrestart" )]
	public static void RoundRestart( )
	{
		var players = Instance.Round.Players;
		Instance.Round = new LobbyRound();
		foreach(var ply in players)
		{
			Instance.Round.OnPlayerJoin( ply );
		}	
	}
	[ServerCmd( "ttt_force_terror" )]
	public static void ForceTerror( )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player local ) return;
		local.role = Player.Role.Innocent;
	}
	[ServerCmd( "ttt_force_traitor" )]
	public static void ForceTraitor( )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player local ) return;
		local.role = Player.Role.Traitor;
	}
	[ServerCmd( "ttt_force_detective" )]
	public static void ForceDetective( )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player local ) return;
		local.role = Player.Role.Detective;
	}

}
