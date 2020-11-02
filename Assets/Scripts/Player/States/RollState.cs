﻿using Player.Commands;
using UnityEngine;

namespace Player.States
{
    /// <summary>
    /// The state where the player is rolling
    /// </summary>
    public class RollState : BaseState
    {
        public float XAxisRaw = 0.0f;
        
        public override void Start()
        {
            base.Start();
            
            Debug.Log("Roll State");
            
            Controller.Animator.SetTrigger("Roll");
        }

        public override void Update(Command cmd)
        {
            base.Update(cmd);

            Controller.Move(XAxisRaw);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }   
}
