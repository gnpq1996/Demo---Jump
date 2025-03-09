using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Demo.InputManagers;

namespace Demo.Player.Movement
{
    public class Player_Move_2 : MonoBehaviour
    {
        [SerializeField] private Vector2 inputDirection;
        public Rigidbody2D rb;

        [Header("Inputs")]
        public bool input_jump = false;

        [Header("Triggers")]
        public InputTrigger_Jump trigger_jump;
        public InputTrigger trigger_stopJumping;

        [Header("Movement")]
        public float velocity = 10;
        public float aceleration = 1;
        public float desaceleration = 1;

        [Header("Ground Check")]
        public bool isGrounded = false;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public float groundCheckHeight = 0.2f;
        public LayerMask groundLayer;
        public float timeStamp_lastIsGrounded = -100;

        [Header("Jump")]
        public int max_jumps = 1;
        public int current_jumps = 0;
        public float jump_force = 10;
        public float jump_stopForce = 0.5f;
        public float gravity_normal = 10;
        public float gravity_down = 10;

        public void InputHandler_Jump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                //Debug.Log("Input: Jump");
                trigger_jump.Activate();
                input_jump = true;
            }
            else if (context.canceled)
            {
                //Debug.Log("Input: Stop jumping");
                trigger_stopJumping.Activate();
                input_jump = false;
            }
        }
        public void InputHandler_Movement(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            IsGrounded();
            Movement();
            Jump_Handler();

            Reset_Triggers();
        }

        void Reset_Triggers()
        {
            trigger_stopJumping.ResetTrigger();
            trigger_jump.ResetTrigger();
        }

        private void Jump_Handler()
        {
            if (trigger_stopJumping.trigger)
            {
                if (rb.linearVelocityY > 0.1f)
                {
                    Debug.Log("Reduzing jump speed");
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocityY * jump_stopForce);
                }
                return;
            }

            if (!trigger_jump.trigger)
            {
                return;
            }

            if (current_jumps < max_jumps)
            {
                Debug.Log("Grounded jump");
                Jump();
                return;
            }
        }

        private void Jump()
        {
            current_jumps++;
            Debug.Log("JUMP!: " + current_jumps);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForceY(jump_force);
            trigger_jump.Deactivate();
            timeStamp_lastIsGrounded = -100;
        }

        private void Movement()
        {
            if (rb.linearVelocityY < -0.01f)
            {
                rb.gravityScale = gravity_down;
            }
            else
            {
                //Debug.Log("Gravity up");
                rb.gravityScale = gravity_normal;
            }

            float targetVelocity = inputDirection.x * velocity;
            float diferencial = targetVelocity - rb.linearVelocityX;
            float aceleration_type = targetVelocity != 0 ? aceleration : desaceleration;
            float finalVelocity = diferencial * velocity * aceleration_type;
            rb.AddForceX(finalVelocity);
        }
        private void IsGrounded()
        {
            var hitColliders = Physics2D.OverlapBox(groundCheck.position, new Vector2(groundCheckRadius, groundCheckHeight), 0, groundLayer);

            if (hitColliders != null)
            {
                rb.gravityScale = gravity_normal;
                current_jumps = 0;

                isGrounded = true;

                timeStamp_lastIsGrounded = Time.time;
                return;
            }

            isGrounded = false;

        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(groundCheck.position, new Vector2(groundCheckRadius, groundCheckHeight));
        }
    }
}
