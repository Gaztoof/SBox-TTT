using Sandbox;

[Library( "smg", Title = "SMG", Spawnable = true )]
public partial class SMG : Weapon
{
	public override float Damage => 25;
	public override bool IsAutomatic => true;
	public override int RPM => 600;
	public override float ReloadTime => 3f;
	public override float Spread => 0.15f;
	public override float DeployTime => 2f;
	public override int ClipSize => 25;
	public override string ShootShound => "rust_smg.shoot";
	public override string WorldModelPath => "weapons/rust_smg/rust_smg.vmdl";
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
	public override CrosshairType CrosshairType => CrosshairType.Circle;
	public override HoldType HoldType => HoldType.SMG;
	public override AmmoType AmmoType => AmmoType.SMG;
	public override bool CanAim => true;
	public override float HeadshotMultiplier => 1.75f;
	public override string Name => "SMG";
	public override string Dryfire => "rust_smg.dryfire";
	public virtual string ReloadStart => "rust_smg.reloadstart";
	public override string DeploySound => "rust_smg.deploy";
	public virtual string BoltInSound => "rust_smg.boltshut";
	public virtual string BoltOutSound => "rust_smg.boltback";
	public virtual string ClipInSound => "rust_smg.clipin";
	public virtual string ClipOutSound => "rust_smg.clipout";

	[ClientRpc]
	public async override void OnDeploy()
	{
		using ( Prediction.Off() )
		{
			PlaySound( DeploySound );
			await Task.Delay( 200 );

			PlaySound( BoltOutSound );
			await Task.Delay( 500 );
			PlaySound( BoltInSound );

		}
	}
	[ClientRpc]
	public async override void StartReloadEffects()
	{
		base.StartReloadEffects();
		using ( Prediction.Off() )
		{
			PlaySound( ReloadStart );
			await Task.Delay( 500 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ClipOutSound );
			await Task.Delay( 1100 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( ClipInSound );

			await Task.Delay( 1800 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( BoltOutSound );
			await Task.Delay( 600 );
			if ( (Parent as Player).ActiveChild != this ) return;// ugly asf
			PlaySound( BoltInSound );
		}
	}
}
