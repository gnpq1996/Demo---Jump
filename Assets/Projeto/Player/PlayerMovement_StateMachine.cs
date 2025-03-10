using Demo.InputManagers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.StateMachines
{
    public class PlayerMovement_StateMachine : StateMachine
    {
        public State<PlayerMovement_StateMachine> previousState;
        public State<PlayerMovement_StateMachine> currentState;
        [Header("Components")]
        [field: SerializeField] public Demo.InputManagers.PlayerMovement_InputManager inputManager { get; private set; }
        [field: SerializeField] public Rigidbody2D rb { get; private set; }

        [System.Serializable]
        public class Movement_Info
        {
            [field: SerializeField] public float max_velocity { get; private set; }
            [field: SerializeField] public float aceleration { get; private set; }
            [field: SerializeField] public float desaceleration { get; private set; }
        }
        public Movement_Info movementInfo;


        [System.Serializable]
        public class Gravity_Info
        {
            [field: SerializeField] public float normal { get; private set; } = 5;
            [field: SerializeField] public float jumping { get; private set; } = 5;
            [field: SerializeField] public float falling { get; private set; } = 8;
        }
        public Gravity_Info gravity;

        [System.Serializable]
        public class Jump_Info
        {
            [field: SerializeField] public float jumpForce { get; private set; } = 10;
            [field: SerializeField, Range(0,1)] public float jumpStopForce { get; private set; } = 0.5f;
        }
        public Jump_Info jumpInfo;

        [System.Serializable]
        public class GroundCheck
        {
            public Transform transform;
            public bool isGrounded;
            public float timeStamp_lastIsGrounded = -100;
            public float checkRadius = 0.2f;
            public float checkHeight = 0.2f;
            public LayerMask groundLayer;

            public void IsGrounded()
            {
                var hitColliders = Physics2D.OverlapBox(transform.position, new Vector2(checkRadius, checkHeight), 0, groundLayer);

                if (hitColliders != null)
                {
                    isGrounded = true;
                    timeStamp_lastIsGrounded = Time.time;
                    return;
                }

                isGrounded = false;

            }
        }
        public GroundCheck groundCheck;
        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            Set_State(new Idle_State(this));
        }

        private void FixedUpdate()
        {
            groundCheck.IsGrounded();

            Tick_Machine();
        }

        public virtual void Tick_Machine()
        {
            if (currentState != null) currentState.Tick_State(this);
        }

        public void Set_State(State<PlayerMovement_StateMachine> newState)
        {
            if (currentState != null && newState.GetType() == currentState.GetType())
            {
                Debug.Log("Same state");
                return;
            }
            previousState = currentState;
            currentState = newState;
        }

        public void Movement_Free()
        {
            if (rb.linearVelocityY < -0.01f)
            {
                rb.gravityScale = gravity.falling;
            }
            else
            {
                //Debug.Log("Gravity up");
                rb.gravityScale = gravity.normal;
            }

            float targetVelocity = inputManager.inputDirection.x * movementInfo.max_velocity;
            float diferencial = targetVelocity - rb.linearVelocityX;
            float aceleration_type = targetVelocity != 0 ? movementInfo.aceleration : movementInfo.desaceleration;
            float finalVelocity = diferencial * movementInfo.max_velocity * aceleration_type;
            rb.AddForceX(finalVelocity);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(groundCheck.transform.position, new Vector2(groundCheck.checkRadius, groundCheck.checkHeight));
        }
    }

    public class Idle_State : State<PlayerMovement_StateMachine>
    {
        public Idle_State(PlayerMovement_StateMachine stateMachine) : base(stateMachine)
        {
            Debug.Log(ccolor.cyan("Changing State: Idle"));
        }

        public override void Tick_State(PlayerMovement_StateMachine stateMachine)
        {
            Debug.Log("Tick: Idle");
            stateMachine.Movement_Free();

            if (stateMachine.inputManager.trigger_jump.trigger)
            {
                //if (stateMachine.groundCheck.isGrounded ||
                //    stateMachine.inputManager.trigger_jump.graceTime_coyoteTime
                //    +
                //    stateMachine.inputManager.trigger_jump.timeStamp
                //    >
                //    Time.time)
                //{
                //    stateMachine.inputManager.trigger_jump.Deactivate();
                //    stateMachine.Set_State(new Jumping_State(stateMachine));
                //}
                if (stateMachine.groundCheck.isGrounded)
                {
                    Debug.Log(ccolor.orange("Grounded jump"));
                    stateMachine.inputManager.trigger_jump.Deactivate();
                    stateMachine.Set_State(new Jumping_State(stateMachine));
                }
                else if (stateMachine.inputManager.trigger_jump.graceTime_coyoteTime
                    +
                    stateMachine.groundCheck.timeStamp_lastIsGrounded
                    >
                    Time.time)
                {
                    Debug.Log(ccolor.orange("Coyote time jump"));
                    stateMachine.inputManager.trigger_jump.Deactivate();
                    stateMachine.Set_State(new Jumping_State(stateMachine));
                }
            }

        }
    }

    public class Jumping_State : State<PlayerMovement_StateMachine>
    {
        public float timeStamp = 0;
        public float delay = 0.2f;
        public Jumping_State(PlayerMovement_StateMachine stateMachine) : base(stateMachine)
        {
            Debug.Log(ccolor.cyan("Changing State: Jump"));
            timeStamp = Time.time + delay;
            stateMachine.rb.AddForce(Vector2.up * stateMachine.jumpInfo.jumpForce, ForceMode2D.Impulse);
        }

        public override void Tick_State(PlayerMovement_StateMachine stateMachine)
        {
            Debug.Log("Tick: Jump");
            stateMachine.Movement_Free();

            if (!stateMachine.groundCheck.isGrounded)
            {
                if (stateMachine.inputManager.trigger_stopJumping.trigger)
                {
                    stateMachine.inputManager.trigger_stopJumping.Deactivate();
                    if (stateMachine.rb.linearVelocityY > 0.1f)
                    {
                        Debug.Log("Reduzing jump speed");
                        stateMachine.rb.linearVelocity = new Vector2(stateMachine.rb.linearVelocity.x, stateMachine.rb.linearVelocityY * stateMachine.jumpInfo.jumpStopForce);
                    }
                }
                return;
            }
            if (timeStamp < Time.time)
            {
                stateMachine.Set_State(new Idle_State(stateMachine));
            }

        }
    }
}
