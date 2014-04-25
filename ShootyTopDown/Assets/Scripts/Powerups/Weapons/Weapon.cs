using UnityEngine;
using System.Collections;

public class Weapon : Powerup
{
	public CharacterBase.WeaponType weaponType;
	public int ammoCount = 10;

	protected override void Activate(CharacterBase character)
	{
		character.SwitchWeapon(weaponType, ammoCount);
	}
}
