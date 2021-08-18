using Sandbox;

public partial class Player
{
	[ServerCmd( "giveammo" )]
	public static void SetInventoryCurrent( string ammotype, int count )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		switch ( ammotype.ToLower() )
		{
			case "pistol":
				target.GiveAmmo( AmmoType.Pistol, count );
				break;
			case "shotgun":
				target.GiveAmmo( AmmoType.Shotgun, count );
				break;
			case "smg":
				target.GiveAmmo( AmmoType.SMG, count );
				break;
		}
	}
	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}
}
