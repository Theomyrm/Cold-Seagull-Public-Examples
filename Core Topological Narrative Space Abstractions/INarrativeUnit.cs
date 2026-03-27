using System;

namespace SeagullEngine.Math.NarrativeSpaces
{
    /// <summary>
    /// Minimal contract for narrative units in the SeagullEngine.
    /// The engine only requires spatial positioning - all semantic meaning
    /// and content complexity is provided by the author's implementation.
    /// 
    /// Design Philosophy:
    /// The topology and its mathematical rules provide the spatial framework
    /// for authors to assign meaning through positioning and relationships.
    /// </summary>
    public interface INarrativeUnit
    {
        /// <summary>
        /// Spatial identifier within the narrative carousel.
        /// Must be within the valid range [0, capacity-1] of the narrative space.
        /// This is the only property required by the engine for spatial operations.
        /// </summary>
        int SpatialId { get; }
    }

    /// <summary>
    /// Optional extended interface for narrative units that provide
    /// additional metadata to enhance engine operations.
    /// All properties have sensible defaults if not implemented.
    /// </summary>
    public interface INarrativeUnitExtended : INarrativeUnit
    {
        /// <summary>
        /// Display name for debugging, logging, and development tools.
        /// Used by the engine for diagnostic output and editor integration.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Semantic weight for influence calculations and centroid operations.
        /// Higher values indicate greater narrative significance.
        /// Default value: 1.0f if not implemented.
        /// </summary>
        /// <returns>Weight value (typically in range [0.1f, 3.0f])</returns>
        float SemanticWeight { get; }
    }
}
