using System;
using System.Collections.Generic;

namespace Pulse.Common.Utils
{
    public static class IdUtil
    {

        static IdUtil()
        {
            _channel = new Dictionary<int, int>();
        }

        #region counter Id

        private static int _idCount = 0;
        private static Object _locker = new object();
        private static IDictionary<int, int> _channel; 

        public static int NextId()
        {
            lock (_locker)
            {
                return ++_idCount;
            }
        }

        #endregion

        #region random Id

        private static HashSet<int> _usedId = new HashSet<int>();

        public static int NextRandomId()
        {
            var nextId = RandomUtil.RandomInt(100000, 999999);

            while (_usedId.Contains(nextId))
            {
                nextId = RandomUtil.RandomInt(100000, 999999);
            }

            _usedId.Add(nextId);
            return nextId;
        }

        #endregion
    }

    public class IdUtil2
    {
        #region counter Id

        private int _idCount = 0;
        private Object _locker = new object();

        public int NextId()
        {
            lock (_locker)
            {
                return ++_idCount;
            }
        }

        #endregion

    }
}