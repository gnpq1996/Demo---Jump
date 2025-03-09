using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.InputManagers
{
    public class PlayerMovement_InputManager : InputManager
    {
        [SerializeField] private Vector2 inputDirection;

        [Header("Inputs")]
        [field:SerializeField] public bool input_jump { get; private set; } = false;

        [Header("Triggers")]
        [field: SerializeField] public InputTrigger_Jump trigger_jump { get; private set; }
        [field: SerializeField] public InputTrigger trigger_stopJumping { get; private set; }

        private void Update()
        {
            Reset_Triggers();
        }

        public override void Reset_Triggers()
        {
            trigger_jump.ResetTrigger();
            trigger_stopJumping.ResetTrigger();
        }

        #region InputHandlers
        //====================================================================================================================
        public void InputHandler_Movement(InputAction.CallbackContext context)
        {
            if (!active) return;
            inputDirection = context.ReadValue<Vector2>();
        }

        public void InputHandler_Jump(InputAction.CallbackContext context)
        {
            if (!active) return;

            if (context.started)
            {
                //Debug.Log("Input: Jump");
                trigger_jump.Activate();
                input_jump = true;
            }
            else if (context.canceled)
            {
                //Debug.Log("Input: Stop jumping");
                trigger_stopJumping.Deactivate();
                input_jump = false;
            }
        }




        #endregion
    }

    #region Inputs
    //====================================================================================================================
    [System.Serializable]
    public class InputTrigger_Jump : InputTrigger
    {
        [field: SerializeField, Min(0.01f)] public float graceTime_coyoteTime { get; protected set; } = 0.3f;
    }


    #endregion
}
