using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Player : Entity
    {
        [SerializeField] Sprite meleeSprite;
        [SerializeField] Sprite gunSprite;
        SpriteRenderer characterSprite;
        bool isGun = false;
        public override InstancePool.PoolableType poolableType => InstancePool.PoolableType.Player;
        public override void Awake()
        {
            base.Awake();
            CameraScript.SetTarget(transform);
            characterSprite = character.GetComponent<SpriteRenderer>(); ;
        }
        private void Update()
        {
            Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SetMoveDirection(inputDirection);
            Vector2 aimDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            SetAimDirection(aimDirection);
            
            if (Input.GetMouseButtonDown(0))
            {
                if (isGun)
                {
                    Fire();
                } else
                {
                    Melee();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isGun = !isGun;
                SetGunSight(isGun);
                if (isGun)
                {
                    characterSprite.sprite = gunSprite;
                } else
                {
                    characterSprite.sprite = meleeSprite;
                }
            }
            if (isGun)
            {
                if (isReloaded)
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
            CameraScript.SetTarget(other.transform);
        }
        public override void OnRespawn()
        {
            base.OnRespawn();
            CameraScript.SetTarget(transform);
            transform.position = Vector3.zero;
        }
    }
}
