using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.Converter.Attribute;
using Fusion.Engine.Graphics.GIS;

namespace City.UIFrames.Converter.Models
{
    [ClassAttribute("Sentiment")]
    public class SentimentControlUI
    {
        private PointsGisLayer globePosts;

       
        public SentimentControlUI(PointsGisLayer globePosts)
        {
            this.globePosts = globePosts;
        }

        [Checkbox("visible")]
        public void IsVisible()
        {
            this.globePosts.IsVisible = !this.globePosts.IsVisible;
        }

        [Checkbox("visible")]
        public void IsVisible1()
        {
            this.globePosts.IsVisible = !this.globePosts.IsVisible;
        }

        //action on mouse
        //public void onMouseClick()
        //{
        //    if (!this.globePosts.IsVisible) return;
        //    DVector2 mousePosition;
        //    MasterLayer.GlobeCamera.ScreenToSpherical(Game.Mouse.Position.X, Game.Mouse.Position.Y, out mousePosition);

        //    for (int i = 0; i < globePosts.PointsCount; i++)
        //    {
        //        var post = globePosts.PointsCpu[i];
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
            for (int i = 0; i < this.globePosts.PointsCount; i++)
            {
                this.globePosts.PointsCpu[i].Tex0.Z = size;
            }
            this.globePosts.UpdatePointsBuffer();
        }

        //update alpha, sets one transparency value to all points
        public void updateAlpha(float alpha)
        {
            for (int i = 0; i < this.globePosts.PointsCount; i++)
            {
                this.globePosts.PointsCpu[i].Color.Alpha = alpha;
            }
            this.globePosts.UpdatePointsBuffer();
        }
    }
}
