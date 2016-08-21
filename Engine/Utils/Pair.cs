namespace Pulse.MultiagentEngine.Utils
{
    public class Pair<TF, TS>
    {
        private TF _first;
        private TS _second;

        public Pair(TF first, TS second)
        {
            _first = first;
            _second = second;
        }

        public TF First
        {
            get { return _first; }
            set { _first = value; }
        }

        public TS Second
        {
            get { return _second; }
            set { _second = value; }
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj is Pair<F, S>)
        //    {
        //        var t = obj as Pair<F, S>;
        //        return First == t.First && Second == t.Second;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}