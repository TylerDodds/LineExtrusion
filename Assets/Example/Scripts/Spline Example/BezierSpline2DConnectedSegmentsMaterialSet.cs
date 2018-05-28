using UnityEngine;
using BabyDinoHerd.Extrusion.LineMaterial;

namespace BabyDinoHerd.Extrusion.Spline
{
    /// <summary>
    /// Component that sets the GameObject's MeshRenderer's sharedMaterial with connected segments results of <see cref="BezierSpline2DSegmentable"/> line extrusion.
    /// </summary>
    [RequireComponent(typeof(BezierSpline2DSegmentable))]
    public class BezierSpline2DConnectedSegmentsMaterialSet : ConnectedSegmentMaterialSet
    {
        /// <summary>
        /// Gets the extrusion configuration and results.
        /// </summary>
        protected override ExtrusionConfigurationAndResults GetLineExtrusionConfigurationAndResults()
        {
            var spline = BezierSpline2D;
            var lineExtrusionResults = spline.ExtrusionResults;
            var configuration = spline.LineExtrusionConfiguration;
            return new ExtrusionConfigurationAndResults(configuration, lineExtrusionResults);
        }

        /// <summary> The <see cref="BezierSpline2DSegmentable"/> component of the GameObject. </summary>
        private BezierSpline2DSegmentable BezierSpline2D
        {
            get
            {
                if(_bezierSpline2D == null)
                {
                    _bezierSpline2D = GetComponent<BezierSpline2DSegmentable>();
                }
                return _bezierSpline2D;
            }
        }
        private BezierSpline2DSegmentable _bezierSpline2D;
    }
}