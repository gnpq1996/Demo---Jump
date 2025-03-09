using UnityEngine;

namespace Demo.InputManagers
{
    [System.Serializable]
    public class InputTrigger
    {
        [field: SerializeField] public bool trigger { get; protected set; } = false;
        [field: SerializeField] public float timeStamp { get; protected set; } = 0;
        [field:SerializeField, Min(0.01f)] public float graceTime { get; protected set; } = 0.3f;

        public virtual void Activate()
        {
            SetTrigger(true, Time.time);
        }

        public virtual void Deactivate()
        {
            SetTrigger(false, 0);
        }
        bool SetTrigger(bool activeTrigger, float timeStamp)
        {
            this.timeStamp = timeStamp;
            trigger = activeTrigger;
            return true;
        }

        public virtual bool ResetTrigger()
        {
            if (timeStamp + graceTime < Time.time)
            {
                trigger = false;
                return true;
            }

            return false;
        }
    }

}
