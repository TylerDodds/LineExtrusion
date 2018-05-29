using BabyDinoHerd.Extrusion.Line.Geometry;
using System;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration.Experimental
{
    /// <summary>
    /// Alters uv parameters of extruded points in some attempt to avoid pinching of u parameter when a line intersects with itself (and similar issues).
    /// Uses a spring approximation to have neighbouring points u-parameters most closely match the difference between those points.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class SpringApproximationExtrudedUVAlteration : IExtrudedContourUVAlteration
    {
        private delegate float GetChangeInCurrentUParameter(float uCurr, float uPrev, float uNext, float signedLengthCurrToNext, float signedLengthPrevToCurr);

        /// <summary>
        /// Returns a new list of points and uvs of an extruded contour, where the uv u-parameters have been undergone convolution filtering to try to smooth out discontinuities.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line contour</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        public Vector2WithUV[] GetUvAlteredExtrudedContour(Vector2WithUV[] extrudedLinePoints, float extrusionAmount)
        {
            return GetUvAlteredExtrudedLine(extrudedLinePoints);
        }

        /// <summary>
        /// Returns a new list of points and uvs of an extruded contour, where the uv u-parameters have been altered to attempt to space them proportionally to segment distance.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line contour</param>
        private static Vector2WithUV[] GetUvAlteredExtrudedLine(Vector2WithUV[] extrudedLinePoints)
        {
            Vector2WithUV[] altered;

            if (extrudedLinePoints.Length > 2)
            {
                bool isPeriodic = extrudedLinePoints[0].Equals(extrudedLinePoints[extrudedLinePoints.Length - 1]);
                //NB Of the possible methods (shift toward ground state, dynamic spring approximation), all seem to struggle with small segments next to large segments, so there is no 'best choice' of them.
                float[] uParameters = GetApproximatedUParameters_ShiftTowardGroundState(extrudedLinePoints, isPeriodic, GetChangeInCurrentUParameter_SpringDistanceApproximation);
                altered = UVAlterationUtil.CopyPointsWithOverriddenUParameters(extrudedLinePoints, uParameters);
            }
            else
            {
                altered = extrudedLinePoints;
            }

            return altered;
        }

        /// <summary>
        /// Get u-parameters of a set of extruded points by trying to have u-parameter differences from point to point match the distance between those points.
        /// Iteratively shifts u-parameters to match neighbouring segment lengths.
        /// </summary>
        /// <param name="extrudedLinePoints"> The extruded points </param>
        /// <param name="isPeriodic">If the contour is periodic (last point is a repeat of the first point).</param>
        /// <param name="deltaUParameterFunction">Function for calculating change in current u parameter</param>
        private static float[] GetApproximatedUParameters_ShiftTowardGroundState(Vector2WithUV[] extrudedLinePoints, bool isPeriodic, GetChangeInCurrentUParameter deltaUParameterFunction)
        {
            int pointCount = isPeriodic ? extrudedLinePoints.Length - 1 : extrudedLinePoints.Length;
            float[] uParameters = UVAlterationUtil.GetUParameters(extrudedLinePoints);
            float[] segmentLengths = UVAlterationUtil_Experimental.GetSegmentLengths(extrudedLinePoints);
            float[] signedSegmentLengths = UVAlterationUtil_Experimental.GetSignedSegmentLengths(segmentLengths, uParameters);

            float[] uParametersDelta = new float[pointCount];

            bool continueIteration = true;
            int numIterations = 0;
            while (continueIteration)
            {
                for (int i = 0; i < pointCount; i++)
                {
                    float uCurr = uParameters[i];
                    float uPrev = uParameters[(i - 1 + pointCount) % pointCount];
                    float uNext = uParameters[(i + 1) % pointCount];
                    float signedLengthCurrToNext = signedSegmentLengths[i];
                    float signedLengthPrevToCurr = signedSegmentLengths[(i - 1 + pointCount) % pointCount];
                    float deltaUCurr = deltaUParameterFunction(uCurr, uPrev, uNext, signedLengthCurrToNext, signedLengthPrevToCurr);
                    uParametersDelta[i] = deltaUCurr;
                }

                for (int i = 0; i < pointCount; i++)
                {
                    uParameters[i] += uParametersDelta[i];
                }

                numIterations++;
                continueIteration = ContinueIteration(numIterations, uParametersDelta);
            }

            if(isPeriodic)
            {
                uParameters[pointCount - 1] = uParameters[0];
            }

            return uParameters;
        }

        /// <summary>
        /// Gets the single-step change in current point u-parameter based on minimizing quadratic function difference between u-parameters and signed segment length fractions.
        /// </summary>
        /// <param name="uCurr"> U parameter at current point </param>
        /// <param name="uPrev"> U parameter at previous point</param>
        /// <param name="uNext"> U parameter at next point </param>
        /// <param name="signedLengthCurrToNext"> Length from current to next point, multiplied by initial sign of difference of u parameters </param>
        /// <param name="signedLengthPrevToCurr"> Length from previous to current point, multiplied by initial sign of difference of u parameters </param>
        private static float GetChangeInCurrentUParameter_FractionalLengthApproximation(float uCurr, float uPrev, float uNext, float signedLengthCurrToNext, float signedLengthPrevToCurr)
        {
            //solving something like [(u_i+1 - u_i)/l_i,i+1 - 1)*(-1/l_i,i+1) + (u_i - u_i-1)/l_i-1,i - 1)*(1/l_i-1,i)] = 0 where l is (initial) signed length of segment
            //coming from minimization of [(u_i+1 - u_i)/l_i,i+1 - 1]^2 for all i
            float squaredLengthPrevToCurr = signedLengthPrevToCurr * signedLengthPrevToCurr;
            float squaredLengthCurrToNext = signedLengthCurrToNext * signedLengthCurrToNext;
            float multiplicativeLengthsFactor = (squaredLengthPrevToCurr * squaredLengthCurrToNext) / (squaredLengthPrevToCurr + squaredLengthCurrToNext);
            float otherParametersUFactor = uNext / squaredLengthCurrToNext + uPrev / squaredLengthPrevToCurr - 1f / signedLengthCurrToNext + 1f / signedLengthPrevToCurr;
            float targetUParameterFromMinimization = otherParametersUFactor * multiplicativeLengthsFactor;
            float difference = targetUParameterFromMinimization - uCurr;
            float deltaUCurr = difference * 0.05f;//Want some stable convergence towards a solution
            return deltaUCurr;
        }

        /// <summary>
        /// Gets the single-step change in current point u-parameter based on minimizing quadratic function difference between u-parameters and signed segment lengths.
        /// </summary>
        /// <param name="uCurr"> U parameter at current point </param>
        /// <param name="uPrev"> U parameter at previous point</param>
        /// <param name="uNext"> U parameter at next point </param>
        /// <param name="signedLengthCurrToNext"> Length from current to next point, multiplied by initial sign of difference of u parameters </param>
        /// <param name="signedLengthPrevToCurr"> Length from previous to current point, multiplied by initial sign of difference of u parameters </param>
        private static float GetChangeInCurrentUParameter_SpringDistanceApproximation(float uCurr, float uPrev, float uNext, float signedLengthCurrToNext, float signedLengthPrevToCurr)
        {
            //solving something like ui = u_i-1 - u_i + u_i+1 - u_i + L_pc*Sign_(u_c - u_p) - L_cn*Sign(u_n - u_c)
            //Except we will keep the signs of neighbouring u parameters fixed from the initial conditions: ui = u_i-1 - 2 * u_i + u_i+1 + L_pc*Sign_(u_0,c - u_0,p) - L_cn*Sign(u_0,n - u_0,c)
            float deltaUCurr = (uPrev - 2 * uCurr + uNext + signedLengthPrevToCurr - signedLengthCurrToNext);
            deltaUCurr *= 0.05f;//Want some stable convergence towards a solution
            return deltaUCurr;
        }

        /// <summary>
        /// Get u-parameters of a set of extruded points by trying to have u-parameter differences from point to point match the distance between those points.
        /// Uses a dynamic spring approximation.
        /// </summary>
        /// <param name="extrudedLinePoints"> The extruded points </param>
        /// <param name="isPeriodic">If the contour is periodic (last point is a repeat of the first point).</param>
        private static float[] GetApproximatedUParameters_DynamicSpring(Vector2WithUV[] extrudedLinePoints, bool isPeriodic)
        {
            int pointCount = isPeriodic ? extrudedLinePoints.Length - 1 : extrudedLinePoints.Length;
            float[] uParameters = UVAlterationUtil.GetUParameters(extrudedLinePoints);
            float[] segmentLengths = UVAlterationUtil_Experimental.GetSegmentLengths(extrudedLinePoints);
            float[] signedSegmentLengths = UVAlterationUtil_Experimental.GetSignedSegmentLengths(segmentLengths, uParameters);

            float[] uParametersDelta = new float[pointCount];
            float[] uParametersVelocities = new float[pointCount];

            bool continueIteration = true;
            int numIterations = 0;
            while (continueIteration)
            {
                for (int i = 0; i < pointCount; i++)
                {
                    float uCurr = uParameters[i];
                    float uPrev = uParameters[(i - 1 + pointCount) % pointCount];
                    float uNext = uParameters[(i + 1) % pointCount];
                    float signedLengthCurrToNext = signedSegmentLengths[i];
                    float signedLengthPrevToCurr = signedSegmentLengths[(i - 1 + pointCount) % pointCount];

                    float uVelCurr = uParametersVelocities[i];

                    float forceTotal = GetDampedSpringForce(uCurr, uPrev, uNext, signedLengthCurrToNext, signedLengthPrevToCurr, uVelCurr);
                    uParametersVelocities[i] = uVelCurr + forceTotal * _dynamicSpringPseudoTimeStep;
                    uParametersDelta[i] = uParametersVelocities[i] * _dynamicSpringPseudoTimeStep;//Want some stable convergence towards a solution
                }

                for (int i = 0; i < pointCount; i++)
                {
                    uParameters[i] += uParametersDelta[i];
                }

                numIterations++;
                continueIteration = ContinueIteration(numIterations, uParametersDelta);
            }

            if(isPeriodic)
            {
                uParameters[pointCount - 1] = uParameters[0];
            }

            return uParameters;
        }

        /// <summary>
        /// Gets the damped spring approximation "force" on u-parameters
        /// </summary>
        /// <param name="uCurr"> U parameter at current point </param>
        /// <param name="uPrev"> U parameter at previous point</param>
        /// <param name="uNext"> U parameter at next point </param>
        /// <param name="signedLengthCurrToNext"> Length from current to next point, multiplied by initial sign of difference of u parameters </param>
        /// <param name="signedLengthPrevToCurr"> Length from previous to current point, multiplied by initial sign of difference of u parameters </param>
        /// <param name="uVelCurr"> Velocity of u-parameter at current point </param>
        private static float GetDampedSpringForce(float uCurr, float uPrev, float uNext, float signedLengthCurrToNext, float signedLengthPrevToCurr, float uVelCurr)
        {
            //solving something like ui = u_i-1 - u_i + u_i+1 - u_i + L_pc*Sign_(u_c - u_p) - L_cn*Sign(u_n - u_c)
            //Except we will keep the signs of neighbouring u parameters fixed from the initial conditions: ui = u_i-1 - 2 * u_i + u_i+1 + L_pc*Sign_(u_0,c - u_0,p) - L_cn*Sign(u_0,n - u_0,c)
            float springForce = (uPrev - 2 * uCurr + uNext + signedLengthPrevToCurr - signedLengthCurrToNext) * _dynamicSpringOmegaSquared;
            float damping = -uVelCurr * _dynamicSpringDampingRatio * _dynamicSpringOmega;
            float forceTotal = springForce + damping;
            return forceTotal;
        }

        /// <summary>
        /// If spring approximation iteration should continue
        /// </summary>
        /// <param name="numIterations">Current number of iterations</param>
        /// <param name="deltaUParameters">Current iteration's change in u parameters</param>
        /// <returns></returns>
        private static bool ContinueIteration(int numIterations, float[] deltaUParameters)
        {
            float maxDeltaU = 0f;
            for(int i = 0; i < deltaUParameters.Length; i++)
            {
                maxDeltaU = Math.Max(maxDeltaU, deltaUParameters[i]);
            }
            return numIterations < _maxNumIterations || maxDeltaU < _uParameterEpsilon;
        }

        /// <summary>
        /// Calculates the average of an array of floating-point values
        /// </summary>
        /// <param name="values">Floating-point values</param>
        private static float CalculateAverage(float[] values)
        {
            float average = 0.0f;
            int numvalues = values.Length;
            for (int i = 0; i < numvalues; i++)
            {
                average += values[i];
            }
            if(numvalues > 0)
            {
                average /= numvalues;
            }
            return average;
        }


        /// <summary> Maximum number of iterations in solutions</summary>
        const int _maxNumIterations = 1000;
        /// <summary> Epsilon for change in u parameter above which to continue iteration </summary>
        const float _uParameterEpsilon = 1e-6f;

        /// <summary> Dynamic spring approximation effective time step </summary>
        const float _dynamicSpringPseudoTimeStep = 0.05f;
        /// <summary> Dynamic spring approximation damping ratio </summary>
        const float _dynamicSpringDampingRatio = 1f;
        /// <summary> Dynamic spring approximation angular frequency </summary>
        const float _dynamicSpringOmega = 1f;
        /// <summary> Dynamic spring approximation angular frequency squared </summary>
        const float _dynamicSpringOmegaSquared = _dynamicSpringOmega * _dynamicSpringOmega;
    }
}
