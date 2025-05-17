// IActivatableLight.cs
using UnityEngine;

/// <summary>
/// Interface définissant un comportement d’activation/désactivation pour une lumière.
/// Utile pour standardiser l’interaction dans des puzzles, triggers ou systèmes de contrôle distants.
/// </summary>
public interface IActivatableLight
{
    /// <summary>
    /// Active la lumière (peut inclure fade, FX, etc.).
    /// </summary>
    void Activate();

    /// <summary>
    /// Désactive la lumière.
    /// </summary>
    void Deactivate();

    /// <summary>
    /// Retourne l’état actuel de la lumière (active ou non).
    /// </summary>
    /// <returns>true si la lumière est active, false sinon.</returns>
    bool IsActive();
}