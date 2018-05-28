using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;
using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Triangulation
{
    /// <summary>
    /// Helper class for algorithms involving polygons defined by points.
    /// </summary>
    public static class PolygonHelper 
    {
        /// <summary>
        /// Orders the line extrusion result's contours by their areas, in descending order.
        /// </summary>
        /// <param name="contours">List of contours.</param>
        public static void OrderContourByAreaDescending(List<Vector2WithUV[]> contours)
        {
            contours.Sort(CompareContoursByLargerArea);
        }

        /// <summary>
        /// Compares two contour polygons, defined by point arrays, based on their enclosed areas.
        /// </summary>
        /// <param name="contour1">The first contour polygon.</param>
        /// <param name="contour2">The second contour polygon.</param>
        private static int CompareContoursByLargerArea(Vector2WithUV[] contour1, Vector2WithUV[] contour2)
        {
            var area1 = Mathf.Abs(ContourSignedArea(contour1));
            var area2 = Mathf.Abs(ContourSignedArea(contour2));
            return -area1.CompareTo(area2);
        }
        
        /// <summary>
        /// Returns the signed area of a 2D contour's enclosed area.
        /// </summary>
        /// <param name="contour">The 2d contour polygon.</param>
        private static float ContourSignedArea(Vector2WithUV[] contour)
        {
            float signedArea = 0f;
            for(int i = 0; i < contour.Length - 1; i++)
            {
                signedArea += contour[i].Vector.x * contour[i + 1].Vector.y - contour[i].Vector.y * contour[i + 1].Vector.x;
            }

            signedArea += contour[contour.Length - 1].Vector.x * contour[0].Vector.y - contour[contour.Length - 1].Vector.y * contour[0].Vector.x;

            return signedArea;
        }
    }
}