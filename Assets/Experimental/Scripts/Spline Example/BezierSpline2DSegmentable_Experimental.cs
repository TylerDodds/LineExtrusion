using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.Configuration.Experimental;
using BabyDinoHerd.Extrusion.Line.Enums.Experimental;

namespace BabyDinoHerd.Extrusion.Spline.Experimental
{
    /// <summary>
    /// 2D spline that is segmentable, and is extruded to create a 2D mesh. Additional options for triangulation and texture mapping.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [BabyDinoHerd.Experimental]
    public class BezierSpline2DSegmentable_Experimental : BezierSpline2DSegmentable, ILineSegmentation
    {
        /// <summary> The method for determining triangulation of a single-contour extrusion result. </summary>
        public SingleContourTriangulationType SingleContourTriangulationType { get { return _singleContourTriangulationType; } }
        [SerializeField]
        private SingleContourTriangulationType _singleContourTriangulationType;

        /// <summary> The method of determining how the extruded line's UV u-parameter should be altered for a single contour. </summary>
        public UParameterAlterationType SingleContourUParameterAlterationType { get { return _singleContourUParameterAlterationType; } }
        [SerializeField]
        private UParameterAlterationType _singleContourUParameterAlterationType;

        /// <summary> The method for determining triangulation of multiple contours. </summary>
        public MultipleContourTriangulationType MultipleContourTriangulationType { get { return _multipleContourTriangulationType; } }
        [SerializeField]
        private MultipleContourTriangulationType _multipleContourTriangulationType;

        /// <summary> The method of determining how the extruded line's UV u-parameter should be altered for multiple contours. </summary>
        public UParameterAlterationType MultipleContoursUParameterDeterminationType { get { return _multipleContoursUParameterAlterationType; } }
        [SerializeField]
        private UParameterAlterationType _multipleContoursUParameterAlterationType;

        /// <summary> The method for determining how UV parameters of extruded intersection points are calculated. </summary>
        public IntersectionUVCalculation IntersectionUVCalculation { get { return _intersectionUVCalculation; } }
        [SerializeField]
        private IntersectionUVCalculation _intersectionUVCalculation;

        /// <summary>
        /// Returns the <see cref="LineExtrusionConfiguration"/> to be used for extrusion.
        /// </summary>
        /// <param name="extrusion">Extrusion amount.</param>
        /// 
        protected override LineExtrusionConfiguration GetLineExtrusionConfiguration(float extrusion)
        {
            return new LineExtrusionConfiguration_Experimental(extrusion, SingleContourTriangulationType, SingleContourUParameterAlterationType, MultipleContourTriangulationType, MultipleContoursUParameterDeterminationType, IntersectionUVCalculation);
        }
    }
}