namespace BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection.Experimental
{
    /// <summary>
    /// Extruded chunk conneector that averages intersection points based on arcdistance to neighbouring contour points.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class ExtrudedChunkContourConnector_AveragedArcdistance : ExtrudedChunkContourConnnectorBase
    {
        /// <summary>
        /// Determines the intersection point calculated from the one at the start of a given chunk with the intersection point at the end of the previous chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        protected override Vector2WithUV DetermineChunkStartIntersectionPointWithRecalculatedUVs(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            return AverageIntersectionPointsBasedOnArcdistance(currentChunkToAdd, intersectionPointAtStart, previousChunk);
        }

        /// <summary>
        /// Averages the intersection point at the start of this chunk with the intersection point at the end of the previous chunk, weighted based on relative arcdistance of the intersection point to the neighbouring point on each chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        private static Vector2WithUV AverageIntersectionPointsBasedOnArcdistance(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            var intersectionPointAtPreviousEnd = new Vector2WithUV(previousChunk.EndIntersection);

            var lengthStartIntersectionToFirstPoint = (currentChunkToAdd.PointAfterStart.Vector - intersectionPointAtStart.Vector).magnitude;
            var lengthLastPointToEndIntersection = (intersectionPointAtPreviousEnd.Vector - previousChunk.PointBeforeEnd.Vector).magnitude;
            var averagingFraction = lengthStartIntersectionToFirstPoint / (lengthStartIntersectionToFirstPoint + lengthLastPointToEndIntersection);

            var averagedIntersectionPointAtStart = intersectionPointAtStart.AverageWith(intersectionPointAtPreviousEnd, averagingFraction);

            return averagedIntersectionPointAtStart;
        }
    }
}