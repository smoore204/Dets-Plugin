using System;
using System.Linq;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace Dets
{
    public class D_AddAnnotation : Command
    {
        public D_AddAnnotation()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static D_AddAnnotation Instance { get; private set; }

        public override string EnglishName => "D_AddAnnotation";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //ensure annotation leaders don't get scaled incorrectly
            doc.ModelSpaceAnnotationScalingEnabled = false;

            //enable user to select objects to be annotated
            var go = new GetObject();
            go.GroupSelect = true;
            go.SetCommandPrompt("Select objects to be annotated");

            //Command line options
            string[] sides = new string[] { "Left", "Right" };
            string[] scales = new string[] { "Scale_1", "Scale_2", "Scale_3", "Scale_4", "Scale_5", "Scale_6" };
            int scaleIndex = Store.scaleDefault;
            int sideIndex = Store.side;
            int optListSide = go.AddOptionList("Side", sides, sideIndex);
            int optListScale = go.AddOptionList("Scale", scales, scaleIndex);

            go.OptionIndex();

            while (true)
            {
                GetResult res = go.GetMultiple(1, 0);
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (res == Rhino.Input.GetResult.Object)
                {
                    //inputs
                    double angle = Store.leaderLineAngleIndex;

                    //get all group ids that exist in a selections
                    List<int> allGroupIndexes = new List<int>();

                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        Guid guid = go.Object(i).ObjectId;
                        var attributes = doc.Objects.Find(guid).Attributes;
                        int[] groupIDs = attributes.GetGroupList();
                        if (groupIDs != null)
                        {
                            foreach (int groupID in groupIDs)
                            {
                                allGroupIndexes.Add(groupID);
                            }
                        }

                        //work with elements that are not grouped
                        if (groupIDs == null)
                        {
                            var obj = go.Object(i).Object();
                            string name = Utils.GetUserStrings(obj);
                            Point3d startPoint = Utils.BBoxIntersection(obj, sides[sideIndex]);
                            Utils.DrawLeaderLine(doc, startPoint, name, scales[scaleIndex], angle, sides[sideIndex]);
                        }
                    }

                    //adding annotation to a group of objects
                    List<int> uniqueGroupIndexes = allGroupIndexes.Distinct().ToList();

                    if (uniqueGroupIndexes.Count > 0)
                    {
                        foreach (int groupIndex in uniqueGroupIndexes)
                        {
                            RhinoObject[] groupObjects = doc.Objects.FindByGroup(groupIndex);
                            List<BoundingBox> bboxes = new List<BoundingBox>();
                            List<Point3d> intersectionPoints = new List<Point3d>();
                            List<Guid> groupMemberIds = new List<Guid>();
                            string groupName = "text";

                            foreach (RhinoObject rhinoObject in groupObjects)
                            {
                                ObjRef objRef = new ObjRef(rhinoObject);
                                var objGroup = objRef.Object();
                                groupName = Utils.GetUserStrings(objGroup);
                                groupMemberIds.Add(objRef.ObjectId);

                                bboxes.Add(Utils.GetBoundingBox(rhinoObject));
                                intersectionPoints.Add(Utils.BBoxIntersection(rhinoObject, sides[sideIndex]));
                            }

                            //get bounding box of a group of objects
                            BoundingBox bboxGroup = Utils.GetGroupBoundingBox(bboxes);

                            //find right most bounding bbox intersection
                            Point3d startPoint = Utils.FindExtremePoint(intersectionPoints, sides[sideIndex]);
                            Utils.DrawLeaderLine(doc, startPoint, groupName, scales[scaleIndex], angle, sides[sideIndex]);
                        }
                    }

                    doc.Views.Redraw();
                    break;
                } 
                else if (res == Rhino.Input.GetResult.Option)
                {
                    if (go.OptionIndex() == optListSide)
                    {
                        sideIndex = go.Option().CurrentListOptionIndex;
                        Store.side = sideIndex;
                    }
                    if (go.OptionIndex() == optListScale)
                    {
                        scaleIndex = go.Option().CurrentListOptionIndex;
                        Store.scaleDefault = scaleIndex;
                        switch (scaleIndex)
                        {
                            case 1:
                                Store.textHeight = Store.Scale_1[0];
                                break;
                            case 2:
                                Store.textHeight = Store.Scale_2[0];
                                break;
                            case 3:
                                Store.textHeight = Store.Scale_3[0];
                                break;
                            case 4:
                                Store.textHeight = Store.Scale_4[0];
                                break;
                            case 5:
                                Store.textHeight = Store.Scale_5[0];
                                break;
                            case 6:
                                Store.textHeight = Store.Scale_6[0];
                                break;
                        }
                    }
                }
            }
            return Result.Success;
        }
    }
}