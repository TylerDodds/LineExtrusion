using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Interfaces
{
    /// <summary>
    /// Convert a set of <see cref="ChunkBetweenIntersectionsCollection"/> into a set of <see cref="Vector2WithUV[]"/>.
    /// </summary>
    public interface IExtrudedChunkContourConnector
    {
        /// <summary>
        /// Convert a list of <see cref="ChunkBetweenIntersectionsCollection"/> into a list of <see cref="Vector2WithUV[]"/>.
        /// </summary>
        /// <param name="chunkCollectionList">A list of <see cref="ChunkBetweenIntersectionsCollection"/>.</param>
        List<Vector2WithUV[]> ConnnectChunksBetweenIntersectionsIntoPointsLists(List<ChunkBetweenIntersectionsCollection> chunkCollectionList);

    }

}