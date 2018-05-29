using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental
{
    /// <summary>
    /// Contains methods used commonly in different UV alteration calculations.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public static class UVAlterationUtil_Experimental
    {
        /// <summary>
        /// Returns the length of segments defined by the points of a closed extruded contour, multiplied by the sign of the difference of the u-parameters of the segment endpoints
        /// </summary>
        /// <param name="segmentLengths">The unsigned segment lengths </param>
        /// <param name="uParameters">The u-parameters of the segment points </param>
        internal static float[] GetSignedSegmentLengths(float[] segmentLengths, float[] uParameters)
        {
            int pointCount = segmentLengths.Length;
            float[] signedSegmentLengths = new float[pointCount];
            if (pointCount > 0)
            {
                float uCurr = uParameters[0];
                for (int i = 1; i < pointCount; i++)
                {
                    float uNext = uParameters[(i) % pointCount];
                    var signCurrNext = Mathf.Sign(uNext - uCurr);
                    signedSegmentLengths[i - 1] = segmentLengths[i - 1] * signCurrNext;
                    uCurr = uNext;
                }
            }

            return signedSegmentLengths;
        }

        /// <summary>
        /// Returns the length of segments defined by the points of a closed extruded contour
        /// </summary>
        /// <param name="extrudedLinePoints">The points of a closed extruded contour</param>
        internal static float[] GetSegmentLengths(Vector2WithUV[] extrudedLinePoints)
        {
            float[] segmentLengths = new float[extrudedLinePoints.Length];
            for (int i = 0; i < segmentLengths.Length - 1; i++)
            {
                segmentLengths[i] = (extrudedLinePoints[i + 1].Vector - extrudedLinePoints[i].Vector).magnitude;
            }
            segmentLengths[segmentLengths.Length - 1] = (extrudedLinePoints[segmentLengths.Length - 1].Vector - extrudedLinePoints[0].Vector).magnitude;
            return segmentLengths;
        }
    }
}