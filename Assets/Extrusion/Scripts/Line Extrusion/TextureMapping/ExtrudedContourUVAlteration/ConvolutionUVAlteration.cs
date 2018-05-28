using BabyDinoHerd.Extrusion.Line.Geometry;
using System;
using BabyDinoHerd.Utility;
using BabyDinoHerd.Extrusion.Line.Interfaces;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration
{
    /// <summary>
    /// Alters uv parameters of extruded points in some attempt to avoid pinching of u parameter when a line intersects with itself (and similar issues).
    /// Uses arcdistance-based convolution filtering.
    /// </summary>
    public class ConvolutionUVAlteration : IExtrudedContourUVAlteration
    {
        /// <summary>
        /// Delegate returning the integral of a convolution function from <paramref name="pointArcDistance"/>-<paramref name="segmentEndArcDistance"/> to <paramref name="pointArcDistance"/>-<paramref name="segmentStartArcDistance"/>.
        /// </summary>
        /// <param name="pointArcDistance">Arc-distance of the reference point of the center of the convolution.</param>
        /// <param name="segmentStartArcDistance">Arc-distance of the start of the segment being integrated over.</param>
        /// <param name="segmentEndArcDistance">Arc-distance of the end of the segment being integrated over.</param>
        delegate double ConvolutionFunctionIntegral(float pointArcDistance, float segmentStartArcDistance, float segmentEndArcDistance);


        /// <summary>
        /// Returns a new list of points and uvs of an extruded contour, where the uv u-parameters have been undergone convolution filtering to try to smooth out discontinuities.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line contour</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        public Vector2WithUV[] GetUvAlteredExtrudedContour(Vector2WithUV[] extrudedLinePoints, float extrusionAmount)
        {
            Vector2WithUV[] altered;

            float sigma = _convolutionWidthExtrusionDistanceFraction * extrusionAmount;

            //Integral from segmentStartArcDistance to segmentEndArcDistance of G(pointArcDistance - x')dx'
            ConvolutionFunctionIntegral gaussianConvolutionIntegral = (pointArcDistance, segmentStartArcDistance, segmentEndArcDistance) => GaussianConvolutionFunctionIntegral(pointArcDistance, segmentStartArcDistance, segmentEndArcDistance, sigma);
            //Integral from pointArcDistance - segmentEndArcDistance to pointArcDistance - segmentStartArcDistance of x'G(x')dx'
            ConvolutionFunctionIntegral gaussianLinearConvolutionIntegral = (pointArcDistance, segmentStartArcDistance, segmentEndArcDistance) => GaussianLinearConvolutionFunctionIntegral(pointArcDistance, segmentStartArcDistance, segmentEndArcDistance, sigma);

            if (extrudedLinePoints.Length > 2)
            {
                bool isPeriodic = extrudedLinePoints[0].Equals(extrudedLinePoints[extrudedLinePoints.Length - 1]);
                float[] uParameters = GetConvolutionUParameters(extrudedLinePoints, isPeriodic, gaussianConvolutionIntegral, gaussianLinearConvolutionIntegral);
                altered = UVAlterationUtil.CopyPointsWithOverriddenUParameters(extrudedLinePoints, uParameters);
            }
            else
            {
                altered = extrudedLinePoints;
            }

            return altered;
        }

        /// <summary>
        /// Returns the u-parameters of an extruded contour determined by arcdistance convolution integral.
        /// </summary>
        /// <param name="contourPoints">The contour points.</param>
        /// <param name="isPeriodic">If the contour is periodic.</param>
        /// <param name="convolutionFunctionIntegral">Function for computing arcdistance integral of the convolution function.</param>
        /// <param name="convolutionLinearFunctionIntegral">Function for computing arcdistance integral of the convolution function multiplied by arcdistance.</param>
        private static float[] GetConvolutionUParameters(Vector2WithUV[] contourPoints, bool isPeriodic, ConvolutionFunctionIntegral convolutionFunctionIntegral, ConvolutionFunctionIntegral convolutionLineareFunctionIntegral)
        {
            int pointCount = isPeriodic ? contourPoints.Length - 1 : contourPoints.Length;
            float[] uParameters = UVAlterationUtil.GetUParameters(contourPoints);
            float[] arcDistances = UVAlterationUtil.GetPointArcdistances(contourPoints);
            float[] uParametersConvolved = new float[contourPoints.Length];

            for(int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                float pointArcDistance = arcDistances[pointIndex];
                uParametersConvolved[pointIndex] = GetConvolutionUParameter(pointArcDistance, uParameters, arcDistances, isPeriodic, convolutionFunctionIntegral, convolutionLineareFunctionIntegral);
            }

            if(isPeriodic)
            {
                uParametersConvolved[contourPoints.Length - 1] = uParametersConvolved[0];
            }

            return uParametersConvolved;
        }

        /// <summary>
        /// Gets the u-parameter of a point determined by arcdistance convolution integral along a contour.
        /// </summary>
        /// <param name="pointArcDistance">The point's arcdistance.</param>
        /// <param name="uParameters">The u-parameters of the contour.</param>
        /// <param name="arcDistances">The arcdistances of the contour.</param>
        /// <param name="isPeriodic">If the contour is periodic.</param>
        /// <param name="convolutionFunctionIntegral">Function for computing arcdistance integral of the convolution function.</param>
        /// <param name="convolutionLinearFunctionIntegral">Function for computing arcdistance integral of the convolution function multiplied by arcdistance.</param>
        private static float GetConvolutionUParameter(float pointArcDistance, float[] uParameters, float[] arcDistances, bool isPeriodic, ConvolutionFunctionIntegral convolutionFunctionIntegral, ConvolutionFunctionIntegral convolutionLinearFunctionIntegral)
        {
            if(isPeriodic)
            {
                arcDistances = PeriodicallyWrappedArcDistances(pointArcDistance, arcDistances);
                pointArcDistance = 0;
            }

            double convolutionSum = 0;

            for(int segmentIndex = 0; segmentIndex < uParameters.Length - 1; segmentIndex++)
            {
                float uStart = uParameters[segmentIndex];
                float uEnd = uParameters[segmentIndex + 1];
                float arcDistanceSegmentStart = arcDistances[segmentIndex];
                float arcDistanceSegmentEnd = arcDistances[segmentIndex + 1];
                float segmentLength = arcDistanceSegmentEnd - arcDistanceSegmentStart;
                float uDifference = uEnd - uStart;

                //In periodic case, we'll ignore the 'furthest-away' segment regardless of how large the convolution kernel is, since we'd need to chop it in two to deal with periodically wrapped arcdistances properly.
                if (segmentLength > 0)
                {
                    var integralConvolutionFunction = convolutionFunctionIntegral(pointArcDistance, arcDistanceSegmentStart, arcDistanceSegmentEnd);
                    var integralConvolutionLinearFunction = convolutionLinearFunctionIntegral(pointArcDistance, arcDistanceSegmentStart, arcDistanceSegmentEnd);

                    var convolutionIntegralFactor = uStart + uDifference * (pointArcDistance - arcDistanceSegmentStart) / segmentLength;
                    var convolutionLinearIntegralFactor = -uDifference / segmentLength;

                    var contribution = convolutionIntegralFactor * integralConvolutionFunction + convolutionLinearIntegralFactor * integralConvolutionLinearFunction;

                    convolutionSum += contribution;
                }
            }

            return (float)convolutionSum;
        }

        /// <summary>
        /// Returns the closest arc-distances with respect to a given <paramref name="pointArcDistance"/> for a closed, periodic contour.
        /// </summary>
        /// <param name="pointArcDistance">The reference point arc-distance.</param>
        /// <param name="arcDistances">The initial arc-distances of the contour.</param>
        private static float[] PeriodicallyWrappedArcDistances(float pointArcDistance, float[] arcDistances)
        {
            int fullNumPoints = arcDistances.Length;
            float[] ret = new float[fullNumPoints];
            float fullArcLength = arcDistances[fullNumPoints - 1];
            float halfArcLength = 0.5f * fullArcLength;
            for (int i = 0; i < fullNumPoints; i++)
            {
                float currentDist = arcDistances[i] - pointArcDistance;
                while(currentDist < -halfArcLength)
                {
                    currentDist += fullArcLength;
                }
                while (currentDist > halfArcLength)
                {
                    currentDist -= fullArcLength;
                }
                ret[i] = currentDist;
            }
            return ret;
        }

        /// <summary>
        /// Returns the integral of g(x)dx, where g(x) is a Gaussian of standard deviation <paramref name="sigma"/>, from <paramref name="pointArcDistance"/>-<paramref name="segmentEndArcDistance"/> to <paramref name="pointArcDistance"/>-<paramref name="segmentStartArcDistance"/>.
        /// </summary>
        /// <param name="pointArcDistance">Arc-distance of the reference point of the center of the convolution.</param>
        /// <param name="segmentStartArcDistance">Arc-distance of the start of the segment being integrated over.</param>
        /// <param name="segmentEndArcDistance">Arc-distance of the end of the segment being integrated over.</param>
        /// <param name="sigma">Standard deviation of the Gaussian function.</param>
        private static double GaussianConvolutionFunctionIntegral(float pointArcDistance, float segmentStartArcDistance, float segmentEndArcDistance, float sigma)
        {
            var rootTwoSigma = sigma * Math.Sqrt(2);
            return 0.5 * (GaussianErrorFunction.Erf((pointArcDistance - segmentStartArcDistance) / rootTwoSigma) - GaussianErrorFunction.Erf((pointArcDistance - segmentEndArcDistance) / rootTwoSigma));
        }

        /// <summary>
        /// Returns the integral of x*g(x)dx, where g(x) is a Gaussian of standard deviation <paramref name="sigma"/>, from <paramref name="pointArcDistance"/>-<paramref name="segmentEndArcDistance"/> to <paramref name="pointArcDistance"/>-<paramref name="segmentStartArcDistance"/>.
        /// </summary>
        /// <param name="pointArcDistance">Arc-distance of the reference point of the center of the convolution.</param>
        /// <param name="segmentStartArcDistance">Arc-distance of the start of the segment being integrated over.</param>
        /// <param name="segmentEndArcDistance">Arc-distance of the end of the segment being integrated over.</param>
        /// <param name="sigma">Standard deviation of the Gaussian function.</param>
        private static double GaussianLinearConvolutionFunctionIntegral(float pointArcDistance, float segmentStartArcDistance, float segmentEndArcDistance, float sigma)
        {
            float twoSigmaSquared = 2 * sigma * sigma;
            return (sigma / Math.Sqrt(2 * Math.PI)) * (Math.Exp(-Math.Pow(pointArcDistance - segmentEndArcDistance, 2) / twoSigmaSquared) - Math.Exp(-Math.Pow(pointArcDistance - segmentStartArcDistance, 2) / twoSigmaSquared));
        }

        /// <summary> Fraction of extrusion distance to use as convolution width. </summary>
        private const float _convolutionWidthExtrusionDistanceFraction = 1f;
    }
}