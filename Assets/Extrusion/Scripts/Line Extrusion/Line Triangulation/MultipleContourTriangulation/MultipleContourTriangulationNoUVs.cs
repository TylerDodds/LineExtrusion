using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour
{
    /// <summary>
    /// Triangulates an outer extruded contour with inner hole contours, with no uvs.
    /// </summary>
    public class MultipleContourTriangulationNoUVs : MultipleContourTriangulationBase
    {
        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        public override bool HasUvAlteration
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Generates empty UV values for triangulated points on the interior of an extruded surface.
        /// </summary>
        /// <param name="triangulatedPoints">The triangulated points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointsList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GenerateTriangulatedPointUVs(Vector3[] triangulatedPoints, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointsList, LineExtrusionConfiguration extrusionConfiguration)
        {
            return new Vector2[triangulatedPoints.Length];
        }

        /// <summary>
        /// If original line points should be included in triangulation.
        /// </summary>
        protected override bool IncludeOriginalLinePoints
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// If removed extruded contours should be included in triangulation.
        /// </summary>
        protected override bool IncludeRemovedContours
        {
            get
            {
                return false;
            }
        }
    }
}