using Player.Commands;
using UnityEngine;

namespace Player.States
{
    /// <summary>
    /// The State where the player is not moving and on the ground
    /// </summary>
    public class IdleState : BaseState
    {
        public override void Start()
        {
            base.Start();

            Controller.Rigidbody.velocity = new Vector2(0.0F, Controller.Rigidbody.velocity.y);

            Controller.Animator.SetInteger("AnimState", 0);
        }

        public override void Update(Command cmd)
        {
            base.Update(cmd);

            if (cmd is JumpCommand || cmd is InteractCommand || cmd is ShootCommand || cmd is ReloadCommand ||
                cmd is SwitchWeaponCommand || cmd is ThrowCommand || cmd is LeaveGameCommand)
            {
                cmd.Execute(Controller);
            }

            Controller.UpdateTextureDirection();

            if (Controller.IsPressingRight || Controller.IsPressingLeft)
            {
                Controller.ChangeState(new RunState());
            }

            if (Controller.IsPressingUp || Controller.IsPressingDown)
            {
                if (Controller.CanClimb)
                {
                    Controller.ChangeState(new ClimbState());
                }
            }

            base.CheckAndHandleFallthrough();
            base.CheckAndHandleFall();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}