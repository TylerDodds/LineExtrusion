using BabyDinoHerd.Extrusion.Line.Interfaces;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection
{
    /// <summary>
    /// Base class for extruded chunk connection (implementation of <see cref="IExtrudedChunkContourConnector"/>).
    /// </summary>
    public abstract class ExtrudedChunkContourConnnectorBase : IExtrudedChunkContourConnector
    {
        /// <summary>
        /// Determines the intersection point calculated from the one at the start of a given chunk with the intersection point at the end of the previous chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        protected abstract Vector2WithUV DetermineChunkStartIntersectionPointWithRecalculatedUVs(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk);

        /// <summary>
        /// Convert a list of <see cref="ChunkBetweenIntersectionsCollection"/> into a list of <see cref="Vector2WithUV[]"/>.
        /// </summary>
        /// <param name="chunkCollectionList">A list of <see cref="ChunkBetweenIntersectionsCollection"/>.</param>
        /// <param name="intersectionUVCalculation">Method for calculating UV of intersection points</param>
        public List<Vector2WithUV[]> ConnnectChunksBetweenIntersectionsIntoPointsLists(List<ChunkBetweenIntersectionsCollection> chunkCollectionList)
        {
            List<Vector2WithUV[]> ret = new List<Vector2WithUV[]>();

            for (int i = 0; i < chunkCollectionList.Count; i++)
            {
                var currentChunkCollection = chunkCollectionList[i];
                var currentChunkCollectionNumPoints = currentChunkCollection.TotalNumberOfPoints;
                Vector2WithUV[] currentChunkArray = new Vector2WithUV[currentChunkCollectionNumPoints];
                int index = 0;
                for (int chunkIndex = 0; chunkIndex < currentChunkCollection.Chunks.Count; chunkIndex++)
                {
                    var currentChunkToAdd = currentChunkCollection.Chunks[chunkIndex];

                    //Add start intersection 
                    var intersectionPointAtStart = new Vector2WithUV(currentChunkToAdd.StartIntersection);
                    if (chunkIndex > 0)
                    {
                        ChunkBetweenIntersections previousChunk = currentChunkCollection.Chunks[chunkIndex - 1];
                        intersectionPointAtStart = DetermineChunkStartIntersectionPointWithRecalculatedUVs(currentChunkToAdd, intersectionPointAtStart, previousChunk);
                    }

                    currentChunkArray[index++] = intersectionPointAtStart;

                    //Add middle points
                    for (int k = 0; k < currentChunkToAdd.ExtrudedPoints.Count; k++)
                    {
                        currentChunkArray[index++] = new Vector2WithUV(currentChunkToAdd.ExtrudedPoints[k]);
                    }

                    //If at the very end, add final point
                    if (chunkIndex == currentChunkCollection.Chunks.Count - 1)
                    {
                        currentChunkArray[index++] = new Vector2WithUV(currentChunkToAdd.EndIntersection);
                    }
                }
                ret.Add(currentChunkArray);
            }

            return ret;
        }
    }
}