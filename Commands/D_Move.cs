using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.DocObjects;
using Rhino.Display;




namespace Dets
{
    public class D_Move : Command
    {
        public class MyGetPoint : Rhino.Input.Custom.GetPoint
        {
            public Curve[] curves { get; set; }
            public Polyline line { get; set; }
            public Leader leader { get; set; }

            public bool intersectionPresent { get; set; }    
            public Point3d[] leaderLinePoints { get;  set; }

            protected override void OnDynamicDraw(GetPointDrawEventArgs e)
            {
                Polyline pline = new Polyline();
                bool intPresent = false;
                Utils.GetMovedLeaderLine(leader, curves, e.CurrentPoint, ref intPresent, ref pline);
                intersectionPresent = intPresent;
                line = pline;
                if (intersectionPresent)
                {
                    e.Display.DrawPolyline(pline, System.Drawing.Color.Black, 2);
                }
                
                base.OnDynamicDraw(e);
            }
        }

        public static Point3d GetClosestFromGroup(GetPointDrawEventArgs e, Curve[] curves)
        {
            double t = 0;
            double dist = 1000000000;
            Point3d closestPoint = new Point3d();
            foreach (Curve curve in curves)
            {
                bool success = curve.ClosestPoint(e.CurrentPoint, out t);
                Point3d tempClosestPoint = curve.PointAt(t);
                double tempDist = Math.Abs(e.CurrentPoint.DistanceTo(tempClosestPoint));
                if (tempDist < dist)
                {
                    dist = tempDist;
                    closestPoint = tempClosestPoint;
                    
                }
            }
            return closestPoint;
            
        }

        public D_Move()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static D_Move Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "D_Move";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var go = new GetObject();
            go.SetCommandPrompt("Select annotation to move");
            go.GeometryFilter = ObjectType.Annotation;

            //Command line options
            string[] fixEnd = new string[] { "True", "False" };
            int fixEndIndex = Store.fixEnd;
            int optListFixEnd = go.AddOptionList("FixEnd", fixEnd, fixEndIndex);

            while (true)
            {
                GetResult res = go.GetMultiple(1, 1);
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (res == Rhino.Input.GetResult.Object)
                {
                    MyGetPoint myGetPoint = new MyGetPoint();
                    //get object or group that correlates to the leader line
                    Leader leader = (Leader)go.Object(0).Geometry();
                    Point2d[] leaderPoints = leader.Points2D;

                    if (fixEnd[fixEndIndex] == "True")
                    {
                        double X = leaderPoints[2].X;
                        myGetPoint.Constrain(new Line(new Point3d(X, -100000, 0), new Point3d(X, 1000000, 0)));
                    }

                    RhinoObject[] closestObjects = Utils.GetClosestObjects(leader, doc);
                    List<Curve> groupCurveList = new List<Curve>();

                    //work with groups
                    if (closestObjects.Length > 1)
                    {
                        foreach (RhinoObject closestObject in closestObjects)
                        {
                            if (closestObject.ObjectType == ObjectType.Curve)
                            {
                                groupCurveList.Add((Curve)closestObject.Geometry);
                            }
                            if (closestObject.ObjectType == ObjectType.Brep)
                            {
                                Brep brep = (Brep)closestObject.Geometry;
                                Curve[] curves = brep.DuplicateEdgeCurves(true);
                                Curve[] joinedCurves = Curve.JoinCurves(curves);
                                groupCurveList.Add(joinedCurves[0]);
                            }
                        }
                        Curve[] groupJoinedCurves = Curve.JoinCurves(groupCurveList);
                        myGetPoint.curves = groupJoinedCurves;
                    }
                    else //work with non grouped elements
                    {
                        if (closestObjects[0].ObjectType == ObjectType.Curve)
                        {
                            Curve[] curves = new Curve[] { (Curve)closestObjects[0].Geometry };
                            myGetPoint.curves = curves;
                        }
                        if (closestObjects[0].ObjectType == ObjectType.Brep)
                        {
                            Brep brep = (Brep)closestObjects[0].Geometry;
                            Curve[] curves = brep.DuplicateEdgeCurves(true);
                            Curve[] joinedCurves = Curve.JoinCurves(curves);
                            myGetPoint.curves = joinedCurves;
                        }
                    }
                    myGetPoint.leader = leader;
                    myGetPoint.Get();

                    Polyline pline = myGetPoint.line;
                    Point3d[] points = pline.ToArray();
                    leader.Points3D = points;
                    if (myGetPoint.intersectionPresent)
                    {
                        Utils.Replace(leader, doc, go.Object(0).Object(), true);
                    }
                    break;
                }
                else if (res == Rhino.Input.GetResult.Option)
                {
                    if (go.OptionIndex() == optListFixEnd)
                    {
                        fixEndIndex = go.Option().CurrentListOptionIndex;
                        Store.fixEnd = fixEndIndex;
                    }
                }
            }
            return Result.Success;
        }
    }
}

