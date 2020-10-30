﻿using System;
using UnityEngine;

/// <summary>
/// Controls the behavior of a weapon. 
/// </summary>
public class WeaponBehaviourScript : MonoBehaviour
{
    // TODO: 
    // 1. Make the Active weapon follow the direction of the player.
    // 2. Change state of the weapon based on game state
    // 3. Make the projectiles spawn at the end of the weapon (+- done)
    // 4. Make sure the weapon gameobject does not have more ammo than allowed by the WeaponData instance
    // 5. Make sure proper keybindings are set and used

    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WeaponState weaponState;
    [SerializeField] private int currentMagazineAmmunition;
    [SerializeField] private int currentTotalAmmunition;
    private WeaponOnGroundBehaviour _weaponOnGroundBehaviour;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private Vector2 _direction;
    
    
    public Vector2 WeaponSpriteEndPosition => (Vector2) transform.position + (_direction * new Vector2(_spriteRenderer.sprite.bounds.extents.x, _spriteRenderer.sprite.bounds.extents.y));
    public WeaponData WeaponData => weaponData;

    public Vector2 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            if (_direction == Vector2.left)
            {
                _spriteRenderer.flipX = true;
            } else if (_direction.Equals(Vector2.right))
            {
                _spriteRenderer.flipX = false;
            }
        }
    }

    public WeaponState WeaponStateProp
    {
        get => weaponState;
        set => weaponState = value;
    }

    public int CurrentMagazineAmmunition
    {
        get => currentMagazineAmmunition;
        set => currentMagazineAmmunition = value;
    }

    public int CurrentTotalAmmunition
    {
        get => currentTotalAmmunition;
        set => currentTotalAmmunition = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.sleepMode = RigidbodySleepMode2D.StartAsleep;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _weaponOnGroundBehaviour = GetComponent<WeaponOnGroundBehaviour>();
        _weaponOnGroundBehaviour.Init(weaponData);
        
        //TODO remove this
        Direction = Vector2.left;
    }

    private void Update()
    {
        switch (weaponState)
        {
            case WeaponState.OnGround:
                _weaponOnGroundBehaviour.enabled = true;
                _rigidbody.WakeUp();
                break;
            case WeaponState.InInventory:
                _weaponOnGroundBehaviour.enabled = false;
                _spriteRenderer.sprite = weaponData.InInventorySprite;
                _rigidbody.Sleep();
                break;
            case WeaponState.Inactive:
                _weaponOnGroundBehaviour.enabled = false;
                _spriteRenderer.sprite = weaponData.InactiveSprite;
                break;
            case WeaponState.Active:
                _weaponOnGroundBehaviour.enabled = false;
                _spriteRenderer.sprite = weaponData.ActiveSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // TODO: Move this Input stuff to player game object
        if (weaponState == WeaponState.Active)
        {
            if (Input.GetMouseButton(0))
            {
                Shoot();
            } 
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Init(Vector2 direction)
    {
        this.Direction = direction;
    }
    
    public void Shoot()
    {
        weaponData.ShootStrategy.Shoot(this);
    }

    public void Reload()
    {
        weaponData.ShootStrategy.Reload(this);
    }

    public enum WeaponState
    {
        OnGround,
        InInventory,
        Inactive,
        Active
    }
}
