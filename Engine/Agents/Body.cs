using System;
using Pulse.MultiagentEngine.Map;

namespace Pulse.MultiagentEngine.Agents
{
    /// <summary>
    /// Fundamental properties
    /// </summary>
    public class Body
    {
        public ShapeBase Shape { get; set; }
        public double Weight { get; set; }

        public double Radius
        {
            get { return Shape.Radius; }
        }

    }

    public abstract class ShapeBase
    {
        public abstract double Radius { get; }
    }

    public class CircleShape : ShapeBase
    {
        private readonly double _radius;
        public override double Radius
        {
            get { return _radius; }
        }

        public CircleShape(double radius)
        {
            _radius = radius;
        }
    }

    class RectangularShape : ShapeBase
    {
        public PulseVector2 Direction { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public override double Radius
        {
            get { return Math.Sqrt(Width * Width + Height * Height) / 2.0; }
        }
    }

}
