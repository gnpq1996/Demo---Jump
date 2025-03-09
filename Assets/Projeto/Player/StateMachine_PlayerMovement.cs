using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.StateMachines
{
    public class StateMachine_PlayerMovement : StateMachine
    {

    }

    public class Movement : State
    {
        public override void Tick(State previousState)
        {
            throw new System.NotImplementedException();
        }
    }
}
