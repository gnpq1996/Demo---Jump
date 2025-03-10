
namespace Demo.StateMachines
{
    public abstract class State<T> where T : StateMachine
    {
        public State(T stateMachine)
        {
           
        }
        public virtual void Tick_State(T stateMachine)
        {

        }
    }
}



