using SeagullEngine.SemanticSystem.Core;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// OBSOLETE: Use ISemanticCoherenceEngine directly.
    /// Legacy adapter interface for semantic coherence calculation.
    /// Kept for backwards compatibility during migration.
    /// </summary>
    [System.Obsolete("Use ISemanticCoherenceEngine instead", false)]
    public interface ISemanticCoherenceAdapter
    {
        float CalculateHistoryCoherence(
            string candidateWord,
            ISemanticProfile profile,
            ISemanticAtlas atlas
        );
    }
}
