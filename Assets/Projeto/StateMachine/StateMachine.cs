using UnityEngine;

namespace Demo.StateMachines
{
    public abstract class StateMachine : MonoBehaviour
    {
        State previousState;
        State currentState;

        private void Update()
        {
            Tick();
        }
        public virtual void Tick()
        {
           if(currentState != null) currentState.Tick(previousState);
        }
    }
}
