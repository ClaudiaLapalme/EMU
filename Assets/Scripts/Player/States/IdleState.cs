﻿using System.Collections;
using System.Collections.Generic;
using Player.States;
using UnityEngine;
using Controller = Player.Controller;

namespace Player.States
{
    public class IdleState : BaseState
    {
        public override void Start()
        {
            base.Start();
            
            Debug.Log("Idle State");
            
            Controller.Rigidbody.velocity = new Vector2(0.0F, Controller.Rigidbody.velocity.y);
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Controller.ChangeState(new RunState {IsFacingRight = true});
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Controller.ChangeState(new RunState {IsFacingRight = false});
            }

            Controller.Animator.SetInteger("AnimState", 0);
            Controller.Animator.SetBool("Grounded", true);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }   
}
