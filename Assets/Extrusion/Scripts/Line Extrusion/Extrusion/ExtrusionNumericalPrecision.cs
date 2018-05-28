using BabyDinoHerd.Extrusion.Line.Geometry;
using System;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    /// <summary>
    /// Class for handling extrusion numerical precision-related issues.
    /// </summary>
    internal static class ExtrusionNumericalPrecision
    {
        /// <summary>
        /// The minimum extrusion distance required to attempt to create an extruded surface.
        /// </summary>
        internal static float MinimumExtrusionDistanceExclusive = 0f;

        /// <summary>
        /// Epsilon (in same units as <paramref name="extrusionAmount"/>) for a point not being at the expected extrusion distance.
        /// </summary>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        internal static float GetExtrusionDistanceEpsilon(float extrusionAmount)
        {
            return Math.Abs(extrusionAmount * 5e-7f);
        }

        /// <summary>
        /// Returns if two intersection points are same point geometrically.
        /// </summary>
        /// <param name="intersectionPoint1">First intersection point</param>
        /// <param name="intersectionPoint2">Second intersection point</param>
        internal static bool IntersectionPointsAreGeometricallyIdentical(IntersectionPoint intersectionPoint1, IntersectionPoint intersectionPoint2)
        {
            return intersectionPoint1.Point == intersectionPoint2.Point;
            //Since this is enforced when determining intersection points, we don't need a distance-epsilon check.
        }
    }
}