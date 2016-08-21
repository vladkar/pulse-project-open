using System;

namespace Pulse.Common.ConfigSystem
{
    public class BaseScenarioConfig
    {
        public BaseConfigField<string> Name { set; get; }

        protected T GetSingleton<T>(T src, Lazy<T> newInst, Func<T, bool> isOk = null)
        {
            if (isOk == null)
                isOk = (T obj) => obj != null;

            if (isOk(src)) return src;
            src = newInst.Value;
            return src;
        }
    }
}
