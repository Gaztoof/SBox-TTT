using Sandbox;

[Library( "pipe", Title = "Pipe Shotgun", Spawnable = true )]
public partial class PipeShotgun : Weapon
{
	public override float Damage => 120;
	public override bool IsAutomatic => false;
	public override int ClipSize => 1;
	public override int RPM => 40;
	public override float ReloadTime => 4f;
	public override float Spread => 0.1f;
	public override int BulletsPerShot => 20;
	public override string ShootShound => "rust_shotgun.shoot";
	public override string WorldModelPath => "weapons/rust_shotgun/rust_shotgun.vmdl";
	public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";
	public override CrosshairType CrosshairType => CrosshairType.Dot;
	public override string Brass => null;
	public override HoldType HoldType => HoldType.Shotgun;
	public override AmmoType AmmoType => AmmoType.Shotgun;
	public override bool CanAim => true;
	public override float HeadshotMultiplier => 1.75f;
	public override string Name => "Pipe Shotgun";

	public override string Dryfire => "rust_shotgun.dryfire";
	public virtual string ReloadStart => "rust_shotgun.reloadstart";
	public virtual string AddShell => "rust_shotgun.insert";
	public virtual string ReloadFinish => "rust_shotgun.reloadfinish";
	public override string DeploySound => "rust_shotgun.deploy";
	public override string PumpSound => "";

	[ClientRpc]
	public async override void StartReloadEffects()
	{
		base.StartReloadEffects();
		using ( Prediction.Off() )
		{
			await Task.Delay( 100 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ReloadStart );
			await Task.Delay( 2100 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( AddShell );
			await Task.Delay( 600 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ReloadFinish );
		}
	}
}
