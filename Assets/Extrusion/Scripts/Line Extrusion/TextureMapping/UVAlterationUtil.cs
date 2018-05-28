using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping
{
    /// <summary>
    /// Contains methods used commonly in different UV alteration calculations.
    /// </summary>
    public static class UVAlterationUtil
    {
        /// <summary>
        /// Returns the arcdistance of points of a closed extruded contour
        /// </summary>
        /// <param name="extrudedLinePoints">The points of a closed extruded contour</param>
        internal static float[] GetPointArcdistances(Vector2WithUV[] extrudedLinePoints)
        {
            float[] arcDistances = new float[extrudedLinePoints.Length];
            float totalArcDistance = 0f;
            for (int i = 1; i < arcDistances.Length; i++)
            {
                totalArcDistance += (extrudedLinePoints[i].Vector - extrudedLinePoints[i - 1].Vector).magnitude;
                arcDistances[i] = totalArcDistance;
            }
            return arcDistances;
        }

        /// <summary>
        /// Copies an array of points with override u-parameter values.
        /// </summary>
        /// <param name="points">The points</param>
        /// <param name="uParameters">The u parameters to use in the copied array</param>
        internal static Vector2WithUV[] CopyPointsWithOverriddenUParameters(Vector2WithUV[] points, float[] uParameters)
        {
            Vector2WithUV[] altered = new Vector2WithUV[points.Length];
            for (int i = 0; i < altered.Length; i++)
            {
                altered[i] = points[i];
                altered[i].UV.x = uParameters[i];
            }
            return altered;
        }

        /// <summary>
        /// Gets an array of u parameters from an array of extruded line points.
        /// </summary>
        /// <param name="extrudedLinePoints">The extruded line points</param>
        internal static float[] GetUParameters(Vector2WithUV[] extrudedLinePoints)
        {
            float[] uParameters = new float[extrudedLinePoints.Length];
            for (int i = 0; i < uParameters.Length; i++)
            {
                uParameters[i] = extrudedLinePoints[i].UV.x;
            }
            return uParameters;
        }
    }
}