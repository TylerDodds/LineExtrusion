using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Curvature.Experimental;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration.Experimental
{
    /// <summary>
    /// Alters uv parameters of extruded points in some attempt to avoid pinching of u parameter when a line intersects with itself (and similar issues).
    /// Uses points where line curvature rises and falls and stretches u-parameters evenly (based on arcdistance) between midpoints thereof.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class CurvatureUVStretchingRiseAndFall : CurvatureUVStretchingBase
    {
        /// <summary>
        /// Gets the set of u-parameters used as anchors to stretch <paramref name="extrudedLinePoints"/> between.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line</param>
        protected override List<float> GetUParametersToStretchBetween(IList<Vector2WithUV> extrudedLinePoints)
        {
            return CurvatureUParameterDetermination.GetUParameterBetweenCurvatureFallAndRise(extrudedLinePoints, curvatureAngleCutoffDegrees: 3f);
        }
    }
}