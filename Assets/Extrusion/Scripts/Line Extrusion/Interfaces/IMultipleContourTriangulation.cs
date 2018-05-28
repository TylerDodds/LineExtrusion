using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Interfaces
{
    /// <summary>
    /// Creates a mesh of a multiple-contour extruded line.
    /// </summary>
    public interface IMultipleContourTriangulation
    {
        /// <summary>
        /// Triangulate a polygon based on an outer closed contour of extruded points, further inner hole contours of extruded points, and the original line points from which they were extruded.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        Mesh TriangulatePolygonWithHoleContours(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration);

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        bool HasUvAlteration { get; }
    }
}
