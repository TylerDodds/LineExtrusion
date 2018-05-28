using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Segmentation
{
    /// <summary>
    /// Segments an <see cref="ILineSegmentation"/> into a list of <see cref="LinePointUV"/> with uv parameters.
    /// </summary>
    public static class LineSegmentation
    {
        /// <summary>
        /// Segments an <see cref="ILineSegmentation"/> line into a list of <see cref="LinePointUV"/> with uv parameters.
        /// </summary>
        public static List<LinePointUV> GetLinePointsUV(ILineSegmentation segemtaneome)
        {
            List<LinePointUV> ret = new List<LinePointUV>();
            var points = segemtaneome.GetLineSegmentsPoints();
            if (points.Count == 0) { return ret; }

            float v = 0.5f;
            float distance = 0f;
            var previousVector = points[0].Point;
            for (int i = 0; i < points.Count; i++)
            {
                var currentPoint = points[i];
                distance += (currentPoint.Point - previousVector).magnitude;
                ret.Add(new LinePointUV(currentPoint.Parameter, currentPoint.Point, new Vector2(distance, v)));
                previousVector = currentPoint.Point;
            }
            return ret;
        }
    }
}