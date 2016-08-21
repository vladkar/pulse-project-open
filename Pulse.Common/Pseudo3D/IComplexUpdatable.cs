using System;

namespace Pulse.Common.Pseudo3D
{
    public interface IComplexUpdatable
    {
        void Update(double timeStep, double time, DateTime geotime);
    }
}