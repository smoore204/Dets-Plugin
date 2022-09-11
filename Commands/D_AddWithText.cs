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
    public class D_AddWithText : Command
    {
        public D_AddWithText()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static D_AddWithText Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "D_AddWithText";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
         {

            //enable user to select breps to be annotated
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
                    //unhighlight all selected objects
                    Utils.UnhighlightAll(go, doc);

                    //inputs
                    double angle = Store.leaderLineAngleIndex;

                    //get all group ids that exist in a selections
                    List<int> allGroupIndexes = new List<int>();

                    for (int i = 0; i < go.ObjectCount; i++)
                    {
                        //check if an object has group id
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

                            //get and set userstring data
                            string out_string = name;
                            obj.Highlight(true);
                            doc.Views.Redraw();
                            GetString gs = new GetString();
                            gs.SetCommandPrompt("add text");
                            var result = gs.GetLiteralString();
                            if (result == Rhino.Input.GetResult.String)
                            {
                                out_string = gs.StringResult().Trim();
                            }
                            obj.Highlight(false);
                            doc.Views.Redraw();
                            name = out_string;
                            Utils.SetUserString(obj, out_string);

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

                            //getting bbox and intersections of individual elements in a group
                            foreach (RhinoObject rhinoObject in groupObjects)
                            {
                                var objRef = new ObjRef(rhinoObject);
                                var groupObj = objRef.Object();
                                groupObj.Highlight(true);

                                bboxes.Add(Utils.GetBoundingBox(rhinoObject));
                                intersectionPoints.Add(Utils.BBoxIntersection(rhinoObject, sides[sideIndex]));
                            }
                            //get and set userdata for the group
                            Group group = doc.Groups.FindIndex(groupIndex);
                            string groupName = Utils.GetUserStringGroup(doc, groupIndex);
                            string out_string = groupName;
                            Utils.HighlightGroup(doc, groupIndex);
                            GetString gs = new GetString();
                            gs.SetCommandPrompt("add text");
                            var result = gs.GetLiteralString();
                            if (result == Rhino.Input.GetResult.String)
                            {
                                out_string = gs.StringResult().Trim();
                            }
                            Utils.SetUserStringGroup(doc, groupIndex, out_string);
                            Utils.UnhighlightGroup(doc, groupIndex);
                            groupName = out_string;

                            //get bounding box of a group of objects
                            BoundingBox bboxGroup = Utils.GetGroupBoundingBox(bboxes);

                            //find right most bounding bbox intersection
                            Point3d startPoint = Utils.FindExtremePoint(intersectionPoints, sides[sideIndex]);
                            //draw leader line for the group
                            Leader leader = new Leader();
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
                    }
                }
            }
            return Result.Success;
        }
    }
}
