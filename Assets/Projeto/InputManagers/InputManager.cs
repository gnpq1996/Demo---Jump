using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.InputManagers
{
    public abstract class InputManager : MonoBehaviour
    {
        [SerializeField] protected bool active = true;

        public virtual void TurnOn_Inputs()
        {
            active = true;
        }
        public virtual void TurnOff_Inputs()
        {
            active = false;
        }

        public abstract void Reset_Triggers();
    }
}
