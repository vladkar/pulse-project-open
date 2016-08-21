using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Pulse.Common.Scenery.Objects;
using Pulse.MultiagentEngine.Map;

namespace NavigField
{
    public class NavfieldCalc : INavfieldCalc
    {
        public NavigationFieldSpace NF;

        public NavfieldCalc()
        {
            NF = new NavigationFieldSpace();
        }

        public void Load(double toPulseVector2Multiplier, IList<PulseVector2[]> obstacles, PulseVector2[] initPoints, PulseVector2[] customBorder = null)
        {
            var maxx = obstacles.Max(o => o.Max(p => p.X));
            var maxy = obstacles.Max(o => o.Max(p => p.Y));

            var minx = obstacles.Min(o => o.Min(p => p.X));
            var miny = obstacles.Min(o => o.Min(p => p.Y));

            int safetyDistanceToBorder = 2;
            NF.safetyDistanceToBorder = safetyDistanceToBorder;

            NF = new NavigationFieldSpace((int)(Math.Round((maxx - minx) / toPulseVector2Multiplier) + 2 * safetyDistanceToBorder),
                                          (int)(Math.Round((maxy - miny) / toPulseVector2Multiplier)) + 2 * safetyDistanceToBorder);

            NF._offset = new PulseVector2(minx - safetyDistanceToBorder * toPulseVector2Multiplier,
                                    miny - safetyDistanceToBorder * toPulseVector2Multiplier);

            NF.LoadInitData(toPulseVector2Multiplier, obstacles, initPoints, customBorder);

        }
        public void CalculateField(PulseVector2 aimPoint, PulseVector2[] path, int iterToPreciseAngles)

        {
                PulseVector2 prev = default(PulseVector2);

                foreach (var e in path)
                {

                    if (prev == default(PulseVector2))
                    {
                        prev = e;

                        continue;
                    }

                    NF.SetLineGuid((prev.X - NF._offset.X) / NF._toPulseVector2Multiplier, (prev.Y - NF._offset.Y) / NF._toPulseVector2Multiplier,
                        (e.X - NF._offset.X) / NF._toPulseVector2Multiplier, (e.Y - NF._offset.Y) / NF._toPulseVector2Multiplier, 0.99);
                    prev = e;

                }
       

            var last = aimPoint;
//            last = path.Last<PulseVector2>();
            int lastX = (int)Math.Round((last.X - NF._offset.X) / NF._toPulseVector2Multiplier);
            int lastY = (int)Math.Round((last.Y - NF._offset.Y) / NF._toPulseVector2Multiplier);

            if ((0 <= lastX) && (lastX < NF.NavigFieldArray.GetLength(0)) &&
                                (0 <= lastY) && (lastY < NF.NavigFieldArray.GetLength(1)))
            
                if (NF.NavigFieldArray[lastX, lastY].isActive
                    &&
                    (!NF.NavigFieldArray[lastX, lastY].isObstacle
                    ||
                    NF.NavigFieldArray[lastX, lastY].isAim))                
                    NF.CalculateFieldForAim(lastX, lastY, iterToPreciseAngles);
            
        }

        public int CalcFieldForOneAim(PulseVector2 aimPoint, int iterToPreciseAngles, double preferredVelocity = 1)
        {
//            var p = (aimPoint.X - NF._offset.X) / NF._toPulseVector2Multiplier, (aimPoint.Y - NF._offset.Y) / NF._toPulseVector2Multiplier
            NF.CalculateFieldForAim((int)Math.Round((aimPoint.X - NF._offset.X) / NF._toPulseVector2Multiplier), (int)Math.Round((aimPoint.Y - NF._offset.Y) / NF._toPulseVector2Multiplier), iterToPreciseAngles, preferredVelocity);
            return 0;
        }

        private Random _r = new Random();
        public PulseVector2 GetVelocity(PulseVector2 point)
        {
            var vect = point;

//            var t1 = (point.X - NF._offset.X)/NF._toPulseVector2Multiplier;
//            var t2 = (point.Y - NF._offset.Y)/NF._toPulseVector2Multiplier;
//
//            var el = NF.NavigFieldArray[(int) (Math.Round((point.X - NF._offset.X)/NF._toPulseVector2Multiplier)),
//                (int) (Math.Round((point.Y - NF._offset.Y)/NF._toPulseVector2Multiplier))];

            double amplitude = NF.NavigFieldArray[(int)(Math.Round((point.X - NF._offset.X ) / NF._toPulseVector2Multiplier)),
                               (int)(Math.Round((point.Y - NF._offset.Y) / NF._toPulseVector2Multiplier))].velocity;
            double angle = NF.NavigFieldArray[(int)(Math.Round((point.X - NF._offset.X) / NF._toPulseVector2Multiplier)),
                               (int)(Math.Round((point.Y - NF._offset.Y) / NF._toPulseVector2Multiplier))].angle;

            vect.X = amplitude * Math.Cos(angle);
            vect.Y = amplitude * Math.Sin(angle);

            return vect;
        }

    }
}
