using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Player
{
	[Net, Local]
	public List<int> ammos { get; set; } = new();

	public virtual void InitAmmo()
	{
		while ( ammos.Count <= Enum.GetNames( typeof( AmmoType ) ).Length )
			ammos.Add( 0 );
	}
	public virtual void ClearAmmo()
	{
		for ( int i = 0; i < ammos.Count; i++ )
			ammos[i] = 0;
	}
	public virtual int AmmoCount( AmmoType type )
	{
		if ( ammos == null ) return 0;
		if ( Game.ConVars.InfiniteAmmos ) return 9999;

		return ammos[(int)type];
	}
	public virtual bool SetAmmo( AmmoType type, int amount )
	{
		if ( !Host.IsServer ) return false;
		if ( ammos == null ) return false;

		ammos[(int)type] = amount;

		return true;
	}

	public virtual bool GiveAmmo( AmmoType type, int amount )
	{
		if ( !Host.IsServer ) return false;
		if ( ammos == null ) return false;
		return SetAmmo( type, AmmoCount( type ) + amount );
	}

	public virtual int TakeAmmo( AmmoType type, int amount )
	{
		if ( ammos == null ) return 0;
		if ( Game.ConVars.InfiniteAmmos ) return amount;

		var available = AmmoCount( type );
		amount = Math.Min( available, amount );

		SetAmmo( type, available - amount );
		return amount;
	}
}
