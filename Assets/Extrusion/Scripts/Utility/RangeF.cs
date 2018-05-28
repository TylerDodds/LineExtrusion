using UnityEngine;

namespace BabyDinoHerd.Utility
{
    /// <summary>
    /// A range between two floating-point values.
    /// </summary>
    public class RangeF
    {
        /// <summary>
        /// The range endpoint with the smallest absolute value.
        /// </summary>
        public float MinByAbs
        {
            get
            {
                return Mathf.Abs(Min) <= Mathf.Abs(Max) ? Min : Max;
            }
        }

        /// <summary>
        /// The range endpoint maximum value.
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// The range endpoint minimum value.
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// The range length.
        /// </summary>
        public float Length { get { return Max - Min; } }

        /// <summary>
        /// Gets the fraction along the range of a value, *not* clamped to [0, 1].
        /// </summary>
        /// <param name="value">The value in question.</param>
        public float GetFractionUnclamped(float value)
        {
            return (value - Min) / Length;
        }

        /// <summary>
        /// Returns a value from a fraction (not restricted to [0, 1]).
        /// </summary>
        /// <param name="fraction">Fraction along the range.</param>
        public float FromFractionUnclamped(float fraction)
        {
            return fraction * Length + Min;
        }

        /// <summary>
        /// Create a range from endpoints.
        /// </summary>
        /// <param name="val1">One endpoint.</param>
        /// <param name="val2">The other endpoint.</param>
        public static RangeF FromEndpoints(float val1, float val2)
        {
            var min = Mathf.Min(val1, val2);
            var max = Mathf.Max(val1, val2);
            return new RangeF(min, max);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RangeF"/> based on its minimum and maximum values.
        /// </summary>
        /// <param name="min">The range endpoint minium value.</param>
        /// <param name="max">The range endpoint maximum value.</param>
        public RangeF(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RangeF"/> based on the minimum and maximum values of another <see cref="RangeF"/>.
        /// </summary>
        /// <param name="other">The <see cref="RangeF"/> to copy.</param>
        public RangeF(RangeF other)
        {
            Min = other.Min;
            Max = other.Max;
        }

        /// <summary>
        /// Include a value in this range.
        /// </summary>
        /// <param name="value">The value to include.</param>
        public void Include(float value)
        {
            Min = Mathf.Min(Min, value);
            Max = Mathf.Max(Max, value);
        }

        /// <summary>
        /// Include another range in this range.
        /// </summary>
        /// <param name="range">The other range to include.</param>
        public void Include(RangeF range)
        {
            Min = Mathf.Min(Min, range.Min);
            Max = Mathf.Max(Max, range.Max);
        }

    }
}