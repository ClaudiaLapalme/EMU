using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeaponsSystem.MonoBehaviours;
using WeaponsSystem.ScriptableObjects;

namespace Interactables
{
    public class ChestBehaviourScript : MonoBehaviour
    {
        private const string GameobjectName = "ChestInteractTrigger";
        
        [SerializeField] private HealthPickupParams healthPickupParams;
        [SerializeField] private AmmoPickupParams ammoPickupParams;
        [SerializeField] private List<WeaponPickupParams> weaponPickupList;

        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private GameObject ammoPickupPrefab;
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] private AudioClip openSoundClip;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private GameObject promptPrefab;
        [SerializeField] private Vector2 promptHitboxSize;
        [SerializeField] private int promptTextSize;

        private GameObject _triggerCollider;
        private SpriteRenderer _spriteRenderer;
        
        private bool _isOpen;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _isOpen ? openSprite : closedSprite;
            
            _triggerCollider = new GameObject {layer = LayerMask.NameToLayer("Trigger"), name = GameobjectName, };
            _triggerCollider.transform.parent = transform;
            _triggerCollider.tag = "InteractTrigger";
            _triggerCollider.AddComponent<BoxCollider2D>();
            _triggerCollider.AddComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
            _triggerCollider.AddComponent<InteractOnGroundTriggerScript>().Init(promptPrefab, promptHitboxSize, promptTextSize);
        
            _triggerCollider.SetActive(true);
        }

        public void Interact()
        {

            if (_isOpen)
            {
                Debug.Log("Tried Interacting with an already open chest");
                _spriteRenderer.sprite = openSprite;
                _triggerCollider.SetActive(false);
                enabled = false;
            }
            else
            {
                GetComponent<AudioSource>()?.PlayOneShot(openSoundClip, PlayerPrefs.GetInt("volume") / 10.0f);
                
                for (var i = 0; i < healthPickupParams.numberOfPickups; i++)
                {
                    var pickup = Instantiate(healthPickupPrefab, (Vector2) transform.position + new Vector2(0f, 0.5f),
                        Quaternion.identity);
                    pickup.GetComponent<HealthPickupBehaviour>().Init(healthPickupParams.healthAmount);
                }

                for (var i = 0; i < ammoPickupParams.numberOfPickups; i++)
                {
                    var pickup = Instantiate(ammoPickupPrefab, (Vector2) transform.position + new Vector2(0f, 0.5f),
                        Quaternion.identity);
                    pickup.GetComponent<AmmoPickupBehaviour>().Init(ammoPickupParams.percentage);
                }

                foreach (var weaponParam in weaponPickupList)
                {
                    var droppedWeapon = GameObject.Instantiate(weaponPrefab,
                        (Vector2) transform.position + new Vector2(0f, 0.5f), Quaternion.identity);

                    var droppedWeaponBehaviour = droppedWeapon.GetComponent<WeaponBehaviourScript>();
                    droppedWeaponBehaviour.WeaponData = WeaponDataDictionary.weaponDataDictionary[weaponParam.weaponName];
                    droppedWeaponBehaviour.CurrentMagazineAmmunition = weaponParam.magazineAmmunition;
                    droppedWeaponBehaviour.CurrentTotalAmmunition = weaponParam.totalAmmunition;
                    droppedWeaponBehaviour.WeaponStateProp = WeaponState.OnGround;
                }
                
                _isOpen = true;
                _spriteRenderer.sprite = openSprite;
                _triggerCollider.SetActive(false);
                enabled = false;
            }
        }
    }

    // This is fucking hacky
    internal static class WeaponDataDictionary
    {
        public static Dictionary<WeaponName, WeaponData> weaponDataDictionary;

        static WeaponDataDictionary()
        {
            // I'll leave this here in case
            /*#if UNITY_EDITOR
            weaponDataDictionary = new Dictionary<WeaponName, WeaponData>()
            {
                {WeaponName.AssaultRifle, (WeaponData) AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Instances/WeaponsSystem/WeaponData/AssaultRifleData.asset", typeof(WeaponData))},
                {WeaponName.Sniper, (WeaponData) AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Instances/WeaponsSystem/WeaponData/SniperData.asset", typeof(WeaponData))},
                {WeaponName.Shotgun, (WeaponData) AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Instances/WeaponsSystem/WeaponData/ShotgunData.asset", typeof(WeaponData))},
                {WeaponName.Knife, (WeaponData) AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Instances/WeaponsSystem/WeaponData/KnifeData.asset", typeof(WeaponData))},
                {WeaponName.Grenade, (WeaponData) AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Instances/WeaponsSystem/WeaponData/GrenadeData.asset", typeof(WeaponData))}
            };
            #else*/
            Debug.Log(Path.Combine(Application.streamingAssetsPath, "/AssetBundles/scriptableobjectinstances"));
            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/scriptableobjectinstances"));
            weaponDataDictionary = new Dictionary<WeaponName, WeaponData>()
            {
                {WeaponName.AssaultRifle, assetBundle.LoadAsset<WeaponData>("AssaultRifleData")},
                {WeaponName.Sniper, assetBundle.LoadAsset<WeaponData>("SniperData")},
                {WeaponName.Shotgun, assetBundle.LoadAsset<WeaponData>("ShotgunData")},
                {WeaponName.Knife, assetBundle.LoadAsset<WeaponData>("KnifeData")},
                {WeaponName.Grenade, assetBundle.LoadAsset<WeaponData>("GrenadeData")}
            };
            //#endif
        }
    }

    [Serializable]
    internal struct HealthPickupParams
    {
        public int numberOfPickups;
        public int healthAmount;
    }

    [Serializable]
    internal struct AmmoPickupParams
    {
        public int numberOfPickups;
        public int percentage;
    }

    [Serializable]
    internal struct WeaponPickupParams
    {
        public WeaponName weaponName;
        public int magazineAmmunition;
        public int totalAmmunition;
    }
}