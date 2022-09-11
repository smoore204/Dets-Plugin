using System;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.DocObjects;
using System.Drawing;
using System.Collections.Generic;

namespace Dets
{
    public class D_ChangeStyle : Command
    {
        public D_ChangeStyle()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static D_ChangeStyle Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "D_ChangeStyle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //enable user to select annotations to be modified
            var go = new GetObject();
            go.SetCommandPrompt("Select annotations to be modified");
            go.GeometryFilter = ObjectType.Annotation;

            //Command Line Options
            OptionDouble fontSize = new OptionDouble(Store.textHeight, 0.01, 100);

            string[] leaderLineAngles = new string[] { "Zero", "Thirty", "FortyFive", "Sixty" };
            string[] pointerTypes = new string[] { "OpenArrow", "SolidTriangle", "Dot", "None" };

            int pointerTypeIndex = Store.pointerTypeIndex;
            int angleIndex = Store.leaderLineAngleIndex;

            int optInteger = go.AddOptionDouble("TextHeight", ref fontSize);
            int optListLeaderAngle = go.AddOptionList("LeaderAngle", leaderLineAngles, angleIndex);
            int optListPointer = go.AddOptionList("Pointer", pointerTypes, pointerTypeIndex);

            go.OptionIndex();

            //code
            while (true)
            {
                GetResult res = go.GetMultiple(1, 0);

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (res == Rhino.Input.GetResult.Object)
                {
                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        Leader leader = (Leader)go.Object(i).Geometry();

                        //change pointer type
                        var pointerType = leader.LeaderArrowType;
                        if (pointerType.ToString() != pointerTypes[pointerTypeIndex])
                        {
                            RhinoObject[] closestObjects = Utils.GetClosestObjects(leader, doc);
                            Point3d[] newLeaderPoints_1;

                            //change leader line points from center to edge if current arrow style is dot
                            if (pointerType.ToString() == "Dot")
                            {
                                newLeaderPoints_1 = Utils.GetSPointOfObjects(leader, doc);
                                leader.Points3D = newLeaderPoints_1;
                                leader.LeaderArrowSize = leader.LeaderArrowSize * 2;
                            }

                            switch (pointerTypes[pointerTypeIndex])
                            {
                                case "OpenArrow":
                                    leader.LeaderArrowType = DimensionStyle.ArrowType.OpenArrow;
                                    Store.pointerTypeIndex = 0;
                                    break;
                                case "SolidTriangle":
                                    leader.LeaderArrowType = DimensionStyle.ArrowType.SolidTriangle;
                                    Store.pointerTypeIndex = 1;
                                    break;
                                case "None":
                                    leader.LeaderArrowType = DimensionStyle.ArrowType.None;
                                    Store.pointerTypeIndex = 3;
                                    break;
                                case "Dot":
                                    leader.LeaderArrowType = DimensionStyle.ArrowType.Dot;
                                    leader.LeaderArrowSize = leader.LeaderArrowSize / 2;
                                    Store.pointerTypeIndex = 2;
                                    //change leader line points to center
                                    newLeaderPoints_1 = Utils.GetCenterPoint(closestObjects, leader);
                                    leader.Points3D = newLeaderPoints_1;
                                    break;
                            }
                        }
                        //change text height
                        int textHeight = Convert.ToInt32(leader.TextHeight);
                        if (textHeight != fontSize.CurrentValue)
                        {
                            leader.TextHeight = fontSize.CurrentValue;
                            Store.textHeight = fontSize.CurrentValue;
                        }

                        //change leader line agle
                        string angle = Utils.GetLeaderLineAngle(leader);
                        if (angle != leaderLineAngles[angleIndex])
                        {
                            Point2d[] leaderPoints = leader.Points2D;
                            Point2d[] newLeaderPoints = new Point2d[3];

                            switch (leaderLineAngles[angleIndex])
                            {
                                case "Zero":
                                    newLeaderPoints[0] = new Point2d(leaderPoints[0][0], leaderPoints[0][1]);
                                    newLeaderPoints[1] = new Point2d(leaderPoints[1][0], leaderPoints[0][1]);
                                    newLeaderPoints[2] = new Point2d(leaderPoints[2][0], leaderPoints[0][1]);
                                    Store.leaderLineAngleIndex = 0;
                                    break;
                                case "Thirty":
                                    newLeaderPoints[0] = new Point2d(leaderPoints[0][0], leaderPoints[0][1]);
                                    newLeaderPoints[1] = new Point2d(leaderPoints[1][0], leaderPoints[0][1] + (1 / Math.Sqrt(3)) * Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    newLeaderPoints[2] = new Point2d(leaderPoints[2][0], leaderPoints[0][1] + (1 / Math.Sqrt(3)) * Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    Store.leaderLineAngleIndex = 1;
                                    break;
                                case "FortyFive":
                                    newLeaderPoints[0] = new Point2d(leaderPoints[0][0], leaderPoints[0][1]);
                                    newLeaderPoints[1] = new Point2d(leaderPoints[1][0], leaderPoints[0][1] + Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    newLeaderPoints[2] = new Point2d(leaderPoints[2][0], leaderPoints[0][1] + Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    Store.leaderLineAngleIndex = 2;
                                    break;
                                case "Sixty":
                                    newLeaderPoints[0] = new Point2d(leaderPoints[0][0], leaderPoints[0][1]);
                                    newLeaderPoints[1] = new Point2d(leaderPoints[1][0], leaderPoints[0][1] + Math.Sqrt(3) * Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    newLeaderPoints[2] = new Point2d(leaderPoints[2][0], leaderPoints[0][1] + Math.Sqrt(3) * Math.Abs(leaderPoints[1][0] - leaderPoints[0][0]));
                                    Store.leaderLineAngleIndex = 3;
                                    break;
                            }
                            leader.Points2D = newLeaderPoints;
                        }

                        //Add new leader line and remove old one
                        Utils.Replace(leader, doc, go.Object(i).Object(), true);
                    }
                    break;
                }
                else if (res == Rhino.Input.GetResult.Option)
                {
                    if (go.OptionIndex() == optListPointer)
                        pointerTypeIndex = go.Option().CurrentListOptionIndex;
                        Store.pointerTypeIndex = pointerTypeIndex;
                    if (go.OptionIndex() == optListLeaderAngle)
                        angleIndex = go.Option().CurrentListOptionIndex;
                        Store.leaderLineAngleIndex = angleIndex;
                }
            }
            return Result.Success; 
        }
    }
}

