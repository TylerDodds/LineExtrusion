using System.Collections.Generic;
using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A list of <see cref="LinePointUV"/> defining a line segmentwise.
    /// </summary>
    public class SegmentwiseLinePointListUV : SegmentwisePointList<LinePointUV>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SegmentwiseLinePointListUV"/> based on its points.
        /// </summary>
        /// <param name="points">The points of the list.</param>
        public SegmentwiseLinePointListUV(IList<LinePointUV> points) : base(points)
        {

        }

        /// <summary>
        /// Return the segment distance from the previous point to this index.
        /// </summary>
        /// <param name="index">Index of the given point.</param>
        protected override float GetDistanceFromPreviousPoint(int index)
        {
            return (Points[index].Point - Points[index - 1].Point).magnitude;
        }
    }

    /// <summary>
    /// A list of <see cref="ExtrudedPointUV"/> defining a line segmentwise.
    /// </summary>
    public class SegmentwiseExtrudedPointListUV : SegmentwisePointList<ExtrudedPointUV>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SegmentwiseExtrudedPointListUV"/> based on its points.
        /// </summary>
        /// <param name="points">The points of the list.</param>
        public SegmentwiseExtrudedPointListUV(IList<ExtrudedPointUV> points) : base(points)
        {
        }

        /// <summary>
        /// Return the segment distance from the previous point to this index.
        /// </summary>
        /// <param name="index">Index of the given point.</param>
        protected override float GetDistanceFromPreviousPoint(int index)
        {
            return (Points[index].Point - Points[index - 1].Point).magnitude;
        }
    }

    /// <summary>
    /// A list of <see cref="Vector2WithUV"/> defining a line segmentwise.
    /// </summary>
    public class SegmentwiseVector2WithUVList : SegmentwisePointList<Vector2WithUV>
    {
        /// <summary>
        /// Creates a new instance of <see cref="SegmentwiseVector2WithUVList"/> based on its points.
        /// </summary>
        /// <param name="points">The points of the list.</param>
        public SegmentwiseVector2WithUVList(IList<Vector2WithUV> points) : base(points)
        {
        }

        /// <summary>
        /// Return the segment distance from the previous point to this index.
        /// </summary>
        /// <param name="index">Index of the given point.</param>
        protected override float GetDistanceFromPreviousPoint(int index)
        {
            return (Points[index].Vector - Points[index - 1].Vector).magnitude;
        }
    }

    /// <summary>
    /// A list of points of type <typeparamref name="T"/> definining a segmentwise line.
    /// </summary>
    /// <typeparam name="T">The point type</typeparam>
    public abstract class SegmentwisePointList<T> where T : struct
    {
        /// <summary>
        /// The points in the length.
        /// </summary>
        public IList<T> Points { get; private set; }

        /// <summary>
        /// The rough length of the line, defined segmentwise.
        /// </summary>
        public float LengthRough { get; private set; }

        /// <summary>
        /// The maximum length of any segment in the line.
        /// </summary>
        public float MaxSegmentDistance { get; private set; }

        /// <summary>
        /// Return the segment distance from the previous point to this index.
        /// </summary>
        /// <param name="index">Index of the given point.</param>
        protected abstract float GetDistanceFromPreviousPoint(int index);

        /// <summary>
        /// Creates a new instance of <see cref="SegmentwisePointList{T}"/> based on its points.
        /// </summary>
        /// <param name="points">The points of the list.</param>
        public SegmentwisePointList(IList<T> points)
        {
            Points = points;
            SetSegmentedLineLengthAndMaximumSegmentDelta();
        }

        /// <summary>
        /// Sets <see cref="LengthRough"/> and <see cref="MaxSegmentDistance"/> based on calculated values.
        /// </summary>
        private void SetSegmentedLineLengthAndMaximumSegmentDelta()
        {
            var lengthRough = 0f;
            var maxSegmentDistance = 0f;
            for (int i = 1; i < Points.Count; i++)
            {
                var segmentDistance = GetDistanceFromPreviousPoint(i);
                lengthRough += segmentDistance;
                maxSegmentDistance = Mathf.Max(maxSegmentDistance, segmentDistance);
            }

            LengthRough = lengthRough;
            MaxSegmentDistance = maxSegmentDistance;
        }

    }
}