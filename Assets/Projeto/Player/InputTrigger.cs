using UnityEngine;

namespace Demo.Inputs
{
    [System.Serializable]
    public class InputTrigger
    {
        [field: SerializeField] public bool trigger { get; protected set; } = false;
        [field: SerializeField] public float timeStamp { get; protected set; } = 0;
        [field:SerializeField, Min(0.01f)] public float graceTime { get; protected set; } = 0.3f;
        public virtual bool SetTrigger(float timeStamp)
        {
            this.timeStamp = timeStamp;
            trigger = true;
            return true;
        }

        public virtual bool ResetTrigger()
        {
            trigger = false;
            return true;
        }

        public virtual void ForceReset()
        {
            trigger = false;
        }
    }

    [System.Serializable]
    public class InputTrigger_Jump : InputTrigger
    {
        [field: SerializeField, Min(0.01f)] public float graceTime_coyoteTime { get; protected set; } = 0.3f;

        public override bool ResetTrigger()
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
