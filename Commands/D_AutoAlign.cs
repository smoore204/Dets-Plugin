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




namespace Dets
{
    public class D_AutoAlign : Command
    {
        public D_AutoAlign()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static D_AutoAlign Instance { get; private set; }

        public override string EnglishName => "D_AutoAlign";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //ensure annotation leaders don't get scaled incorrectly
            doc.ModelSpaceAnnotationScalingEnabled = false;

            //enable user to select breps to be annotated
            var go = new GetObject();
            go.SetCommandPrompt("Select annotations to be rearranged");
            go.GeometryFilter = ObjectType.Annotation;

            //Command line options
            OptionInteger offsetDistance = new OptionInteger(Store.offsetDistance);
            int optInteger = go.AddOptionInteger("OffsetDistance", ref offsetDistance);

            while (true)
            {
                GetResult res = go.GetMultiple(1, 0);
                
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (res == Rhino.Input.GetResult.Object)
                {
                    List<Point3d> startPointsRight = new List<Point3d>();
                    List<Point3d> startPointsLeft = new List<Point3d>();
                    List<Leader> leaders = new List<Leader>();
                    List<double> textHeights = new List<double>();
                    List<Point3d> endPoints = new List<Point3d>();
                    List<Point3d[]> listLeaderPoints = new List<Point3d[]>();
                    List<string> textList = new List<string>();
                    List<string> angles = new List<string>();
                    List<DimensionStyle.ArrowType> arrowTypes = new List<DimensionStyle.ArrowType>();

                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        //get info about selected leaderlines
                        Leader leader = (Leader)go.Object(i).Geometry();
                        textHeights.Add(leader.TextHeight);
                        if (leader.GetUserString("Side") == "Right")
                        {
                            startPointsRight.Add(leader.Curve.PointAtStart);
                        }
                        else 
                        {
                            startPointsLeft.Add(leader.Curve.PointAtStart);
                        } 
                        angles.Add(Utils.GetLeaderLineAngle(leader));
                    }

                    double maxRightXVal = Utils.FindExtremePoint(startPointsRight, "Right").X;
                    double maxLeftXVal = Utils.FindExtremePoint(startPointsLeft, "Left").X;

                    //draw leader lines that are all aligned
                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        //set second leader line point to be 5 away from maxXval
                        Leader leader = (Leader)go.Object(i).Geometry();
                        double secondPtX;
                        Store.offsetDistance = offsetDistance.CurrentValue;
                        Point2d[] newPoints = new Point2d[3];

                        //draw new leaderlines
                        if (leader.GetUserString("Side") == "Right")
                        {
                            newPoints[0] = new Point2d(leader.Points2D[0]);
                            secondPtX = maxRightXVal + offsetDistance.CurrentValue;
                            switch (angles[i])
                            {
                                case "Zero":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y);
                                    newPoints[2] = new Point2d(secondPtX + offsetDistance.CurrentValue, startPointsRight[i].Y);
                                    break;
                                case "Thirty":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + (1 / Math.Sqrt(3)) * Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX + offsetDistance.CurrentValue, newPoints[0].Y + (1 / Math.Sqrt(3)) * Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                case "FortyFive":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX + offsetDistance.CurrentValue, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                case "Sixty":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Sqrt(3) * Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX + offsetDistance.CurrentValue, newPoints[0].Y + Math.Sqrt(3) * Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                default: //if no match default to 45 degree angle
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX + offsetDistance.CurrentValue, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                            }
                        }
                        else
                        {
                            newPoints[0] = new Point2d(leader.Points2D[0]);
                            secondPtX = maxLeftXVal - offsetDistance.CurrentValue;
                            switch (angles[i])
                            {
                                case "Zero":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y);
                                    newPoints[2] = new Point2d(secondPtX - offsetDistance.CurrentValue, startPointsLeft[i].Y);
                                    break;
                                case "Thirty":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + (1 / Math.Sqrt(3)) * Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX - offsetDistance.CurrentValue, newPoints[0].Y + (1 / Math.Sqrt(3)) * Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                case "FortyFive":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX - offsetDistance.CurrentValue, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                case "Sixty":
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Sqrt(3) * Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX - offsetDistance.CurrentValue, newPoints[0].Y + Math.Sqrt(3) * Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                                default: //if no match default to 45 degree angle
                                    newPoints[1] = new Point2d(secondPtX, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    newPoints[2] = new Point2d(secondPtX - offsetDistance.CurrentValue, newPoints[0].Y + Math.Abs(secondPtX - newPoints[0].X));
                                    break;
                            }
                        }


                        //RhinoList<Point3d> leaderPoints = new RhinoList<Point3d>(pointArray);
                        //endPoints.Add(new Point3d(secondPtX, startPointsRight[i].Y + secondPtX - startPointsRight[i].X, 0.0));
                        //listLeaderPoints.Add(leaderPoints.ToArray());
                        leader.Points2D = newPoints;
                        leaders.Add(leader);
                    }

                    //find leader lines that are too close together
                    int numTooClose = 1;
                    int iters = 0;
                    double avTextHeight = textHeights.Average();

                    while (iters < 100) //cap number of iterations so that it wont run forever
                    {
                        Dictionary<int, List<int>> tooCloseIndices = Utils.tooCloseIndices(leaders, avTextHeight);
                        numTooClose = tooCloseIndices.Count;
                        if (numTooClose == 0)
                        {
                            break;
                        }
                        else
                        {
                            //get top leader of each group
                            foreach (KeyValuePair<int, List<int>> kvp in tooCloseIndices)
                            {
                                int topLeaderIndex = -1;
                                int bottomLeaderIndex = -1;
                                double maxY = -100000000;
                                double minY = 100000000;
                                bool intersectionPresentTop = false;
                                bool intersectionPresentBottom = false;
                                Polyline movedPlineTop = null;
                                Polyline movedPlineBottom = null;

                                foreach (int index in kvp.Value)
                                {
                                    RhinoObject[] objects = Utils.GetClosestObjects(leaders[index], doc);
                                    List<BoundingBox> bboxes = new List<BoundingBox>(); 
                                    foreach (RhinoObject obj in objects)
                                    {   bboxes.Add(Utils.GetBoundingBox(obj));  }
                                    BoundingBox boundingBox = Utils.GetGroupBoundingBox(bboxes);
                                    Point3d[] bboxCorners = boundingBox.GetCorners();
                                    Point3d topCorner = bboxCorners[2];
                                    Point3d bottomCorner = bboxCorners[0];

                                    if (topCorner.Y > maxY)
                                    {
                                        topLeaderIndex = index;
                                        maxY = topCorner.Y;
                                    }
                                    if (bottomCorner.Y < minY)
                                    {
                                        bottomLeaderIndex = index;
                                        minY = bottomCorner.Y;
                                    }
                                }
                                Point3d movedTestPointUp = new Point3d(leaders[topLeaderIndex].Curve.PointAtEnd.X, leaders[topLeaderIndex].Curve.PointAtEnd.Y + avTextHeight/2 + 1, 0);
                                Point3d movedTestPointDown = new Point3d(leaders[bottomLeaderIndex].Curve.PointAtEnd.X, leaders[bottomLeaderIndex].Curve.PointAtEnd.Y - avTextHeight / 2 + 1, 0);
                                RhinoObject[] closestObjectsTop = Utils.GetClosestObjects(leaders[topLeaderIndex], doc);
                                RhinoObject[] closestObjectsBottom = Utils.GetClosestObjects(leaders[bottomLeaderIndex], doc);
                                Curve[] curvesTop = Utils.ObjectsToCurve(closestObjectsTop);
                                Curve[] curvesBottom = Utils.ObjectsToCurve(closestObjectsBottom);
                                Utils.GetMovedLeaderLine(leaders[topLeaderIndex], curvesTop, movedTestPointUp, ref intersectionPresentTop, ref movedPlineTop);
                                Utils.GetMovedLeaderLine(leaders[bottomLeaderIndex], curvesBottom, movedTestPointDown, ref intersectionPresentBottom, ref movedPlineBottom);
                                if (intersectionPresentTop)
                                {
                                    leaders[topLeaderIndex].Points3D = movedPlineTop.ToArray();
                                }
                                else
                                {
                                    if (intersectionPresentBottom)
                                    {
                                        leaders[bottomLeaderIndex].Points3D = movedPlineBottom.ToArray();
                                    }
                                    else
                                        break;
                                }
                            }
                            iters++;
                        }
                    }
                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        Utils.Replace(leaders[i], doc, go.Object(i).Object(), true);
                    }


                    //doc.Views.Redraw();
                    return Result.Success;
                }

            }
        }
    }
}