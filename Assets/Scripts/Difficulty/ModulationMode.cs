/// <summary>
/// The five modes the DifficultyModulator can be in. Names come from section 1.3 of the design document.
/// Mercy ≤ -7, Ease -3..-6, Default -2..+2, Tighten +3..+6, Challenge ≥ +7.
/// </summary>
public enum ModulationMode
{
    Mercy,
    Ease,
    Default,
    Tighten,
    Challenge
}
