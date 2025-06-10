// IRevealable.cs

/// <summary>
/// Rend l'objet visible temporairement en fonction de l'intensité de l'onde.
/// </summary>
/// <param name="waveIntensity">Valeur liée à la force de l'onde (ex: pour moduler la durée de révélation).</param>

public interface IRevealable
{
    void Reveal(float waveIntensity);       // Force normalisée entre 0 et 1
}