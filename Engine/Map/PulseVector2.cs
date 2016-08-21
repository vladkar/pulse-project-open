using System;

namespace Pulse.MultiagentEngine.Map
{
    [Serializable]
    public struct PulseVector2
    {

        public PulseVector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public double DistanceTo(PulseVector2 B)
        {
            return Math.Sqrt((X - B.X) * (X - B.X) + (Y - B.Y) * (Y - B.Y));
        }

        public double DistanceSquared(PulseVector2 B)
        {
            return (X - B.X) * (X - B.X) + (Y - B.Y) * (Y - B.Y);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public static bool operator ==(PulseVector2 left, PulseVector2 right)
        {
            return Equals(left, right);
        }

        public static PulseVector2 operator +(PulseVector2 left, PulseVector2 right)
        {
            return new PulseVector2(left.X + right.X, left.Y + right.Y);
        }

        public static PulseVector2 operator -(PulseVector2 left, PulseVector2 right)
        {
            return new PulseVector2(left.X - right.X, left.Y - right.Y);
        }

        public static PulseVector2 operator *(PulseVector2 left, double right)
        {
            return new PulseVector2(left.X*right, left.Y*right);
        }

        public static bool operator !=(PulseVector2 left, PulseVector2 right)
        {
            return !Equals(left, right);
        }

        public PulseVector2 Normalize()
        {
            var l = Math.Sqrt(X * X + Y * Y);
            if (l == 0.0d)
                return new PulseVector2(0, 0);
            return new PulseVector2(X / l, Y / l);
        }

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        private const double DegToRad = Math.PI / 180;

        public PulseVector2 Rotate(double degrees)
        {
            return RotateRadians(degrees * DegToRad);
        }

        public PulseVector2 RotateRadians(double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new PulseVector2(ca * this.X - sa * this.Y, sa * this.X + ca * this.Y);
        }

        public bool Equals(PulseVector2 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Y.Equals(Y) && other.X.Equals(X);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PulseVector2)) return false;
            return Equals((PulseVector2) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Y.GetHashCode()*397) ^ X.GetHashCode();
            }
        }

        public PulseVector2 Clone()
        {
            return new PulseVector2(X, Y);
        }
    }
//
//    public class PulseVector2 : PulseVector2
//    {
//        public PulseVector2(PulseVector2 c)
//        {
//            X = c.X;
//            Y = c.Y;
//        }
//
//        public PulseVector2(double x, double y)
//            : base(x, y)
//        {
//        }
//
//        public static PulseVector2 operator -(PulseVector2 A, PulseVector2 B)
//        {
//            return new PulseVector2(A.X - B.X, A.Y - B.Y);
//        }
//
//        public static PulseVector2 operator +(PulseVector2 A, PulseVector2 B)
//        {
//            return new PulseVector2(A.X + B.X, A.Y + B.Y);
//        }
//
//        public PulseVector2 Normalize()
//        {
//            var l = Math.Sqrt(X * X + Y * Y);
//            if (l == 0.0d)
//                return new PulseVector2(0, 0);
//            return new PulseVector2(X / l, Y / l);
//        }
//
//        public double Magnitude()
//        {
//            return Math.Sqrt(X * X + Y * Y);
//        }
//    }
}
