using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.Patron
{
    [RegisterComponent]
    [Access(typeof(PatronSystem))]
    public sealed class PatronEarsComponent : Component
    {
        [DataField("sprite")]
        public string RsiPath = "Clothing/Head/Hats/catears.rsi";
    }
}
