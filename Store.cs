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
using Rhino.DocObjects;

namespace Dets
{
    public class Store
    {
        public static double textHeight = 1;

        public static int pointerTypeIndex = 1;

        public static int leaderLineAngleIndex = 2;

        public static int offsetDistance = 5;

        public static int side = 1;

        public static int fixEnd = 0;

        public static int scaleDefault = 1;

        //textheight, leaderArrowSize, length, thirdPoint, text gap
        public static double[] Scale_1 = { 0.5, 0.5, 2, 2, 0.5 };
        public static double[] Scale_2 = { 1, 1, 4, 4, 1};
        public static double[] Scale_3 = { 1.5, 1.5, 6, 6, 1 };
        public static double[] Scale_4 = { 2, 2, 8, 8, 2};
        public static double[] Scale_5 = { 3, 3, 12, 12, 2};
        public static double[] Scale_6 = { 4, 4, 16, 16, 2};

        public static IDictionary<Guid, Guid> ObjectLeaderPairs = new Dictionary<Guid, Guid>();
        public static IDictionary<Guid, Guid[]> GroupObjectLeaderPairs = new Dictionary<Guid, Guid[]>();
    }
}
