using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.Converter.Attribute;
using Fusion.Engine.Graphics.GIS;

namespace City.UIFrames.Converter.Models
{
    [Class("Calls data")]
    public class CallsDataControlUI
    {
        private PointsGisLayer _globeCalls;
        private List<PolyGisLayer> _morningPolygons;
        private LinesGisLayer _morningBounds;
        private int currentNumber = 0;
        private bool IsStart = true;
        private List<List<PolyGisLayer>> _polygonIntervals;
        private List<LinesGisLayer> _boundsIntervals; 

        public CallsDataControlUI(PointsGisLayer globeCalls ,List<PolyGisLayer> morning,
            List<List<PolyGisLayer>> polygonIntervals,List<LinesGisLayer> boundsIntervals)
        {
            this._globeCalls = globeCalls;
            _morningPolygons = morning;
            _polygonIntervals = polygonIntervals;
            _boundsIntervals = boundsIntervals;
        }

        /*
        [Checkbox("Calls visible")]
        public void IsVisible()
        {
            this._globeCalls.IsVisible = !this._globeCalls.IsVisible;
        }

        [Checkbox("Morning data")]
        public void IsVisible_1()
        {
            foreach (var morningPolygon in _morningPolygons)
            {
                morningPolygon.IsVisible = !morningPolygon.IsVisible;
            }
        }
        */

        [Button("Next interval")]
        public void NextInterval()
        {
            if (currentNumber < _polygonIntervals.Count - 1)
            {
                currentNumber += 1;
            }

            for (int i = 0; i < _polygonIntervals.Count; i++)
            {
                foreach (var layer in _polygonIntervals[i])
                {
                    layer.IsVisible = (i==currentNumber);
                }
            }

            for (int i = 0; i < _boundsIntervals.Count; i++)
            {
                _boundsIntervals[i].IsVisible = i == currentNumber;
            }

            
        }

        [Button("Previous interval")]
        public void PreviousInterval()
        {
            if (currentNumber > 0)
            {
                currentNumber -= 1;
            }

            for (int i = 0; i < _polygonIntervals.Count; i++)
            {
                foreach (var layer in _polygonIntervals[i])
                {
                    layer.IsVisible = (i == currentNumber);
                }
            }

            for (int i = 0; i < _boundsIntervals.Count; i++)
            {
                _boundsIntervals[i].IsVisible = i == currentNumber;
            }

            
        }


        //action on mouse
        //public void onMouseClick()
        //{
        //    if (!this.globeCalls.IsVisible) return;
        //    DVector2 mousePosition;
        //    MasterLayer.GlobeCamera.ScreenToSpherical(Game.Mouse.Position.X, Game.Mouse.Position.Y, out mousePosition);

        //    for (int i = 0; i < globeCalls.PointsCount; i++)
        //    {
        //        var post = globeCalls.PointsCpu[i];
        //        var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));

        //        if (distance < post.Tex0.Z)
        //        {
        //            Log.Message("" + Posts[i].SentimentValue);
        //        }
        //    }
        //}

        //update size
        public void updateSize(float size)
        {
            for (int i = 0; i < this._globeCalls.PointsCount; i++)
            {
                this._globeCalls.PointsCpu[i].Tex0.Z = size;
            }
            this._globeCalls.UpdatePointsBuffer();
        }

        //update alpha, sets one transparency value to all points
        public void updateAlpha(float alpha)
        {
            for (int i = 0; i < this._globeCalls.PointsCount; i++)
            {
                this._globeCalls.PointsCpu[i].Color.Alpha = alpha;
            }
            this._globeCalls.UpdatePointsBuffer();
        }
    }
}
