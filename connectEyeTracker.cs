using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

using EyeXFramework;
using Tobii.EyeX.Framework;
using System.Windows.Threading;
using Tobii.Gaze.Core;

namespace Eyeplayer
{
    public partial class connectEyeTracker
    {
        public Point eyePoint;
        public Point SmoothPoint;
        private Point gazePoint;
        public void start()
        {
            Global.eyeXHost = new EyeXHost();
            Global.eyeXHost.EyeTrackingDeviceStatusChanged += new EventHandler<EngineStateValue<EyeTrackingDeviceStatus>>(TrackingDeviceStatus);
            Global.eyeXHost.GazeTrackingChanged += new EventHandler<EngineStateValue<GazeTracking>>(GazeTrackingStatus);
            Global.eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered).Next += (s, e) =>
            {
                gazePoint = new Point((int)e.X, (int)e.Y);
                OnGazePoint();
            };
            Global.trackerStatus = Global.eyeTrackerStatus.Tracking;
            Global.eyeXHost.Start();
        }
        private void TrackingDeviceStatus(object sender, EventArgs e)
        {
            switch (e.ToString())
            {
                case "Tracking":
                    Global.trackerStatus = Global.eyeTrackerStatus.Tracking;
                    break;
                case "Configuring":
                    Global.trackerStatus = Global.eyeTrackerStatus.Configuring;
                    break;
                case "Initializing":
                    Global.trackerStatus = Global.eyeTrackerStatus.Initializing;
                    break;
            }
        }

        private void GazeTrackingStatus(object sender, EventArgs e)
        {
            if (e.ToString() == "GazeNotTracked") Global.isGotGaze = false;
            if (e.ToString() == "GazeTracked") Global.isGotGaze = true;
        }

        Point mousePos = new Point(0, 0);
        DateTime mouseMovieEndTime;
        DateTime watchEndTime;
        private void OnGazePoint()
        {
            if (Global.testMode)
            {
                Global.isWatching = true;
                SmoothPoint = new Point(Cursor.Position.X, Cursor.Position.Y);
                Global.SmoothPoint = SmoothPoint;
                if (mousePos != SmoothPoint)
                {
                    mousePos = new Point(Cursor.Position.X, Cursor.Position.Y);
                    mouseMovieEndTime = DateTime.Now;
                }
                else
                {
                    if ((DateTime.Now - mouseMovieEndTime).TotalMilliseconds > Global.changeModeTime)
                    {
                        Global.testMode = false;
                    }
                }
                return;
            }
            if (Global.trackerStatus != Global.eyeTrackerStatus.Tracking) return;
            if (eyePoint == gazePoint)
            {
                if (Distance(Global.SmoothPoint, new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y)) > 30)
                {
                    Global.testMode = true;
                    mouseMovieEndTime = DateTime.Now;
                }
                return;
            }
            if (gazePoint.X < 0 || gazePoint.X > Screen.PrimaryScreen.Bounds.Width || gazePoint.Y < 0 || gazePoint.Y > Screen.PrimaryScreen.Bounds.Height)
            {
                eyePoint = new Point(-1, -1);
            }
            else
            {
                eyePoint = gazePoint;
                SmoothPoint = smoothGazePoint((int)(120 * Global.formSize), gazePoint);
                watchEndTime = DateTime.Now;
            }
            Global.eyePoint = eyePoint;
            Global.SmoothPoint = SmoothPoint;
            if (Global.clickState == Global.ClickState.pause)
            {
                if (isEnterMenuBtn(new System.Drawing.Point((int)Global.SmoothPoint.X, (int)Global.SmoothPoint.Y)))
                {
                    Cursor.Position = new System.Drawing.Point((int)Global.SmoothPoint.X, (int)Global.SmoothPoint.Y);
                }
                else
                {
                    Cursor.Position = new System.Drawing.Point(0, 0);
                }
            }
            else
            {
                Cursor.Position = new System.Drawing.Point((int)Global.SmoothPoint.X, (int)Global.SmoothPoint.Y);
            }
        }

        private bool isEnterMenuBtn(System.Drawing.Point pt)
        {
            if (Global.SRF != null)
            {
                if (Global.SRF.srBackPos.X == 0) return false;
                System.Windows.Point relativePoint = Global.SRF.srBackPos;
                if (pt.X > relativePoint.X * 0.8 && pt.X < relativePoint.X + Global.SRF.srBack.ActualWidth * Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth * 1.1 &&
                    pt.Y > relativePoint.Y * 0.8 && pt.Y < relativePoint.Y + Global.SRF.srBack.ActualHeight * Screen.PrimaryScreen.Bounds.Height / SystemParameters.PrimaryScreenHeight * 1.1)
                {
                    return true;
                }
            }
            return false;
        }
        DateTime starTick = DateTime.Now;
        private Point[] pos = new Point[100];
        private List<Point> eyePos = new List<Point>();
        int posIndex = 0;
        private Point smoothGazePoint(int distance,Point point)
        {
            if ((DateTime.Now - starTick).TotalMilliseconds > 300 || Distance(point, Global.SmoothPoint) > distance)
            {
                posIndex = 0;
                for (int i = 0; i < pos.Length; i++) pos[i] = new Point(0, 0);
                eyePos.Clear();
            }
            starTick = DateTime.Now;
           
            eyePos.Add(point);
            if (eyePos.Count > 30)
            {
                Global.isFocus = true;
                Global.cursorControl.dTimer.IsEnabled = true;
            }
            else
            {
                Global.isFocus = false;
            }
            int x = 0, y = 0;
            if (eyePos.Count > 0)
            {
                foreach (Point p in eyePos)
                {
                    x += (int)p.X;
                    y += (int)p.Y;
                }
                x = x / eyePos.Count;
                y = y / eyePos.Count;
            }
            Point sp = new Point(x, y);
            return sp;
        }
        public double Distance(Point pointA, Point pointB)
        {
            double dist = Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2) + Math.Pow(pointB.Y - pointA.Y, 2));
            return dist;
        }
        public void stop()
        {
            Global.eyeXHost.Dispose();
        }
    }
}
