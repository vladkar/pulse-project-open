using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Agents;

namespace Pulse.MultiagentEngine.Containers
{
    public class AgentRegistry : ICollection<AgentBase>
    {
        public class AgentAdditionEventArgs
        {
            public IEnumerable<AgentBase> NewAgents { get; private set; }

            public AgentAdditionEventArgs(IEnumerable<AgentBase> newAgents)
            {
                NewAgents = newAgents;
            }
        }

        public class AgentRemoveEventArgs
        {
            public AgentBase RemovedAgent { get; private set; }

            public AgentRemoveEventArgs(AgentBase newAgents)
            {
                RemovedAgent = newAgents;
            }
        }

        // Declare the delegate (if using non-generic pattern).
        public delegate void AgentAdditionEventHandler(object sender, AgentAdditionEventArgs e);
        // Declare the delegate (if using non-generic pattern).
        public delegate void AgentRemoveEventHandler(object sender, AgentRemoveEventArgs e);

        // Declare the event.
        public event AgentAdditionEventHandler AgentAdditionEvent;
        public event AgentRemoveEventHandler AgentRemoveEvent;

        /// <summary>
        /// Id index
        /// </summary>
        private IDictionary<long, AgentBase> IndexById = new Dictionary<long, AgentBase>();

        public IEnumerator<AgentBase> GetEnumerator()
        {
            return IndexById.Values.GetEnumerator();
        }

        public IEnumerable<T> OfType<T>()
        {
            return IndexById.Values.OfType<T>();
        }

        public void Add(AgentBase item)
        {
            IndexById.Add(item.Id, item);

            if (AgentAdditionEvent != null)
                AgentAdditionEvent(this, new AgentAdditionEventArgs(new List<AgentBase>() { item }));
        }

        public void AddRange(IEnumerable<AgentBase> items)
        {
            foreach (var item in items)
            {
                IndexById.Add(item.Id, item);
            }
            if (AgentAdditionEvent != null)
                AgentAdditionEvent(this, new AgentAdditionEventArgs(items));
        }

        public void CopyTo(AgentBase[] array, int arrayIndex)
        {
            IndexById.CopyTo(array.Select(v => new KeyValuePair<long, AgentBase>(v.Id, v)).ToArray(), arrayIndex);
        }

        public bool Remove(AgentBase item)
        {
            var ret = IndexById.Remove(item.Id);

            if (AgentRemoveEvent != null)
                AgentRemoveEvent(this, new AgentRemoveEventArgs(item));

            return ret;
        }

        public AgentBase GetById(long id)
        {
            return IndexById[id];
        }

        public bool Contains(AgentBase item)
        {
            return IndexById.ContainsKey(item.Id);
        }

        /// <summary>
        /// Faster than Contains(AgentBase)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(long id)
        {
            return IndexById.ContainsKey(id);
        }

        public void Clear()
        {
            IndexById.Clear();
        }

        public int Count
        {
            get { return IndexById.Count; }
        }

        public bool IsReadOnly
        {
            get { return IndexById.IsReadOnly; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // TODO My extras

//        public ICollection<AgentBase> GetAgents()
//        {
//            return IndexById.Values;
//        }
//
//        public ICollection<AgentBase> Values { get { return IndexById.Values; } }
//
//        public AgentBase[] AgentArray;
//
//        public AgentBase[] GetAgentArray()
//        {
//            if (AgentArray == null || AgentArray.Length < 1000)
//            {
//                AgentArray = IndexById.Values.ToArray();
//            }
//
//            return AgentArray;
//        }
    }
}
