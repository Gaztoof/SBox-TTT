using Sandbox;

public partial class Player
{
	public enum HitboxGroup
	{
		None = -1,
		Generic = 0,
		Head = 1,
		Chest = 2,
		Stomach = 3,
		LeftArm = 4,
		RightArm = 5,
		LeftLeg = 6,
		RightLeg = 7,
		Gear = 10,
		Special = 11,
	}
	private float ScaleDamage(float damage, HitboxGroup hitboxGroup, Entity damageWeapon)
	{
		switch(hitboxGroup)
		{
			case HitboxGroup.None:
				return damage *= 1.0f;
			case HitboxGroup.Generic:
				return damage *= 1.0f;
			case HitboxGroup.Head:
				if ( damageWeapon is Weapon weapon )
					return damage *= weapon.HeadshotMultiplier;
				else return damage *= 1.0f;
			case HitboxGroup.Chest:
				return damage *= 1.0f;
			case HitboxGroup.Stomach:
				return damage *= 1.0f;
			case HitboxGroup.LeftArm:
				return damage *= 0.55f;
			case HitboxGroup.RightArm:
				return damage *= 0.55f;
			case HitboxGroup.LeftLeg:
				return damage *= 0.55f;
			case HitboxGroup.RightLeg:
				return damage *= 0.55f;
			case HitboxGroup.Gear:
				return damage *= 0.55f;
			case HitboxGroup.Special:
				return damage *= 1.0f;
		}
		return damage;
	}
}
