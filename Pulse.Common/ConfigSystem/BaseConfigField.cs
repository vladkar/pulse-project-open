namespace Pulse.Common.ConfigSystem
{
    public class BaseConfigField<T>
    {
        public T Value { set; get; }

        public BaseConfigField(T val)
        {
            Value = val;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
