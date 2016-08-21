using System;
using System.Collections.Generic;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.Poi
{
    public interface IQueueableObject<T>
    {
        Queue<T> Queue { set; get; }
        T CurrentObject { get; }
        DateTime CurrentObjectStartProcessingTime { get; }
        DateTime LastObjectCompleteProcessingTime { get; }
        bool IsDoneCurrentObect(double timeStep, double time, DateTime geotime);
        void QueueIteration(double timeStep, double time, DateTime geotime);
    }

    public abstract class AbstractQueueablePoi : AbstractInteractivePointOfInterest,
        IQueueableObject<AbstractPulseAgent>
    {
        public Queue<AbstractPulseAgent> Queue { get; set; }
        public AbstractPulseAgent CurrentObject { set; get; }
        public DateTime CurrentObjectStartProcessingTime { get; set; }
        public DateTime LastObjectCompleteProcessingTime { get; set; }
        public abstract TimeSpan GetCurrentClientProcessTime();
        public abstract void QueueIteration(double timeStep, double time, DateTime geotime);
        public abstract bool IsDoneCurrentObect(double timeStep, double time, DateTime geotime);

        protected AbstractQueueablePoi() : base()
        {
            Initialize();
        }

        protected AbstractQueueablePoi(IPointOfInterest poi) : base(poi)
        {
            Initialize();
        }

        private void Initialize()
        {
            Queue = new Queue<AbstractPulseAgent>();
        }
    }

    public class SimpleQueueablePoi : AbstractQueueablePoi
    {
        protected TimeSpan _currentObjectProcessingTime;

        public SimpleQueueablePoi() : base()
        {
            Initialize();
        }

        public SimpleQueueablePoi(IPointOfInterest poi) : base(poi)
        {
            Initialize();
        }

        private void Initialize()
        {
            _currentObjectProcessingTime = new TimeSpan();
        }

        public override void Enter(AbstractPulseAgent agent)
        {
            base.Enter(agent);
            Queue.Enqueue(agent);
        }

        public override void Exit(AbstractPulseAgent agent)
        {
            base.Exit(agent);
            Queue.Dequeue();
        }

        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (Queue.Count <= 0) return;

            if (CurrentObject == null)
            {
                CurrentObject = Queue.Peek();
                _currentObjectProcessingTime = GetCurrentClientProcessTime();
                CurrentObjectStartProcessingTime = geotime;
            }

            if (IsDoneCurrentObect(timeStep, time, geotime))
            {
                CurrentObject.DoneActivity();
                Exit(CurrentObject);
                CurrentObject = null;
            }
        }

        public override TimeSpan GetCurrentClientProcessTime()
        {
            return new TimeSpan(0, RandomUtil.RandomInt(0, 3), RandomUtil.RandomInt(0, 59));
        }

        public override bool IsDoneCurrentObect(double timeStep, double time, DateTime geotime)
        {
            return geotime > CurrentObjectStartProcessingTime + _currentObjectProcessingTime;
        }

        public override void QueueIteration(double timeStep, double time, DateTime geotime)
        {
            throw new NotImplementedException();
        }
    }

    public class QueueablePoi : AbstractInteractivePointOfInterest
    {
        public Queue<AbstractPulseAgent> Queue { set; get; }

        protected DateTime _currentProcessStartTime;
        protected TimeSpan _processTime;
        protected AbstractPulseAgent _currentAgent;

        public QueueablePoi() : base()
        {
            Queue = new Queue<AbstractPulseAgent>();
        }

        public QueueablePoi(IPointOfInterest poi) : base(poi)
        {
            Queue = new Queue<AbstractPulseAgent>();
        }

//        public override void Enter(AbstractPulseAgent agent)
//        {
//            base.Enter(agent);
//            Queue.Enqueue(agent);
//        }

        public virtual void Enqueue(AbstractPulseAgent agent)
        {
            Queue.Enqueue(agent);
        }

        public virtual AbstractPulseAgent Dequeue()
        {
            return Queue.Dequeue();
        }

        //        public override void Exit(AbstractPulseAgent agent)
        //        {
        //            base.Exit(agent);
        //            Queue.Dequeue();
        //        }

        //queueable update
        //TODO extract queueable object from poi as separate queue (which can be used for many pois)
        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (_currentAgent == null)
            {
                //queue complete
                if (Queue.Count <= 0) return;

                _currentAgent = Dequeue();
                _currentAgent.DoneActivity();
                _processTime = GetNextClientProcessTime();
                _currentProcessStartTime = geotime;
                
            }

            //passive action complete
            else if (geotime >= _currentProcessStartTime + _processTime)
            {
                Exit(_currentAgent);
                _currentAgent.DoneActivity();
                _currentAgent = null;

            }
        }

        protected virtual TimeSpan GetNextClientProcessTime()
        {
            return new TimeSpan(0, RandomUtil.RandomInt(0, 0), RandomUtil.RandomInt(5, 10));
        }
    }

    public class SpatialQueueablePoi : QueueablePoi
    {
        public PulseVector2[] QueueRoute { set; get; }
        public double _queueLength;
        public Queue<double> DistancQueue { set; get; } 

        private double _bodyRadius = 1;

        public SpatialQueueablePoi() : base()
        {
            DistancQueue = new Queue<double>();
        }

        public SpatialQueueablePoi(IPointOfInterest poi) : base(poi)
        {
            DistancQueue = new Queue<double>();
        }

        public override void Update(double timeStep, double time, DateTime geotime)
        {
            base.Update(timeStep, time, geotime);
        }

        public PulseVector2 GetVaccancyPoint()
        {
            var accLength = 0d;

            for (int i = 0; i < QueueRoute.Length - 1; i++)
            {
                accLength += QueueRoute[i].DistanceTo(QueueRoute[i + 1]);

                var d = accLength - (_queueLength + _bodyRadius);

                if (d >= 0)
                {
                    return ClipperUtil.GetDistancePointOnGgment(QueueRoute[i], QueueRoute[i + 1], d);
                }
            }

            throw new Exception("Cant define next vaccant queue spatial position");
        }

        public PulseVector2 AskMyPosition(AbstractPulseAgent agent)
        {
            //todo this and simple movement system

            var agentIndex = Queue.IndexOf(agent);

            return CalculatedPositions[agentIndex];
        }

        public override void Enqueue(AbstractPulseAgent agent)
        {
            base.Enqueue(agent);
            _queueLength += _bodyRadius;


            //todo config
            var bodyRadius = 1;

            if (DistancQueue.Count == 0)
                DistancQueue.Enqueue(bodyRadius + RandomUtil.RandomDouble() * 0.5);
            else
                DistancQueue.Enqueue(bodyRadius*2 + RandomUtil.RandomDouble()*0.5);

            RecalcPositions();
        }

        public override AbstractPulseAgent Dequeue()
        {
            _queueLength -= _bodyRadius;

            DistancQueue.Dequeue();
            var agent = base.Dequeue();
            RecalcPositions();

            return agent;
        }

        public void RecalcPositions()
        {
            CalculatedPositions = new PulseVector2[Queue.Count];

            var accumDist = 0d;
            var i = 0;
            foreach (var interval in DistancQueue)
            {
                accumDist += interval;
                var rawPoint = ClipperUtil.GetDistancePointOnGgment(QueueRoute[0], QueueRoute[1], accumDist);
                CalculatedPositions[i] = new PulseVector2(rawPoint.X + RandomUtil.RandomDouble() - 0.5, rawPoint.Y + RandomUtil.RandomDouble() - 0.5);
                i++;
            }
        }

        public PulseVector2[] CalculatedPositions { get; set; }
    }
}