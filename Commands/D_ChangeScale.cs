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
    public class D_ChangeScale : Command
    {
        public D_ChangeScale()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static D_ChangeScale Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "D_ChangeScale";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //ensure annotation leaders don't get scaled incorrectly
            doc.ModelSpaceAnnotationScalingEnabled = false;

            //enable user to select annotations to be modified
            var go = new GetObject();
            go.SetCommandPrompt("Select annotations to be modified");
            go.GeometryFilter = ObjectType.Annotation;

            //Command Line Options
            string[] scales = new string[] { "Scale_1", "Scale_2", "Scale_3", "Scale_4", "Scale_5", "Scale_6" };
            int scaleIndex = Store.scaleDefault;
            int optListScale = go.AddOptionList("Scale", scales, scaleIndex);

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

                        //change scale
                        double[] scaleValues = Utils.GetScaleValues(scales[scaleIndex]);
                        Point3d[] changedScaleLeaderPoints = Utils.ChangeScale(leader, scales[scaleIndex]);
                        leader.Points3D = changedScaleLeaderPoints;
                        leader.TextHeight = scaleValues[0];
                        leader.LeaderArrowSize = scaleValues[1];
                        leader.DimensionStyle.TextGap = scaleValues[4];
                        
                        //Add new leader line and remove old one
                        Utils.Replace(leader, doc, go.Object(i).Object(), true);

                        //update store values
                        Store.textHeight = scaleValues[0];
                    }
                    break;
                }
                else if (res == Rhino.Input.GetResult.Option)
                {
                    if (go.OptionIndex() == optListScale)
                        scaleIndex = go.Option().CurrentListOptionIndex;
                        Store.scaleDefault = scaleIndex;
                }
            }
            return Result.Success; 
        }
    }
}

