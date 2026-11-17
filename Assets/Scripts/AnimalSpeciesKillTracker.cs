using System.Collections.Generic;
using UnityEngine;

public enum AnimalSpecies
{
    Monkey,
    Jaguar,
    Bat
}

/// <summary>Registra quais espécies animais o jogador matou nesta partida.</summary>
public static class AnimalSpeciesKillTracker
{
    private static readonly HashSet<AnimalSpecies> killedSpecies = new HashSet<AnimalSpecies>();

    public static bool HasKilledEverySpecies
    {
        get { return killedSpecies.Count >= 3; }
    }

    public static void RegisterKill(AnimalSpecies species)
    {
        killedSpecies.Add(species);
    }

    public static void ResetForNewRun()
    {
        killedSpecies.Clear();
    }
}
