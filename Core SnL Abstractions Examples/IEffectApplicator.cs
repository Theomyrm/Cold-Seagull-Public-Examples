using System.Collections.Generic;
using SeagullEngine.DSL.SeagullNoLonger.Models;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// Generic effect applicator interface for DSL.
    /// Applies parsed effects to targets in the execution context.
    /// </summary>
    public interface IEffectApplicator
    {
        void ApplyEffects(List<EffectToken> effects, IDSLContext context);
    }
}
