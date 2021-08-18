﻿using Sandbox;

[Library( "smg", Title = "SMG", Spawnable = true )]
public class SMG : Weapon
{
	public override float Damage => 25;
	public override bool IsAutomatic => true;
	public override int RPM => 600;
	public override float ReloadTime => 3f;
	public override float Spread => 0.15f;
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
}
