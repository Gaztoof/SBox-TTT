using Sandbox;

[Library( "pump", Title = "Pump Shotgun", Spawnable = true )]
public partial class PumpShotgun : Weapon
{
	public override float Damage => 120;
	public override bool IsAutomatic => false;
	public override int RPM => 100;
	public override float ReloadTime => 0.75f;
	public override int BulletsPerShot => 20;
	public override float Spread => 0.2f;
	public override int ClipSize => 6;
	public override string ShootShound => "rust_pumpshotgun.shootdouble";
	public override string WorldModelPath => "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl";
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override string Brass => null;
	public override bool ReloadMagazine => false;
	public override CrosshairType CrosshairType => CrosshairType.Sides;
	public override HoldType HoldType => HoldType.Shotgun;
	public override AmmoType AmmoType => AmmoType.Shotgun;
	public override bool CanAim => true;
	public override float HeadshotMultiplier => 1.75f;
	public override string Name => "Pump Shotgun";
	public override string Dryfire => "rust_pumpshotgun.dryfire";
	public virtual string ReloadStart => "rust_pumpshotgun.reloadstart";
	public virtual string AddShell => "rust_pumpshotgun.insert";
	public virtual string ReloadFinish => "rust_pumpshotgun.reloadfinish";
	public override string PumpSound => "rust_pumpshotgun.pump";
	public override string DeploySound => "rust_pumpshotgun.deploy";


	[ClientRpc]
	public async override void ShootEffects()
	{
		base.ShootEffects();
		using ( Prediction.Off() )
		{
			await Task.Delay( 400 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( PumpSound );
		}
	}

	[ClientRpc]
	public async override void StartReloadEffects()
	{
		base.StartReloadEffects();
		using ( Prediction.Off() )
		{
			await Task.Delay( 300 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ReloadStart );
		}
	}
	[ClientRpc]
	public async override void FinishReloadEffects()
	{
		base.FinishReloadEffects();
		using ( Prediction.Off() )
		{
			await Task.Delay( 300 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ReloadFinish );
		}
	}
	public override int ReloadDelay => 300;
	[ClientRpc]
	public override void StartAmmoReloadEffects()
	{
		using ( Prediction.Off() )
		{
			PlaySound( AddShell );
		}
	}

}
