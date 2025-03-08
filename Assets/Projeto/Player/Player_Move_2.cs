using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.Player.Movement
{
    public class Player_Move_2 : MonoBehaviour
    {
        [SerializeField] private Vector2 inputDirection;
        public Rigidbody2D rb;

        [Header("Movement")]
        public float velocity = 10;
        public float aceleration = 1;
        public float desaceleration = 1;

        [Header("Ground Check")]
        public bool isGrounded = false;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;

        [Header("Inputs")]
        public bool jump_input = false;
        public bool jumpStop_input = false;

        float jump_lastGround_time = -100;
        float jump_input_time = -100;
        float jump_stopForce = 0.5f;

        [Header("Jump")]
        public int max_jumps = 1;
        public int current_jumps = 0;
        [Min(0.1f)]
        public float gracePeriod_beforeGround = 0.3f;
        [Min(0.1f)]
        public float gracePeriod_coyoteTime = 0.3f;
        public float jump_force = 10;
        public float gravity_up = 10;
        public float gravity_down = 10;

        public void InputHandler_Jump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                //Debug.Log("Input: Jump");
                jump_input_time = Time.time;
                jump_input = true;
            }
            else if (context.canceled)
            {
                //Debug.Log("Input: Stop jumping");
                jumpStop_input = true;
            }
        }
        public void InputHandler_Movement(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            IsGrounded();
            Jump_Handler();
            Movement();
        }

        private void Jump_Handler()
        {
            if (jumpStop_input)
            {
                if (rb.linearVelocityY > 0.1f)
                {
                    Debug.Log("Reduzing jump speed");
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocityY * jump_stopForce);
                }

                jumpStop_input = false;
                jump_input = false;
                return;
            }

            if (!jump_input)
            {
                return;
            }

            if (isGrounded)
            {
                Debug.Log("Grounded jump");
                Jump();
                return;
            }
            else if (jump_lastGround_time + gracePeriod_coyoteTime > Time.time)
            {
                Debug.Log("Coyote time jump");
                Jump();
            }
            else if (jump_input_time + gracePeriod_beforeGround < Time.time)
            {
                Debug.Log("To late");
                jump_input = false;
            }
        }

        private void Jump()
        {
            if (current_jumps >= max_jumps)
            {
                jump_input = false;
                jump_lastGround_time = -100;
                return;
            }
            current_jumps++;
            jump_lastGround_time = -100;
            Debug.Log("JUMP!: " + current_jumps);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForceY(jump_force);
            jump_input = false;
        }

        private void Movement()
        {
            if (rb.linearVelocityY < -0.01f)
            {
                //Debug.Log("Gravity down");
                rb.gravityScale = gravity_down;
            }
            else
            {
                //Debug.Log("Gravity up");
                rb.gravityScale = gravity_up;
            }

            float targetVelocity = inputDirection.x * velocity;
            float diferencial = targetVelocity - rb.linearVelocityX;
            float aceleration_type = targetVelocity != 0 ? aceleration : desaceleration;
            float finalVelocity = diferencial * velocity * aceleration_type;
            rb.AddForceX(finalVelocity);
        }
        private void IsGrounded()
        {
            var hitColliders = Physics2D.OverlapBox(groundCheck.position, new Vector2(groundCheckRadius, 0.2f), 0, groundLayer);

            if (hitColliders != null)
            {
                Debug.Log("Colisão com: " + hitColliders.name);

                jump_lastGround_time = Time.time;
                isGrounded = true;
                current_jumps = 0;
            }
            else
            {
                isGrounded = false;
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(groundCheck.position, new Vector2(groundCheckRadius, 0.2f));
        }
    }
}
