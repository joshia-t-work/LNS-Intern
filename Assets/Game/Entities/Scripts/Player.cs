using LNS.CameraMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    public class Player : Soldier
    {
        [SerializeField] Sprite _meleeSprite;
        [SerializeField] Sprite _gunSprite;
        [SerializeField] CameraScript _cam;
        SpriteRenderer _characterSprite;
        bool _isGun = false;
        public override void Awake()
        {
            base.Awake();
            _cam.SetTarget(transform);
            _characterSprite = _character.GetComponent<SpriteRenderer>(); ;
        }
        private void Update()
        {
            Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SetMoveDirection(inputDirection);
            Vector2 aimDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            SetAimDirection(aimDirection);
            
            if (Input.GetMouseButtonDown(0))
            {
                if (_isGun)
                {
                    Fire();
                } else
                {
                    Melee();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isGun = !_isGun;
                SetGunSight(_isGun);
                if (_isGun)
                {
                    _characterSprite.sprite = _gunSprite;
                } else
                {
                    _characterSprite.sprite = _meleeSprite;
                }
            }
            if (_isGun)
            {
                if (IsReloaded)
                {
                    SetGunSight(true);
                } else
                {
                    SetGunSight(false);
                }
            }
        }
        public override void OnKilled(Damagable other)
        {
            base.OnKilled(other);
            _cam.SetTarget(other.transform);
        }
        public override void OnRespawn()
        {
            base.OnRespawn();
            _cam.SetTarget(transform);
            transform.position = Vector3.zero;
        }
    }
}
