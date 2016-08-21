using System;
using Pulse.Common.Utils;

namespace Pulse.Common.Model.AgentScheduling.Current
{
    public abstract class CurrentActivity
    {
        public int Id { set; get; }
        public DateTime RealStartTime { get; set; }
        public DateTime RealEndTime { get; set; }

        public ActivityState State { set; get; }

        public CurrentActivity Prev { set; get; }
        public CurrentActivity Next { set; get; }
        public CurrentActivity Child { set; get; }
        public CurrentActivity Parent { set; get; }

        /*
         *    Activity tree example  
         *    
         *    (A1)---(A2)----------(A5)
         *             |
         *           (A3)---(A4)
         *           
         *    A2 is Next for A1
         *    A1 is Prev for A2
         *    A2 is Parent for A3
         *    A3 is child for A2
         *    
         *    Execution order: A1-A2-A3-A4-A5
         * 
         */

        protected CurrentActivity()
        {
            //Id = IdUtil.NextRandomId();
        }

        public void Finish()
        {
            OnOnFinish();
            State = ActivityState.Finished;
        }

        public delegate void OnFinishDelegate();
        public event OnFinishDelegate OnFinish;

        protected virtual void OnOnFinish()
        {
            OnFinishDelegate handler = OnFinish;
            if (handler != null) handler();
        }

        public bool IsChildrenFinish()
        {
            return IsChildrenFinishRec(this);
        }

        private bool IsChildrenFinishRec(CurrentActivity currentActivity)
        {
            var done = true;

            if (currentActivity.Next == null & currentActivity.Child == null)
                return currentActivity.State == ActivityState.Finished;

            if (currentActivity.Next != null)
                done = done & IsChildrenFinishRec(currentActivity.Next);

            if (currentActivity.Child != null)
                done = done & IsChildrenFinishRec(currentActivity.Child);

            return done;
        }
    }
}