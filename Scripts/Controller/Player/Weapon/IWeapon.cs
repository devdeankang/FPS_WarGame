using UnityEngine;

public interface IWeaponShootMode
{
    Sprite GetCurrentModeSprite();
}

public interface IWeaponAmmo
{
    int GetCurrentAmmo();
    int GetMaxAmmo();
    string GetCurrentAmmoString();
    string GetCapacityAmmoString();
}