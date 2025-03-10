using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Demo.StateMachines.Player
{
    public class PlayerMovement_StateMachine : StateMachine
    {
        #region Variables
        public State<PlayerMovement_StateMachine> previousState;
        public State<PlayerMovement_StateMachine> currentState;

        [Header("Components")]
        [field: SerializeField] public Demo.InputManagers.PlayerMovement_InputManager inputManager { get; private set; }
        [field: SerializeField] public Rigidbody2D rb { get; private set; }
        [field: SerializeField] public Transform gfx { get; private set; }


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
            [field: SerializeField, Range(0, 1)] public float jumpStopForce { get; private set; } = 0.5f;
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


        [System.Serializable]
        public class State_Events
        {
            [field: SerializeField] public UnityEvent onJump { get; private set; }
            [field: SerializeField] public UnityEvent onAirJump { get; private set; }
        }
        public State_Events stateEvents;
        #endregion

        #region MonoBehaviours
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



        #endregion

        #region StateMachine
        //====================================================================================================================
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

            EventHandler_StateEvents();
        }

        void EventHandler_StateEvents()
        {
            switch (currentState)
            {
                case Demo.StateMachines.Player.Jumping_State state:
                    {
                        stateEvents.onJump?.Invoke();
                        break;
                    }
                case Demo.StateMachines.Player.Air_Jump_State state:
                    {
                        stateEvents.onAirJump?.Invoke();
                        break;
                    }
            }
        }



        #endregion

        #region State Tools
        //====================================================================================================================
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

        public void Jump()
        {
            gfx.DOShakeScale(0.2f, 0.3f, 10, 90, false);
            rb.AddForce(Vector2.up * jumpInfo.jumpForce, ForceMode2D.Impulse);
        }

        public void AirJump()
        {
            rb.linearVelocityY = 0;
            rb.AddForce(Vector2.up * jumpInfo.jumpForce * 0.6f, ForceMode2D.Impulse);
        }
        public void StopJumpin()
        {
            if (rb.linearVelocityY > 0.1f)
            {
                Debug.Log("Reduzing jump speed");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocityY * jumpInfo.jumpStopForce);
            }
        }




        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(groundCheck.transform.position, new Vector2(groundCheck.checkRadius, groundCheck.checkHeight));
        }


        #endregion
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
            var trigger_jump = stateMachine.inputManager.trigger_jump;

            stateMachine.Movement_Free();

            if (trigger_jump.trigger)
            {
                if (stateMachine.groundCheck.isGrounded
                    ||
                    trigger_jump.graceTime_coyoteTime + trigger_jump.timeStamp > Time.time)
                {
                    trigger_jump.Deactivate();
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
            stateMachine.Jump();
        }

        public override void Tick_State(PlayerMovement_StateMachine stateMachine)
        {
            Debug.Log("Tick: Jump");
            var trigger_stopJump = stateMachine.inputManager.trigger_stopJumping;
            var trigger_jump = stateMachine.inputManager.trigger_jump;

            stateMachine.Movement_Free();

            if (!stateMachine.groundCheck.isGrounded)
            {
                if (trigger_stopJump.trigger)
                {
                    trigger_stopJump.Deactivate();
                    stateMachine.StopJumpin();
                }
                else if (trigger_jump.trigger)
                {
                    trigger_jump.Deactivate();
                    stateMachine.Set_State(new Air_Jump_State(stateMachine));
                }
                return;
            }
            if (timeStamp < Time.time)
            {
                stateMachine.Set_State(new Idle_State(stateMachine));
            }

        }
    }

    public class Air_Jump_State : State<PlayerMovement_StateMachine>
    {
        public float timeStamp = 0;

        public Air_Jump_State(PlayerMovement_StateMachine stateMachine) : base(stateMachine)
        {
            Debug.Log(ccolor.cyan("Changing State: Air Jump"));
            timeStamp = Time.time;
            stateMachine.AirJump();
        }
        public override void Tick_State(PlayerMovement_StateMachine stateMachine)
        {
            Debug.Log("Tick: Air Jump");
            var trigger_stopJump = stateMachine.inputManager.trigger_stopJumping;

            stateMachine.Movement_Free();

            if (!stateMachine.groundCheck.isGrounded)
            {
                if (trigger_stopJump.trigger)
                {
                    trigger_stopJump.Deactivate();
                    stateMachine.StopJumpin();
                }
                return;
            }

            stateMachine.Set_State(new Idle_State(stateMachine));

        }
    }

}
