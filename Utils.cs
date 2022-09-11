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
using Rhino.Geometry.Collections;
using Rhino.DocObjects;
using Rhino.Geometry.Intersect;

namespace Dets
{
    class Utils
    {
        /// <summary>
        /// returns the angle as a double given the Angle Option Index
        /// </summary>
        /// <param name="angleIndex"></param>
        /// <returns></returns>
        public static double AngleIndexToDouble(double angleIndex)
        {
            double angleDeg = -1;

            switch (angleIndex)
            {
                case 0:
                    angleDeg = 0;
                    break;
                case 1:
                    angleDeg = 30;
                    break;
                case 2:
                    angleDeg = 45;
                    break;
                case 3:
                    angleDeg = 60;
                    break;
            }
            return angleDeg;
        }
        /// <summary>
        /// Given a string value of an angle, returns a double
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static int AngleStringToDouble(string angle)
        {
            int angleDeg = -1;
            switch (angle)
            {
                case "Zero":
                    angleDeg = 0;
                    break;
                case "Thirty":
                    angleDeg = 30;
                    break;
                case "FortyFive":
                    angleDeg = 45;
                    break;
                case "Sixty":
                    angleDeg = 60;
                    break;
            }
            return angleDeg;
        }
        /// <summary>
        /// returns scale values
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double[] GetScaleValues(string scale)
        {
            switch (scale)
            {
                case "Scale_1":
                    return Store.Scale_1;
                case "Scale_2":
                    return Store.Scale_2;
                case "Scale_3":
                    return Store.Scale_3;
                case "Scale_4":
                    return Store.Scale_4;
                case "Scale_5":
                    return Store.Scale_5;
                default:
                    return Store.Scale_6;
            }
        }
        /// <summary>
        /// Draws a new leaderline
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="stPoint"></param>
        /// <param name="name"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="side"></param>
        public static void DrawLeaderLine(RhinoDoc doc, Point3d stPoint, string name, string scale, double angle, string side)
        {
            Rhino.DocObjects.Tables.ObjectTable ot = RhinoDoc.ActiveDoc.Objects;

            //have to make new leader line and add to document before you can modify any of its properties
            Leader leader = new Leader();
            Guid guids = doc.Objects.AddLeader(leader);
            RhinoObject leaderObject = ot.FindId(guids);
            Leader refLeader = (Leader)leaderObject.Geometry;

            double angleDeg = AngleIndexToDouble(angle);
            double[] scaleValues = GetScaleValues(scale);

            double textHeight = scaleValues[0];
            double leaderArrowSize = scaleValues[1];
            double length = scaleValues[2];
            double thirdPoint = scaleValues[3];
            double angleRad = (Math.PI / 180) * angleDeg;
            double xAdd = length * Math.Cos(angleRad);
            double yAdd = length * Math.Sin(angleRad);
            string myText = name;

            Point3d[] leaderPoints = new Point3d[3];
            switch (side)
            {
                case "Right":
                    leaderPoints[0] = new Point3d(stPoint.X, stPoint.Y, stPoint.Z);
                    leaderPoints[1] = new Point3d(stPoint.X + xAdd, stPoint.Y + yAdd, stPoint.Z);
                    leaderPoints[2] = new Point3d(stPoint.X + xAdd + thirdPoint, stPoint.Y + yAdd, stPoint.Z);
                    break;
                case "Left":
                    leaderPoints[0] = new Point3d(stPoint.X, stPoint.Y, stPoint.Z);
                    leaderPoints[1] = new Point3d(stPoint.X - xAdd, stPoint.Y + yAdd, stPoint.Z);
                    leaderPoints[2] = new Point3d(stPoint.X - xAdd - thirdPoint, stPoint.Y + yAdd, stPoint.Z);
                    break;
            }

            //Leader leader = new Leader();
            refLeader.PlainText = myText;
            refLeader.DimensionStyle.TextGap = scaleValues[4];
            refLeader.Points3D = leaderPoints;
            refLeader.TextHeight = textHeight;
            refLeader.LeaderArrowSize = leaderArrowSize;
            refLeader.LeaderArrowType = DimensionStyle.ArrowType.SolidTriangle;
            refLeader.SetUserString("Side", side);
            doc.Objects.AddLeader(refLeader);
        }
        /// <summary>
        /// returns the new leader line when a new scale is applied
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Point3d[] ChangeScale(Leader leader, string scale)
        {
            double[] scaleValues = GetScaleValues(scale);
            double distance1 = scaleValues[2];
            double distance2 = scaleValues[3];
            string angle = GetLeaderLineAngle(leader);
            int angleDeg = AngleStringToDouble(angle);
            double angleRad = angleDeg * Math.PI / 180;
            Point3d sPoint = leader.Curve.PointAtStart;
            string side = leader.GetUserString("Side");

            Point3d[] newLeaderLinePoints = new Point3d[3];
            newLeaderLinePoints[0] = sPoint;

            if (side == "Right")
            {
                switch (angle)
                {
                    case "Zero":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + distance1, sPoint.Y, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + distance1 + distance2, sPoint.Y, 0);
                        break;
                    case "Thirty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(30 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(30 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        break;
                    case "FortyFive":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(45 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(45 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        break;
                    case "Sixty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(60 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(60 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        break;
                }
            }
            else
            {
                switch (angle)
                {
                    case "Zero":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - distance1, sPoint.Y, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - distance1 - distance2, sPoint.Y, 0);
                        break;
                    case "Thirty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(30 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(30 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        break;
                    case "FortyFive":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(45 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(45 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        break;
                    case "Sixty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(60 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(60 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        break;
                }
            }

            return newLeaderLinePoints;
        }
        /// <summary>
        /// Draw a new leader line and delete an old one
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="doc"></param>
        /// <param name="deleteObject"></param>
        /// <param name="redraw"></param>
        public static void Replace(Leader leader, RhinoDoc doc, RhinoObject deleteObject, bool redraw)
        {
            doc.Objects.AddLeader(leader);
            Rhino.DocObjects.Tables.ObjectTable ot = RhinoDoc.ActiveDoc.Objects;
            ot.Delete(deleteObject);
            if (redraw)
            {
                doc.Views.Redraw();
            }
        }
        /// <summary>
        /// Get the left or right most intersection point between an object and its bounding box
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public static Point3d BBoxIntersection(RhinoObject obj, string side)
        {
            if (obj.ObjectType == ObjectType.Curve)
            {
                Curve curve = (Curve)obj.Geometry;
                BoundingBox bbox = curve.GetBoundingBox(true);
                Rectangle3d rect = new Rectangle3d(Plane.WorldXY, bbox.Min, bbox.Max);
                Polyline polyline = rect.ToPolyline();
                Line intersectionLine;

                if (side == "Right") { intersectionLine = polyline.SegmentAt(1); }
                else { intersectionLine = polyline.SegmentAt(3); }
                
                CurveIntersections intersections = Intersection.CurveLine(curve, intersectionLine, 0.01, 0.01);
                //if curve is a horizontal it wont have an intersection event, so have to check below
                if (intersections == null) 
                {
                    if (curve.PointAtStart.X > curve.PointAtEnd.X && side == "Right"){return curve.PointAtStart;}
                    else { return curve.PointAtEnd; }
                }
                IntersectionEvent intersection = intersections[0];
                Point3d intersectionPoint = intersection.PointA;
                return intersectionPoint;
            }
            else
            {
                Brep brep = (Brep)obj.Geometry;
                Point3d intersectionPoint = GetBrepIntersection(side, brep);
                return intersectionPoint;
            }
            
        }
        /// <summary>
        /// Gets the bounding box intersection of a brep
        /// </summary>
        /// <param name="side"></param>
        /// <param name="brep"></param>
        /// <returns></returns>
        private static Point3d GetBrepIntersection(string side, Brep brep)
        {
            BoundingBox bbox = brep.GetBoundingBox(false);
            BrepEdgeList edges = brep.Edges;
            Rectangle3d rect = new Rectangle3d(Plane.WorldXY, bbox.Min, bbox.Max);
            Polyline polyline = rect.ToPolyline();
            Line intersectionLine;
            double intersectionMax;
            if (side == "Right") 
            { 
                intersectionLine = polyline.SegmentAt(1);
                intersectionMax = -100000000;
            }
            else 
            { 
                intersectionLine = polyline.SegmentAt(3);
                intersectionMax = 1000000000;
            }

            Point3d intersectionPoint = new Point3d(0, 0, 0);

            for (int i = 0; i < edges.Count; i++)
            {
                CurveIntersections intersections = (Intersection.CurveLine(edges[i], intersectionLine, 0.01, 0.01));
                if (intersections != null)
                {
                    for (int j = 0; j < intersections.Count; j++)
                    {
                        IntersectionEvent intersection = intersections[j];
                        Point3d tempIntersectionPoint = intersection.PointA;
                        double y_ = tempIntersectionPoint.Y;
                        switch (side)
                        {
                            case "Right":
                                if (y_ > intersectionMax)
                                {
                                    intersectionMax = y_;
                                    intersectionPoint = tempIntersectionPoint;
                                }
                                break;
                            case "Left":
                                if (y_ < intersectionMax)
                                {
                                    intersectionMax = y_;
                                    intersectionPoint = tempIntersectionPoint;
                                }
                                break;
                        }

                    }
                }
            }
            return intersectionPoint;
        }
        /// <summary>
        /// returns the user strings of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetUserStrings(RhinoObject obj)
        {
            if (obj != null)
            {
                string value = obj.Attributes.GetUserString("Dets_Name");
                if (value != null)
                    return value;
                else
                    return "text";
            }
            else
                return "text";

        }
        /// <summary>
        /// returns the user strings of a group
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="groupIndex"></param>
        /// <returns></returns>
        public static string GetUserStringGroup(RhinoDoc doc, int groupIndex)
        {
            RhinoObject[] groupMembers = doc.Groups.GroupMembers(groupIndex);
            RhinoObject groupMember = groupMembers[0];
            string userString = groupMember.Attributes.GetUserString("Dets_Name");
            if (userString != null)
            {
                return userString;
            }
            else
            {
                return "text";
            }
        }
        /// <summary>
        /// the the user strings of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="userString"></param>
        public static void SetUserString(RhinoObject obj, string userString)
        {
            obj.Attributes.SetUserString("Dets_Name", userString);
        }
        /// <summary>
        /// set the user strings of a group of objects
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="groupIndex"></param>
        /// <param name="userString"></param>
        public static void SetUserStringGroup(RhinoDoc doc, int groupIndex, string userString)
        {
            RhinoObject[] groupMembers = doc.Groups.GroupMembers(groupIndex);
            foreach (RhinoObject groupMember in groupMembers)
            {
                groupMember.Attributes.SetUserString("Dets_Name", userString);
            }
        }
        /// <summary>
        /// unhighlight all objects in the documents
        /// </summary>
        /// <param name="go"></param>
        /// <param name="doc"></param>
        public static void UnhighlightAll(GetObject go, RhinoDoc doc)
        {
            ObjRef[] allObjs = go.Objects();
            foreach (ObjRef obj in allObjs)
            {
                obj.Object().Highlight(false);
            }
            doc.Views.Redraw();
        }
        /// <summary>
        /// highlight a group of objects given the groupIndex
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="groupIndex"></param>
        public static void HighlightGroup(RhinoDoc doc, int groupIndex)
        {
            RhinoObject[] groupMembers = doc.Groups.GroupMembers(groupIndex);
            foreach (RhinoObject groupMember in groupMembers)
            {
                groupMember.Highlight(true);
            }
            doc.Views.Redraw();
        }
        /// <summary>
        /// unhighlight all objetcs in a group given a group index
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="groupIndex"></param>
        public static void UnhighlightGroup(RhinoDoc doc, int groupIndex)
        {
            RhinoObject[] groupMembers = doc.Groups.GroupMembers(groupIndex);
            foreach (RhinoObject groupMember in groupMembers)
            {
                groupMember.Highlight(false);
            }
            doc.Views.Redraw();
        }
        /// <summary>
        /// return the closestobjects to a leader line
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static RhinoObject[] GetClosestObjects(Leader leader, RhinoDoc doc)
        {
            Point3d startPoint = leader.Curve.PointAtStart;
            List<RhinoObject> closestObjects = new List<RhinoObject>();

            Guid guid = GetClosestGuid(leader);

            //check if object is part of a group 
            int[] groupIndices = GetGroupIndices(doc, guid);
            if (groupIndices.Length > 0)
            {
                RhinoObject[] groupMembers = doc.Groups.GroupMembers(groupIndices[0]);
                foreach (RhinoObject member in groupMembers)
                {
                    Guid guid1 = member.Id;
                    RhinoObject closestObject = RhinoDoc.ActiveDoc.Objects.FindId(guid1);
                    closestObjects.Add(closestObject);
                }
                return closestObjects.ToArray();
            }
            else //object is not grouped
            {
                RhinoObject closestObject = RhinoDoc.ActiveDoc.Objects.FindId(guid);
                closestObjects.Add(closestObject);
                return closestObjects.ToArray();
            }
        }
        /// <summary>
        /// return the guid of the closest object to a leader line
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        private static Guid GetClosestGuid(Leader leader)
        {
            Point3d startPoint = leader.Curve.PointAtStart;
            Guid closestGuid = Guid.Empty;
            double distance = 10000000;

            foreach (RhinoObject obj in Rhino.RhinoDoc.ActiveDoc.Objects)
            {
                if (obj.ObjectType == ObjectType.Curve)
                {
                    Curve curve = (Curve)obj.Geometry;
                    double t;
                    bool onCurve = curve.ClosestPoint(startPoint, out t, 0.0001);
                    if (onCurve)
                    {
                        return obj.Id;
                    }
                }
                if (obj.ObjectType == ObjectType.Brep)
                {
                    RhinoObject[] objArray = new RhinoObject[] { obj };
                    Curve[] curves = Utils.ObjectsToCurve(objArray);
                    foreach (Curve curve in curves)
                    {
                        double t;
                        bool onCurve = curve.ClosestPoint(startPoint, out t, 0.0001);
                        if (onCurve)
                        {
                            return obj.Id;
                        }
                    }
                }
            }
            foreach (RhinoObject obj in Rhino.RhinoDoc.ActiveDoc.Objects)
            { 
                if (obj.ObjectType == ObjectType.Curve || obj.ObjectType == ObjectType.Brep)
                {
                    Guid guid = obj.Id;
                    BoundingBox bbox = GetBoundingBox(obj);
                    Point3d bboxCenter = bbox.Center;
                    double distanceTemp = startPoint.DistanceTo(bboxCenter);
                    if (distanceTemp < distance)
                    {
                        distance = distanceTemp;
                        closestGuid = guid;
                    }
                }
            }
            return closestGuid;
        }
        /// <summary>
        /// Given an objects guid, return the group indices that it belongs too
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static int[] GetGroupIndices(RhinoDoc doc, Guid guid)
        {
            var attributes = doc.Objects.Find(guid).Attributes;
            int[] groupIndices = attributes.GetGroupList();
            int[] emptyArray = new int[0];
            if (groupIndices != null)
            {
                if (groupIndices.Length > 0)
                {
                    return groupIndices;
                }
                else
                    return emptyArray;
            }
            return emptyArray;
        }
        /// <summary>
        /// get bounding box of object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static BoundingBox GetBoundingBox(RhinoObject obj)
        {
            if (obj.ObjectType == ObjectType.Curve)
            {
                Curve curve = (Curve)obj.Geometry;
                return curve.GetBoundingBox(false);
            }
            else
            {
                Brep brep = (Brep)obj.Geometry;
                return brep.GetBoundingBox(false);
            }

        }
        /// <summary>
        /// finds the center point of a group of objects and returns new leader line points given this centered start point
        /// </summary>
        /// <param name="closestObjects"></param>
        /// <param name="leader"></param>
        /// <returns></returns>
        public static Point3d[] GetCenterPoint(RhinoObject[] closestObjects, Leader leader)
        {  
            List<BoundingBox> bboxes = new List<BoundingBox>();
            foreach (RhinoObject obj in closestObjects)
            {
                bboxes.Add(obj.Geometry.GetBoundingBox(true));
                
            }
            BoundingBox bboxUnion = Utils.GetGroupBoundingBox(bboxes);
            Point3d[] newLeaderPoints = GetLeaderPointsGivenSPoint(leader, bboxUnion.Center);
            return newLeaderPoints;
        }


        /// <summary>
        /// finds the right/left most point from a group of objects given a leader line if edge is true, return object centers if false
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="doc"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static Point3d[] GetSPointOfObjects(Leader leader, RhinoDoc doc)
        {
            Curve leaderCurve = leader.Curve;
            Point3d startPoint = leaderCurve.PointAtStart;
            RhinoObject[] closestObjects = Utils.GetClosestObjects(leader, doc);
            Point3d intersectionPoint;
            List<BoundingBox> bboxes = new List<BoundingBox>();
            string side = leader.GetUserString("Side");
            if (closestObjects.Length > 1)
            {
                List<Point3d> intersectionPoints = new List<Point3d>();
                foreach (RhinoObject obj in closestObjects)
                {
                    intersectionPoints.Add(BBoxIntersection(obj, side));
                    bboxes.Add(obj.Geometry.GetBoundingBox(false));
                }
                intersectionPoint = FindExtremePoint(intersectionPoints, side);

            }
            else
            {
                intersectionPoint = BBoxIntersection(closestObjects[0], side); 
            }
            Point3d[] newPoints = GetLeaderPointsGivenSPoint(leader, intersectionPoint);

            return newPoints;
        }
        /// <summary>
        /// Returns the bounding box that encompasses a list of bboxes
        /// </summary>
        /// <param name="bboxes"></param>
        /// <returns></returns>
        public static BoundingBox GetGroupBoundingBox(List<BoundingBox> bboxes)
        {
            BoundingBox unionBox = BoundingBox.Empty;

            for (int i = 0; i < bboxes.Count; i++)
            {
                unionBox.Union(bboxes[i]);
            }

            return unionBox;
        }
        /// <summary>
        /// Gets right or left most point from a list of points
        /// </summary>
        /// <param name="Points"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public static Point3d FindExtremePoint(List<Point3d> Points, string side)
        {
            Point3d point = new Point3d();
            double maxXVal;
            if (side == "Right")
            {
                maxXVal = -1000000;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Points[i].X > maxXVal)
                    {
                        maxXVal = Points[i].X;
                        point = Points[i];
                    }
                }
            }
            else
            {
                maxXVal = 10000000;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Points[i].X < maxXVal)
                    {
                        maxXVal = Points[i].X;
                        point = Points[i];
                    }
                }
            }
            return point;
        }
        /// <summary>
        /// returns the angle of a leader line as a string
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public static string GetLeaderLineAngle(Leader leader)
        {
            //Calculate current leader line angle
            Point2d point1 = leader.Points2D[0];
            Point2d point2 = leader.Points2D[1];
            double X = Math.Abs(point2.X - point1.X);
            double Y = Math.Abs(point2.Y - point1.Y);
            double angleRad = Math.Atan(Y / X);
            double angleDeg = Math.Round(angleRad * 180 /Math.PI);

            switch (angleDeg)
            {
                case 0:
                    return "Zero";
                case 30:
                    return "Thirty";
                case 45:
                    return "FortyFive";
                case 60:
                    return "Sixty";
                default:
                    return "None";
            }
        }
        /// <summary>
        /// returns an array where the first value represents the X distance between the start point and the second point of a leader line, the second value is the Y value, and the third value is the length of the tail
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public static double[] GetLeaderDimensions(Leader leader) 
        {
            Point2d[] leaderPoints = leader.Points2D;
            double[] dimensions = new double[3];
            dimensions[0] = Math.Abs(leaderPoints[1].X - leaderPoints[0].X);
            dimensions[1] = Math.Abs(leaderPoints[1].Y - leaderPoints[0].Y);
            dimensions[2] = Math.Abs(leaderPoints[2].X - leaderPoints[1].X);

            return dimensions;
        }
        /// <summary>
        /// returns the lengths of the first and second segments of a leader line
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public static double[] GetLeaderDistances(Leader leader)
        {
            Point2d[] leaderPoints = leader.Points2D;
            double[] distances = new double[2];
            distances[0] = leaderPoints[0].DistanceTo(leaderPoints[1]);
            distances[1] = leaderPoints[1].DistanceTo(leaderPoints[2]);

            return distances;
        }
        /// <summary>
        /// Will return the leader points of a leader line that has been moved to a new given start point. Use only when the geometry of the leader line is static.
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="sPoint"></param>
        /// <returns></returns>
        public static Point3d[] GetLeaderPointsGivenSPoint(Leader leader, Point3d sPoint)
        {
            string side = leader.GetUserString("Side");
            double distance1 = Utils.GetLeaderDistances(leader)[0];
            double distance2 = Utils.GetLeaderDistances(leader)[1];
            string angle = Utils.GetLeaderLineAngle(leader);

            Point3d[] newLeaderLinePoints = new Point3d[3];
            newLeaderLinePoints[0] = sPoint;    

            if (side == "Right")
            {
                switch (angle)
                {
                    case "Zero":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + distance1, sPoint.Y, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + distance1 + distance2, sPoint.Y, 0);
                        break;
                    case "Thirty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(30 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(30 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        break;
                    case "FortyFive":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(45 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(45 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        break;
                    case "Sixty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X + Math.Cos(60 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X + Math.Cos(60 * Math.PI / 180) * distance1 + distance2, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        break;
                }
            }
            else
            {
                switch (angle)
                {
                    case "Zero":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - distance1, sPoint.Y, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - distance1 - distance2, sPoint.Y, 0);
                        break;
                    case "Thirty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(30 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(30 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(30 * Math.PI / 180) * distance1, 0);
                        break;
                    case "FortyFive":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(45 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(45 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(45 * Math.PI / 180) * distance1, 0);
                        break;
                    case "Sixty":
                        newLeaderLinePoints[1] = new Point3d(sPoint.X - Math.Cos(60 * Math.PI / 180) * distance1, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        newLeaderLinePoints[2] = new Point3d(sPoint.X - Math.Cos(60 * Math.PI / 180) * distance1 - distance2, sPoint.Y + Math.Sin(60 * Math.PI / 180) * distance1, 0);
                        break;
                }
            }
            return newLeaderLinePoints;
        }
        /// <summary>
        /// Returns the leaderline geometry a moved leaderline. Use when the geometry of a leader line is dynamic.
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="curves"></param>
        /// <param name="testPoint"></param>
        /// <param name="intersectionPresent"></param>
        /// <param name="line"></param>
        public static void GetMovedLeaderLine(Leader leader, Curve[] curves, Point3d testPoint, ref bool intersectionPresent, ref Polyline line)
        {
            Point3d startPoint = new Point3d();
            string angle = Utils.GetLeaderLineAngle(leader);
            double[] dimensions = Utils.GetLeaderDimensions(leader);
            double X = dimensions[0];
            double Y = dimensions[1];
            double tail = dimensions[2];
            Point3d secondPoint = new Point3d();
            Point3d thirdPoint = new Point3d();
            Point3d shortPoint = new Point3d();

            thirdPoint = testPoint;

            if (leader.GetUserString("Side") == "Right")
            {
                secondPoint = new Point3d(thirdPoint.X - tail, thirdPoint.Y, 0);
                switch (angle)
                {
                    case "Zero":
                        shortPoint = new Point3d(thirdPoint.X - 1, thirdPoint.Y, 0);
                        break;
                    case "Thirty":
                        shortPoint = new Point3d(secondPoint.X - Math.Sqrt(3) / 2, secondPoint.Y - 0.5, 0);
                        break;
                    case "FortyFive":
                        shortPoint = new Point3d(secondPoint.X - 1, secondPoint.Y - 1, 0);
                        break;
                    case "Sixty":
                        shortPoint = new Point3d(secondPoint.X - 0.5, secondPoint.Y - Math.Sqrt(3) / 2, 0);
                        break;
                }
            }
            else
            {
                secondPoint = new Point3d(thirdPoint.X + tail, thirdPoint.Y, 0);
                switch (angle)
                {
                    case "Zero":
                        shortPoint = new Point3d(thirdPoint.X + 1, thirdPoint.Y, 0);
                        break;
                    case "Thirty":
                        shortPoint = new Point3d(secondPoint.X + Math.Sqrt(3) / 2, secondPoint.Y - 0.5, 0);
                        break;
                    case "FortyFive":
                        shortPoint = new Point3d(secondPoint.X + 1, secondPoint.Y - 1, 0);
                        break;
                    case "Sixty":
                        shortPoint = new Point3d(secondPoint.X + 0.5, secondPoint.Y - Math.Sqrt(3) / 2, 0);
                        break;
                }
            }
            Line shortLine = new Line(secondPoint, shortPoint);
            shortLine.Extend(10000000, 10000000);
            intersectionPresent = false;
            Point3d intersectionPoint = new Point3d();
            double dist = 1000000;
            foreach (Curve curve in curves)
            {
                CurveIntersections intersections = Rhino.Geometry.Intersect.Intersection.CurveLine(curve, shortLine, 0.01, 0.01);
                if (intersections != null)
                {
                    foreach (IntersectionEvent intersection in intersections)
                    {
                        double t = intersection.ParameterA;
                        Point3d tempIntersectionPoint = curve.PointAt(t);
                        double tempDist = secondPoint.DistanceTo(tempIntersectionPoint);
                        if (tempDist < dist)
                        {
                            dist = tempDist;
                            intersectionPoint = tempIntersectionPoint;
                            intersectionPresent = true;
                        }
                    }
                }
            }

            startPoint = intersectionPoint;
            Polyline pline = new Polyline();
            pline.Add(startPoint);
            pline.Add(secondPoint);
            pline.Add(thirdPoint);
            line = pline;
        }

        //Not proud of this function - clean up if there's time
        /// <summary>
        /// finds the indices of leaderlines that are too close together
        /// </summary>
        /// <param name="leaders"></param>
        /// <param name="minDist"></param>
        /// <returns></returns>
        public static Dictionary<int, List<int>> tooCloseIndices(List<Leader> leaders, double minDist)
        {
            Dictionary<int, List<int>> tooClosePairs = new Dictionary<int, List<int>>();
            List<int> alreadyUsed = new List<int>();
            bool keepLooping = true;
            int key = 0;

            for (int i = 0; i < leaders.Count()-1; i++)
            {    if (alreadyUsed.Contains(i))
                { 
                    continue; 
                }
                else
                {
                    keepLooping = true;
                }
                for (int j = i + 1; j < leaders.Count(); j++)
                {
                    double diffY = Math.Abs(leaders[i].Curve.PointAtEnd.Y - leaders[j].Curve.PointAtEnd.Y);
                    if (diffY < minDist*1.5 & (leaders[i].GetUserString("Side") == leaders[j].GetUserString("Side")))
                    {
                        foreach (KeyValuePair<int, List<int>> kvp in tooClosePairs)
                        {
                            foreach (int value in kvp.Value)
                            {
                                if (value == i)
                                {
                                    List<int> values = kvp.Value;
                                    values.Add(j);
                                    tooClosePairs[kvp.Key] = values;
                                    keepLooping = false;
                                    alreadyUsed.Add(i);
                                    alreadyUsed.Add(j);
                                    break;
                                }
                            }break;
                        }
                        if (keepLooping || tooClosePairs.Count == 0) 
                        {
                            List<int> newPair = new List<int>();
                            newPair.Add(i);
                            newPair.Add(j);
                            tooClosePairs.Add(key, newPair);
                            key++;
                            keepLooping = true;
                            break;
                        }

                    }

                }
            }
            return tooClosePairs;
        }
        /// <summary>
        /// Converts breps to curves if they are present in objects array
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static Curve[] ObjectsToCurve(RhinoObject[] objects)
        {
            
            if (objects.Length > 1)
            {
                List<Curve> curvesList = new List<Curve>();
                foreach (RhinoObject closestObject in objects)
                {
                    if (closestObject.ObjectType == ObjectType.Curve)
                    {
                        curvesList.Add((Curve)closestObject.Geometry);
                    }
                    if (closestObject.ObjectType == ObjectType.Brep)
                    {
                        Brep brep = (Brep)closestObject.Geometry;
                        Curve[] curves = brep.DuplicateEdgeCurves(true);
                        Curve[] joinedCurves = Curve.JoinCurves(curves);
                        curvesList.Add(joinedCurves[0]);
                    }
                }
                Curve[] groupJoinedCurves = Curve.JoinCurves(curvesList);
                return groupJoinedCurves;
            }
            else //work with non grouped elements
            {
                if (objects[0].ObjectType == ObjectType.Curve)
                {
                    Curve[] curves = new Curve[] { (Curve)objects[0].Geometry };
                    return curves;
                }
                else
                {
                    Brep brep = (Brep)objects[0].Geometry;
                    Curve[] curves = brep.DuplicateEdgeCurves(true);
                    Curve[] joinedCurves = Curve.JoinCurves(curves);
                    return joinedCurves;
                }
            }
        }

    }
}
