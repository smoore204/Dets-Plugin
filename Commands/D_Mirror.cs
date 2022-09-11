using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.Geometry;
using Rhino.DocObjects;
using System.Collections.Generic;

namespace Dets
{
    public class D_Mirror : Command
    {
        public D_Mirror()
        {
            Instance = this;
        }
        public static D_Mirror Instance { get; private set; }
        public override string EnglishName => "D_Mirror";
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //enable user to select annotations to be modified
            var go = new GetObject();
            go.SetCommandPrompt("Select annotations to be modified");
            go.GeometryFilter = ObjectType.Annotation;
            go.GetMultiple(1, 0);

            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                Leader leader = (Leader)go.Object(i).Geometry();
                string currentSide = leader.GetUserString("Side");
                string newSide;
                if (currentSide == "Right")
                {
                    newSide = "Left";
                    leader.SetUserString("Side", "Left");
                }
                else
                {
                    newSide = "Right";
                    leader.SetUserString("Side", "Right");
                }

                RhinoObject[] closestObjects = Utils.GetClosestObjects(leader, doc);
                Point3d sPoint = new Point3d();
                if (closestObjects.Length == 1)
                {
                    sPoint = Utils.BBoxIntersection(closestObjects[0], newSide);
                }
                if (closestObjects.Length > 1)
                {
                    List<Point3d> intersectionPoints = new List<Point3d>();
                    List<BoundingBox> boundingBoxes = new List<BoundingBox>();
                    foreach (RhinoObject obj in closestObjects)
                    {
                        intersectionPoints.Add(Utils.BBoxIntersection(obj, newSide));
                        boundingBoxes.Add(Utils.GetBoundingBox(obj));
                    }
                    sPoint = Utils.FindExtremePoint(intersectionPoints, newSide);
                }
                Point3d[] newLeaderLinePoints = Utils.GetLeaderPointsGivenSPoint(leader, sPoint);
                leader.Points3D = newLeaderLinePoints;
                if (leader.LeaderArrowType == DimensionStyle.ArrowType.Dot)
                {
                    leader.Points3D = Utils.GetCenterPoint(closestObjects, leader);
                }
                Utils.Replace(leader, doc, go.Object(i).Object(), true);

            }
            return Result.Success;
        }
    }
}

