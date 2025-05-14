using UnityEngine;

public interface IAlertable
{
    /// <summary>
    /// Appelé lorsqu’une onde atteint un objet alertable (ex : ennemi).
    /// </summary>
    /// <param name="sourcePosition">Position de la source de l'alerte (ex : joueur).</param>
    void Alert(Vector2 sourcePosition);
}