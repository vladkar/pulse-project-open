using System;
using NLog;

namespace Pulse.Common.Scenery.Loaders
{
    public abstract class AbstractDataBroker
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        public string Name { get; protected set; }
        public bool IsLoaded { get; protected set; }

        
        public delegate void OnInitializationCompleteDelegate();
        public event OnInitializationCompleteDelegate OnInitializationComplete;

        protected AbstractDataBroker()
        {
            IsLoaded = false;
            Name = "UNDEFINED";
        }

        public virtual void Initialize()
        {
            if (IsLoaded) return;

            Log.Info(string.Format("Loading: {0}", ToString()));
#if !DEBUG
            try
            {
#endif
            LoadData();
            OnInitializationCompleteInvoke();
            IsLoaded = true;
#if !DEBUG
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Loading error: {0}. Exception: {1}, inner {2}", ToString(), e.Message, e.InnerException == null ? "null": e.InnerException.Message));
                throw;
            }
#endif
            Log.Info(string.Format("Loading success: {0}", ToString()));
        }

        protected void OnInitializationCompleteInvoke()
        {
            var handler = OnInitializationComplete;
            if (handler != null) handler();
        }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        protected abstract void LoadData();
    }
}