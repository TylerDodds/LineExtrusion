using BabyDinoHerd.Extrusion.Line.Enums.Experimental;
using BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection;
using BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection.Experimental;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration.Experimental;
using BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour;
using BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour.Experimental;
using BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour;
using BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Configuration.Experimental
{
    /// <summary>
    /// Configuration class for line extrusion parameters, particularly the <see cref="ExtrusionAmount"/>.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class LineExtrusionConfiguration_Experimental : LineExtrusionConfiguration
    {
        /// <summary>
        /// The method for determining uv parameters for points of a single-contour extrusion result.
        /// </summary>
        public SingleContourTriangulationType SingleContourLineUVDeterminationType = SingleContourTriangulationType.Default;

        /// <summary>
        /// The method for determining u parameters used in UV alteration for a single contour.
        /// </summary>
        public UParameterAlterationType SingleContourUParameterAlterationType = UParameterAlterationType.Default;

        /// <summary>
        /// The method for determining uv parameters for triangulated points coming from a multiple-contour extrusion result.
        /// </summary>
        public MultipleContourTriangulationType TriangulatedPointsUVDeterminationType = MultipleContourTriangulationType.Default;

        /// <summary>
        /// The method for determining u parameters used in UV alteration for multiple contours.
        /// </summary>
        public UParameterAlterationType MultipleContoursUParameterAlterationType = UParameterAlterationType.Default;

        /// <summary>
        /// The method for determining how UV parameters of extruded intersection points are calculated.
        /// </summary>
        public IntersectionUVCalculation IntersectionUVCalculation = IntersectionUVCalculation.Default;

        #region Implementations

        /// <summary> Returns an implementation of <see cref="IExtrudedChunkContourConnector"/> based on <see cref="IntersectionUVCalculation"/> parameter. </summary>
        public override IExtrudedChunkContourConnector GetExtrudedChunkContourConnector()
        {
            IExtrudedChunkContourConnector contourConnector = null;
            switch(IntersectionUVCalculation)
            {
                case IntersectionUVCalculation.KeepEndPointUV:
                    contourConnector = new ExtrudedChunkContourConnector_KeepEnd();
                    break;
                case IntersectionUVCalculation.KeepStartPointUV:
                    contourConnector = new ExtrudedChunkContourConnector_KeepStart();
                    break;
                case IntersectionUVCalculation.AveragedFromArcdistance:
                    contourConnector = new ExtrudedChunkContourConnector_AveragedArcdistance();
                    break;
                case IntersectionUVCalculation.Default:
                case IntersectionUVCalculation.AveragedEqually:
                default:
                    contourConnector = new ExtrudedChunkContourConnector_AveragedEqually();
                    break;
            }
            return contourConnector;
        }

        /// <summary> Returns an implementation of <see cref="IExtrudedContourUVAlteration"/> based on <see cref="SingleContourUParameterAlterationType"/> value. </summary>
        public override IExtrudedContourUVAlteration GetSingleContourUVAlteration()
        {
            return GetContourUVAlteration(SingleContourUParameterAlterationType);
        }

        /// <summary> Returns an implementation of <see cref="IExtrudedContourUVAlteration"/> based on <see cref="MultipleContoursUParameterAlterationType"/> value. </summary>
        public override IExtrudedContourUVAlteration GetMultipleContourUVAlteration()
        {
            return GetContourUVAlteration(MultipleContoursUParameterAlterationType);
        }

        /// <summary> Returns an implementation of <see cref="IExtrudedContourUVAlteration"/> based on <paramref name="uParameterAlterationType"/> parameter. </summary>
        /// <param name="uParameterAlterationType">The method of altering u-parameters.</param>
        private static IExtrudedContourUVAlteration GetContourUVAlteration(UParameterAlterationType uParameterAlterationType)
        {
            IExtrudedContourUVAlteration alteration = null;
            switch (uParameterAlterationType)
            {
                case UParameterAlterationType.CurvatureInflectionPoints:
                    alteration = new CurvatureUVStretchingInflectionPoints();
                    break;
                case UParameterAlterationType.CurvatureLargeValueMidpoints:
                    alteration = new CurvatureUVStretchingLargeValues();
                    break;
                case UParameterAlterationType.CurvatureLocalMaxima:
                    alteration = new CurvatureUVStretchingLocalMaxima();
                    break;
                case UParameterAlterationType.CurvatureRiseAndFall:
                    alteration = new CurvatureUVStretchingRiseAndFall();
                    break;
                case UParameterAlterationType.SpringApproximation:
                    alteration = new SpringApproximationExtrudedUVAlteration();
                    break;
                case UParameterAlterationType.Default:
                case UParameterAlterationType.Convolution:
                default:
                    alteration = new ConvolutionUVAlteration();
                    break;
            }
            return alteration;
        }

        /// <summary> Returns an implementation of <see cref="ISingleContourTriangulation"/> based on <see cref="SingleContourLineUVDeterminationType"/> parameter. </summary>
        public override ISingleContourTriangulation GetSingleContourTriangulation()
        {
            ISingleContourTriangulation singleContourTriangulation;
            switch(SingleContourLineUVDeterminationType)
            {
                case SingleContourTriangulationType.OriginalLineAndAlteredExtrudedParameters:
                    singleContourTriangulation = new SingleContourTriangulationBetweenOriginalAndExtrudedPoints();
                    break;
                case SingleContourTriangulationType.OriginalLineWeightedSegments:
                    singleContourTriangulation = new SingleContourTriangulationWeightedFromOriginalLinePoints();
                    break;
                case SingleContourTriangulationType.OriginalLineCurvature:
                    singleContourTriangulation = new SingleContourTriangulationUVFromOriginalLineCurvature();
                    break;
                case SingleContourTriangulationType.ConnectedSegmentsOriginalLineAndAlteredExtrudedParameters:
                case SingleContourTriangulationType.Default:
                default:
                    singleContourTriangulation = new SingleContourTriangulationFromConnectedSegments();
                    break;
            }
            return singleContourTriangulation;
        }

        /// <summary> Returns an implementation of <see cref="IMultipleContourTriangulation"/> based on <see cref="TriangulatedPointsUVDeterminationType"/> parameter. </summary>
        public override IMultipleContourTriangulation GetMultipleContourTriangulation()
        {
            IMultipleContourTriangulation multipleContourTriangulation;
            switch (TriangulatedPointsUVDeterminationType)
            {
                case MultipleContourTriangulationType.OriginalAndExtrudedPoints:
                    multipleContourTriangulation = new MultipleContourTriangulationUVFromOriginalAndExtrudedPointsAlteredUVs();
                    break;
                case MultipleContourTriangulationType.OriginalAndExtrudedPointsAnglewiseWeighted:
                    multipleContourTriangulation = new MultipleContourTriangulationUVFromOriginalAndExtrudedPointsAnglewiseWeighted();
                    break;
                case MultipleContourTriangulationType.OriginalLineCurvature:
                    multipleContourTriangulation = new MultipleContourTriangulationUVFromOriginalLineCurvature();
                    break;
                case MultipleContourTriangulationType.OriginalLineWeightedSegments:
                    multipleContourTriangulation = new MultipleContourTriangulationWeightedFromOriginalLinePoints();
                    break;
                case MultipleContourTriangulationType.NoUVs:
                case MultipleContourTriangulationType.Default:
                default:
                    multipleContourTriangulation = new MultipleContourTriangulationNoUVs();
                    break;
            }
            return multipleContourTriangulation;
        }

        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="LineExtrusionConfiguration"/>.
        /// </summary>
        /// <param name="extrusionAmount">The distance for points to be extruded.</param>
        /// <param name="singleContourTriangulationType">The method for triangulating points of a single-contour extrusion.</param>
        /// <param name="singleContourUParameterAlterationType">The method for determining u parameters used in UV alteration for a single contour.</param>
        /// <param name="multipleContourTriangulationType">The method for triangulating points of a multiple-contoru extrusion.</param>
        /// <param name="multipleContoursUParameterAlterationType">The method for determining u parameters used in UV alteration for multiple contours.</param>
        /// <param name="intersectionUVCalculation">The method for determining how UV parameters of extruded intersection points are calculated.</param>
        public LineExtrusionConfiguration_Experimental(float extrusionAmount, SingleContourTriangulationType singleContourTriangulationType, UParameterAlterationType singleContourUParameterAlterationType, MultipleContourTriangulationType multipleContourTriangulationType, UParameterAlterationType multipleContoursUParameterAlterationType, IntersectionUVCalculation intersectionUVCalculation)
            : base(extrusionAmount)
        {
            SingleContourLineUVDeterminationType = singleContourTriangulationType;
            SingleContourUParameterAlterationType = singleContourUParameterAlterationType;
            TriangulatedPointsUVDeterminationType = multipleContourTriangulationType;
            MultipleContoursUParameterAlterationType = multipleContoursUParameterAlterationType;
            IntersectionUVCalculation = intersectionUVCalculation;
        }
    }
}