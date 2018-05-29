using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental
{
    /// <summary>
    /// Generates UV values for points based on their positions relative to their closest points to a segmentwise-defined line and its extruded contour.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public static class PointUVGenerationBetweenOriginalAndExtrudedPoints
    {
        /// <summary>
        /// Gets the uv parameters of a triangulated point when the segment between it and the closest point on the original line does not intersect any removed extruded segments.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point.</param>
        /// <param name="closestPointOnOriginalLine">The closest point on the original line to the triangulated point.</param>
        /// <param name="closestOriginalLineSegmentDirection">The direction of segment containing the closest point on the original line.</param>
        /// <param name="closestPointOnExtrudedContours">The closest point on the extruded contours to the triangulated point.</param>
        /// <param name="closestExtrudedContoursSegmentDirection">The direction of segment containing the closest point on the extruded contours.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance.</param>
        public static Vector2 GetTriangulatedPointUvFromClosestOriginalAndExtrudedSegments(Vector2 triangulatedPoint, LinePointUV closestPointOnOriginalLine, Vector2 closestOriginalLineSegmentDirection, Vector2WithUV closestPointOnExtrudedContours, Vector2 closestExtrudedContoursSegmentDirection, float extrusionAmountAbs)
        {
            Vector2 triangulatedToOrig = (triangulatedPoint - closestPointOnOriginalLine.Point);
            var origSegmentNormal = NormalUtil.NormalFromTangent(closestOriginalLineSegmentDirection);
            var origPointExtrusionDistanceFraction = Mathf.Clamp01(triangulatedToOrig.magnitude / extrusionAmountAbs);
            var vParamFromOrig = 0.5f + 0.5f * origPointExtrusionDistanceFraction * Mathf.Sign(Vector2.Dot(triangulatedToOrig, origSegmentNormal));
            //TODO Additional work is needed to make this work if the extruded point is too far or the angle is too high. 
            //Something more like the "connected segments" approach, or something like the closest line point by ray intersection from closest extruded point to the actual point, may help.
            var uParamFromOrig = closestPointOnOriginalLine.UV.x;

            Vector2 triangulatedToExtruded = (triangulatedPoint - closestPointOnExtrudedContours.Vector);
            var extrudedPointExtrusionDistanceFraction = 1f - Mathf.Clamp01(triangulatedToExtruded.magnitude / extrusionAmountAbs);
            var vParamFromExtruded = closestPointOnExtrudedContours.UV.y * extrudedPointExtrusionDistanceFraction + closestPointOnOriginalLine.UV.y * (1f - extrudedPointExtrusionDistanceFraction);
            var uParamFromExtruded = closestPointOnExtrudedContours.UV.x;

            var uParamFinal = uParamFromOrig * (1f - origPointExtrusionDistanceFraction) + uParamFromExtruded * origPointExtrusionDistanceFraction;
            var vParamFinal = vParamFromOrig * (1f - origPointExtrusionDistanceFraction) + vParamFromExtruded * origPointExtrusionDistanceFraction;

            return new Vector2(uParamFinal, vParamFinal);
        }
    }
}