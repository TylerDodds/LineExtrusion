using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A chunk of an extruded contour that is monotonic in its points' u-parameters. 
    /// Since each contour is closed, we expect the u-parameter to increase monotonically from its minimum up to its maximum, and then decrease monotonically back down until the chunk re-connects back on itself.
    /// </summary>
    public class ExtrudedContourMonotonicChunk
    {
        /// <summary> Gets the closest point in the chunk with the given u-parameter. </summary>
        public bool GetClosestPoint(float uParameter, out Vector2WithUV closest)
        {
            bool gotPoint;
            int numPoints = Points.Count;
            if (numPoints > 1)
            {
                if (uParameter < MinimumU)
                {
                    gotPoint = false;
                    closest = Points[0];
                }
                else if (uParameter > MaximumU)
                {
                    gotPoint = false;
                    closest = Points[numPoints - 1];
                }
                else
                {
                    int indexLower = 0;
                    int indexHigher = Points.Count - 1;
                    while (indexHigher - indexLower >= 2)
                    {
                        int indexMid = indexLower + (indexHigher - indexLower) / 2;
                        float uMid = Points[indexMid].UV.x;
                        if (uMid > uParameter)
                        {
                            indexHigher = indexMid;
                        }
                        else
                        {
                            indexLower = indexMid;
                        }
                    }

                    gotPoint = true;
                    float fraction = (uParameter - Points[indexLower].UV.x) / (Points[indexHigher].UV.x - Points[indexLower].UV.x);
                    closest = Points[indexLower].AverageWith(Points[indexHigher], fraction);
                }
            }
            else if (numPoints == 1)
            {
                gotPoint = false;
                closest = Points[0];
            }
            else
            {
                gotPoint = false;
                closest = default(Vector2WithUV);
            }
            return gotPoint;
        }

        /// <summary> The set of all points in this monotonic chunk (endpoints inclusive). Ordered in increasing u-parameter. </summary>
        public List<Vector2WithUV> Points { get; private set; }
        /// <summary> The minimum u-parameter of this chunk. </summary>
        public float MinimumU { get; private set; }
        /// <summary> The maximum u-parameter of this chunk. </summary>
        public float MaximumU { get; private set; }
        /// <summary> If the points are reversed (that is, this chunk was originally monotonic decreasing). </summary>
        public bool IsReversed { get; private set; }

        /// <summary> Creates an instance of <see cref="ExtrudedContourMonotonicChunk"/> with the given parameters. </summary>
        public ExtrudedContourMonotonicChunk(List<Vector2WithUV> points, float minimumU, float maximumU, bool isReversed)
        {
            Points = points;
            MinimumU = minimumU;
            MaximumU = maximumU;
            IsReversed = isReversed;
        }
    }
}