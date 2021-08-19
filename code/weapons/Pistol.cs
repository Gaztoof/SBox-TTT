using Sandbox;

[Library( "pistol", Title = "Pistol", Spawnable = true )]
public partial class Pistol : Weapon
{
	public override float Damage => 40;
	public override bool IsAutomatic => false;
	public override int RPM => 600;
	public override float ReloadTime => 2.2f;
	public override float Spread => 0.05f;
	public override string ShootShound => "rust_pistol.shoot";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override CrosshairType CrosshairType => CrosshairType.Cross;
	public override int ClipSize => 15;
	public override AmmoType AmmoType => AmmoType.Pistol;
	public override bool CanAim => true;
	public override float HeadshotMultiplier => 1.75f;
	public override string Name => "Pistol";
	public override string Dryfire => "rust_pistol.dryfire";
	public override string DeploySound => "rust_pistol.deploy";
	public virtual string GrabClip => "rust_pistol.grabclip";
	public virtual string ClipInSound => "rust_pistol.clipin";
	public virtual string ClipOutSound => "rust_pistol.clipout";

	[ClientRpc]
	public override void OnDeploy()
	{
		using ( Prediction.Off() )
		{
			PlaySound( DeploySound );
		}
	}
	[ClientRpc]
	public async override void StartReloadEffects()
	{
		base.StartReloadEffects();
		using ( Prediction.Off() )
		{
			await Task.Delay( 100 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ClipOutSound );
			await Task.Delay( 800 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( GrabClip );
			PlaySound( ClipInSound );
		}
	}

}
