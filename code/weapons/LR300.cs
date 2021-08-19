using Sandbox;
using System;

[Obsolete("I can't get the animations to work on the viewmodel...")]
[Library( "lr300", Title = "LR300", Spawnable = true )]
public class LR300 : Weapon
{
	public override float Damage => 25;
	public override bool IsAutomatic => true;
	public override int RPM => 600;
	public override float ReloadTime => 3f;
	public override float Spread => 0.15f;
	public override int ClipSize => 25;
	public override string ShootShound => "rust_smg.shoot";
	public override string WorldModelPath => "weapons/rust_lr300/rust_lr300.vmdl";
	public override string ViewModelPath => "weapons/rust_lr300/v_rust_lr300.vmdl";
	public override CrosshairType CrosshairType => CrosshairType.Circle;
	public override HoldType HoldType => HoldType.SMG;
	public override AmmoType AmmoType => AmmoType.SMG;
	public override bool CanAim => true;
	public override float HeadshotMultiplier => 1.75f;
	public override string Name => "LR300";
	public override string Dryfire => "rust_smg.dryfire";
	public virtual string ReloadStart => "rust_smg.reloadstart";
}
