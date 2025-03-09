
namespace Demo.StateMachines
{
    public abstract class State
    {
        public abstract void Tick(State previousState);
    }
}

