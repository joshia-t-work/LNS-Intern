﻿using LNS.CameraMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// A special type of soldier that owns its own following camera
    /// </summary>
    public class Player : Soldier
    {
        [Header("Player")]
        [SerializeField]
        CameraScript _cam;
        public override void Awake()
        {
            base.Awake();
            _cam.SetTarget(this);
        }
        public override void OnKilledBy(Damagable other)
        {
            base.OnKilledBy(other);
            _cam.SetTarget((Soldier)other.TraceOwner());
        }
        public override void OnRespawn()
        {
            base.OnRespawn();
            _cam.SetTarget(this);
        }
    }
}
