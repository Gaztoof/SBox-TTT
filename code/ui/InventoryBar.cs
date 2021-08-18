using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

public class InventoryBar : Panel
{
	readonly List<InventoryIcon> slots = new();

	public InventoryBar()
	{
		for ( int i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			slots.Add( icon );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;
		for ( int i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( player.Inventory.GetSlot( i ), slots[i], i );
		}
	}
	private bool isOpened = false;
	private void ShowSelf(bool show)
	{
		isOpened = show;
		Style.Display = show ? DisplayMode.Flex : DisplayMode.None;
		Style.Dirty();
	}
	private void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
	{
		if ( ent == null )
		{
			inventoryIcon.Style.Display = DisplayMode.None;
			inventoryIcon.Style.Dirty();
			inventoryIcon.Clear();
			return;
		}
		if( Time.Now >= lastSlotChange +4f)
		{
			if ( ent.IsActiveChild() ) activeSlot = i;
			ShowSelf( false );
		}else ShowSelf( true );

		inventoryIcon.Style.Display = DisplayMode.Flex;
		inventoryIcon.Style.Dirty();
		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = ent.ClassInfo.Title;
		inventoryIcon.SetClass( "active", ent.IsActiveChild() );


		//inventoryIcon.NumberHolder.SetClass( "active", ent.IsActiveChild() );
		inventoryIcon.NumberHolder.SetClass( "active", activeSlot == i );
		inventoryIcon.SetClass( "active", activeSlot == i );

		if ( ent.Parent is Player ply )
		{
			inventoryIcon.NumberHolder.SetClass( "innocent", false );
			inventoryIcon.NumberHolder.SetClass( "traitor", false );
			inventoryIcon.NumberHolder.SetClass( "detective", false );
			if ( ply.role == Player.Role.Innocent || ply.role == Player.Role.Spectator )
				inventoryIcon.NumberHolder.SetClass( "innocent", true );
			else if ( ply.role == Player.Role.Detective )
				inventoryIcon.NumberHolder.SetClass( "detective", true );
			else if ( ply.role == Player.Role.Traitor )
				inventoryIcon.NumberHolder.SetClass( "traitor", true );
		}
	}

	[Event( "buildinput" )]
	public void ProcessClientInput( InputBuilder input )
	{
		var player = Local.Pawn as Player;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( input, inventory, 0 );
		if ( input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( input, inventory, 1 );
		if ( input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( input, inventory, 2 );
		if ( input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( input, inventory, 3 );
		if ( input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( input, inventory, 4 );
		if ( input.Pressed( InputButton.Slot6 ) ) SetActiveSlot( input, inventory, 5 );
		if ( input.Pressed( InputButton.Slot7 ) ) SetActiveSlot( input, inventory, 6 );
		if ( input.Pressed( InputButton.Slot8 ) ) SetActiveSlot( input, inventory, 7 );
		if ( input.Pressed( InputButton.Slot9 ) ) SetActiveSlot( input, inventory, 8 );

		if ( input.Pressed( InputButton.Attack1 ) && isOpened ) SetActiveSlot( input, inventory, activeSlot, true );

		if ( input.MouseWheel != 0 ) SwitchActiveSlot( input, inventory, -input.MouseWheel );
		// if click ShowSelf( false );
	}
	private int activeSlot;
	private float lastSlotChange;
	private void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i, bool real = false )
	{
		// called on cli!!
		lastSlotChange = Time.Now;
		
		var player = Local.Pawn;
		if ( player == null )
			return;

		var ent = inventory.GetSlot( i );
		if ( !real )
			activeSlot = i;
		if ( player.ActiveChild == ent )
		{
			if ( real )
			{
				lastSlotChange = Time.Now - 4f;
			}
			return;
		}

		if ( ent == null )
			return;

		if ( real )
		{
			input.ActiveChild = ent;
			lastSlotChange = Time.Now - 4f;
		}
	}

	private void SwitchActiveSlot( InputBuilder input, IBaseInventory inventory, int idelta )
	{
		var count = inventory.Count();
		if ( count == 0 ) return;

		//var slot = inventory.GetActiveSlot();
		var slot = activeSlot;
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( input, inventory, nextSlot );
	}
}
