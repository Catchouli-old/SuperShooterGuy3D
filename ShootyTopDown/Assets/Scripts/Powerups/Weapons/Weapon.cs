using UnityEngine;
using System.Collections;

public class Weapon : Powerup
{
	private const int AMMO_AMOUNT = 50;

	public CharacterBase.WeaponType weaponType;

	protected override void Activate(CharacterBase character)
	{
		character.CurrentAmmo = AMMO_AMOUNT;
		character.CurrentWeapon = weaponType;
	}
}
