using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WeaponsSystem.MonoBehaviours;
using WeaponsSystem.ScriptableObjects;
using WeaponsSystem.ScriptableObjects.WeaponShootStrategies;

namespace Player
{
    public class InventoryManager : MonoBehaviour
    {
        private Dictionary<InventoryIndex, WeaponBehaviourScript> _weaponSlots;
        private InventoryIndex _currentActiveWeaponSlot;
        
        /// <summary>
        /// Events listened by the HUD to update the HUD inventory. Delegates allows to pass variable using events.
        /// </summary>
        public delegate void AddWeaponToHUDHandler(InventoryIndex slot, WeaponBehaviourScript weaponScript);
        public delegate void UpdateHUDInventoryHandler(InventoryIndex slot);
        public event AddWeaponToHUDHandler AddWeaponHUD;
        public event UpdateHUDInventoryHandler RemoveWeaponHUD;
        public event UpdateHUDInventoryHandler ChangeSelectedWeaponHUD;

        /// <summary>
        /// Public property to access the weapons currently in the inventory. Keys are slots, and values are the
        /// WeaponBehaviourScripts attached to the weapon gameobjects. Values can be null.
        /// </summary>
        public Dictionary<InventoryIndex, WeaponBehaviourScript> WeaponSlots => _weaponSlots;
        public InventoryIndex CurrentActiveWeaponSlot => _currentActiveWeaponSlot;

        private void Awake()
        {
            _weaponSlots = new Dictionary<InventoryIndex, WeaponBehaviourScript>
            {
                {InventoryIndex.First, null}, {InventoryIndex.Second, null}, {InventoryIndex.Throwable, null}
            };

            _currentActiveWeaponSlot = InventoryIndex.First;
        }

        private void Update()
        {
            if (_weaponSlots[_currentActiveWeaponSlot] != null)
            {
                _weaponSlots[_currentActiveWeaponSlot].WeaponStateProp = WeaponState.Active;
            }

            if (_weaponSlots[InventoryIndex.Throwable] != null)
            {
                _weaponSlots[InventoryIndex.Throwable].WeaponStateProp = WeaponState.Active;
            }
        }

        /// <summary>
        /// Returns the current active Weapon
        /// </summary>
        /// <returns>The current active Weapon</returns>
        public WeaponBehaviourScript GetActiveWeapon()
        {
            return _weaponSlots[_currentActiveWeaponSlot];
        }

        [CanBeNull]
        public WeaponBehaviourScript GetThrowableWeapon()
        {
            return _weaponSlots[InventoryIndex.Throwable];
        }
        
        public void SwitchActiveWeapon(KeyCode keyPressed)
        {
            switch (keyPressed)
            {
                case KeyCode.Alpha1:
                {
                    _currentActiveWeaponSlot = InventoryIndex.First;
                    ChangeSelectedWeaponHUD?.Invoke(InventoryIndex.First);
                    if (_weaponSlots[InventoryIndex.Second] != null)
                        _weaponSlots[InventoryIndex.Second].WeaponStateProp = WeaponState.InInventory;
                    break;
                }
                case KeyCode.Alpha2:
                {
                    _currentActiveWeaponSlot = InventoryIndex.Second;
                    ChangeSelectedWeaponHUD?.Invoke(InventoryIndex.Second);
                    if (_weaponSlots[InventoryIndex.First] != null)
                        _weaponSlots[InventoryIndex.First].WeaponStateProp = WeaponState.InInventory;
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a weapon to the inventory.
        /// </summary>
        /// <param name="weapon">The weapon gameobject to add to the inventory</param>
        /// <returns>A boolean indicating if the addition was successful</returns>
        public bool AddWeapon(GameObject weapon)
        {
            var weaponScript = weapon.GetComponent<WeaponBehaviourScript>();
            if (weaponScript == null) return false;

            if (weaponScript.WeaponData.ShootStrategy is ThrowableShootStrategy)
            {
                return AddWeapon(InventoryIndex.Throwable, weaponScript);
            }
            else
            {
                InventoryIndex? maybeIndex = GetInventoryIndexByWeapon(weaponScript);
                if (maybeIndex.HasValue)
                {
                    return AddWeapon(maybeIndex.Value, weaponScript);
                }
            
                if (weapon.CompareTag("Weapon"))
                {
                    var openSlot = GetFirstOpenSlot();
                    if (openSlot.HasValue)
                    {
                        return AddWeapon(openSlot.Value, weaponScript);
                    }
                    else
                    {
                        return AddWeapon(_currentActiveWeaponSlot, weaponScript);
                    }
                } 
                else if (weapon.CompareTag("Throwable"))
                {
                    return AddWeapon(InventoryIndex.Throwable, weaponScript);
                }
                else
                {
                    Debug.Log("Tried to add weapon that didn't have a valid tag.");
                    return false; 
                }
            }
        }
        
        /// <summary>
        /// Adds a weapon to the inventory, at the given inventory slot. If there already is a weapon there, drops it.
        /// </summary>
        /// <param name="slot">The inventory slot to add the weapon to</param>
        /// <param name="weaponScript">The WeaponBehaviourScript attached to the weapon gameobject</param>
        /// <returns>A boolean indicating if the addition was successful</returns>
        private bool AddWeapon(InventoryIndex slot, WeaponBehaviourScript weaponScript)
        {
            if (_weaponSlots[slot] != null)
            {
                if (_weaponSlots[slot].WeaponData.WeaponName.Equals(weaponScript.WeaponData.WeaponName))
                {
                    if (_weaponSlots[slot].WeaponData.WeaponName is WeaponName.Grenade)
                    {
                        _weaponSlots[slot].CurrentMagazineAmmunition += weaponScript.CurrentMagazineAmmunition + weaponScript.CurrentTotalAmmunition;
                    }
                    else
                    {
                        _weaponSlots[slot].CurrentTotalAmmunition += weaponScript.CurrentMagazineAmmunition + weaponScript.CurrentTotalAmmunition;
                    }
                    Destroy(weaponScript.gameObject);
                    weaponScript.gameObject.transform.parent = gameObject.transform;
                    return true;
                }
                else
                {
                    _weaponSlots[slot].WeaponStateProp = WeaponState.OnGround;
                    _weaponSlots[slot].transform.parent = null;
                    weaponScript.WeaponStateProp = WeaponState.InInventory;
                    _weaponSlots[slot] = weaponScript;
                    AddWeaponHUD?.Invoke(slot, weaponScript);
                    weaponScript.gameObject.transform.parent = gameObject.transform;
                    return true; 
                }
            }
            else
            {
                weaponScript.WeaponStateProp = WeaponState.InInventory;
                _weaponSlots[slot] = weaponScript;
                AddWeaponHUD?.Invoke(slot, weaponScript);
                weaponScript.gameObject.transform.parent = gameObject.transform;
                return true;
            }
        }
        
        private InventoryIndex? GetFirstOpenSlot()
        {
            if (_weaponSlots[InventoryIndex.First] == null)
            {
                return InventoryIndex.First;
            }
            else if (_weaponSlots[InventoryIndex.Second] == null)
            {
                return InventoryIndex.Second;
            }

            return null;
        }

        private InventoryIndex? GetInventoryIndexByWeapon(WeaponBehaviourScript weaponScript)
        {
            if (_weaponSlots[InventoryIndex.First] != null && _weaponSlots[InventoryIndex.First].WeaponData.WeaponName.Equals(weaponScript.WeaponData.WeaponName))
            {
                return InventoryIndex.First;
            } 
            else if (_weaponSlots[InventoryIndex.Second] != null && _weaponSlots[InventoryIndex.Second].WeaponData.WeaponName.Equals(weaponScript.WeaponData.WeaponName))
            {
                return InventoryIndex.Second;
            } 
            else if (_weaponSlots[InventoryIndex.Throwable] != null && _weaponSlots[InventoryIndex.Throwable].WeaponData.WeaponName.Equals(weaponScript.WeaponData.WeaponName))
            {
                return InventoryIndex.Throwable;
            }

            return null;
        }
    }

    public enum InventoryIndex
    {
        First,
        Second,
        Throwable
    }
}