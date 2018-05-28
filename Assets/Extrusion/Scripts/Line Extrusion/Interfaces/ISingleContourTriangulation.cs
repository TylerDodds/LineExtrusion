using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Interfaces
{
    /// <summary>
    /// Creates a mesh of a single-contour extruded line.
    /// </summary>
    public interface ISingleContourTriangulation
    {
        /// <summary>
        /// Triangulate a line based on a single closed contour of extruded points and the original line points from which they were extruded.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        Mesh TriangulateClosedContour(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration);

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        bool HasUvAlteration { get; }
    }
}
