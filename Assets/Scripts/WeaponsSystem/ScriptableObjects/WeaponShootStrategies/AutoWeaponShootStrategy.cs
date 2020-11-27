﻿using System;
using System.Collections;
using UnityEngine;
using WeaponsSystem.MonoBehaviours;

namespace WeaponsSystem.ScriptableObjects.WeaponShootStrategies
{
    /// <summary>
    /// Meant for weapons that you can keep shooting as long as you have ammo. (Ex. ak47)
    /// </summary>
    [CreateAssetMenu(fileName = "NewAutoWeaponShootStrategy", menuName = "ScriptableObjects/WeaponShootStrategy/Auto", order = 2)]
    public class AutoWeaponShootStrategy: WeaponShootStrategy
    {
        private const bool DefaultCanShootValue = true;
        private const bool DefaultCanReloadValue = true;

        [SerializeField] private bool canReload = true;
        [SerializeField] private bool canShoot = true;
        public override void Shoot(WeaponBehaviourScript weapon)
        {
            if (canShoot && weapon.CurrentMagazineAmmunition > 0)
            {
                weapon.StartCoroutine(WaitForShot(weapon));
            }
        }
    
        private IEnumerator WaitForShot(WeaponBehaviourScript weapon)
        {
            canShoot = false;
            SpawnProjectile(weapon);
            weapon.CurrentMagazineAmmunition -= 1;
            yield return new WaitForSeconds(1.0f / weapon.WeaponData.FireRate);
            canShoot = true;
        }

        public override void Reload(WeaponBehaviourScript weapon)
        {
            if (canReload && weapon.CurrentTotalAmmunition >= 1 && weapon.CurrentMagazineAmmunition < weapon.WeaponData.MagazineCapacity)
            {
                weapon.StartCoroutine(WaitForReload(weapon));
            }
        }
    
        private IEnumerator WaitForReload(WeaponBehaviourScript weapon)
        {
            canReload = false;
            canShoot = false;
            int reloadAmount = weapon.WeaponData.MagazineCapacity - weapon.CurrentMagazineAmmunition;
            reloadAmount = Math.Min(reloadAmount, weapon.CurrentTotalAmmunition);
            yield return new WaitForSeconds(weapon.WeaponData.ReloadTime);
            weapon.CurrentTotalAmmunition -= reloadAmount;
            weapon.CurrentMagazineAmmunition += reloadAmount;
            canShoot = true;
            canReload = true;
        }
    
        protected override void SpawnProjectile(WeaponBehaviourScript weapon)
        {
            GameObject projectile = Instantiate(weapon.WeaponData.ProjectileData.ProjectilePrefab, weapon.WeaponShootLocation, Quaternion.identity);
            
            // Make sure the weapon has a parent gameobject of this line is gonna cause a Nullptrexception
            projectile.GetComponent<ProjectileBehaviourScript>().Init(weapon.WeaponData.ProjectileData, weapon.Direction,!weapon.transform.parent.CompareTag("Player"));
            
            projectile.SetActive(true);
        }

        private void Awake()
        {
            canShoot = DefaultCanShootValue;
            canReload = DefaultCanReloadValue;
        }

        private void OnEnable()
        {
            canShoot = DefaultCanShootValue;
            canReload = DefaultCanReloadValue;
        }

        private void OnDisable()
        {
            canShoot = DefaultCanShootValue;
            canReload = DefaultCanReloadValue;
        }

        private void OnDestroy()
        {
            canShoot = DefaultCanShootValue;
            canReload = DefaultCanReloadValue;
        }
    }
}