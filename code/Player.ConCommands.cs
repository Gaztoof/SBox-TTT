using Sandbox;
using System;
using System.Linq;

public partial class Player
{
	[ServerCmd( "giveammo" )]
	public static void GiveAmmo( string ammotype, int count )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;
		foreach(var elem in Enum.GetValues( typeof( AmmoType ) ).Cast<AmmoType>())
		{
			if ( elem.ToString().ToLower() == ammotype )
			{
				target.GiveAmmo( elem, count );
				break;
			}
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
