using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper is a static class that provides utility methods and functions for various tasks in the project. 
/// It serves as a centralized location for commonly used helper methods, 
/// making it easier to maintain and reuse code across different parts of the application.
/// </summary>
public static class Helper {
    public static readonly List<EnumCreatureName> CliffCreatures = new List<EnumCreatureName> { 
        EnumCreatureName.BirdMan 
    };
    public static readonly List<EnumCreatureName> SeaCreatures = new List<EnumCreatureName> {
        EnumCreatureName.BabyNenni, EnumCreatureName.Mermaid1, EnumCreatureName.Mermaid2, EnumCreatureName.SeaBeast, EnumCreatureName.Kraki
    };

    public static EnumBiomes GetCreatureBiome(EnumCreatureName creatureName) {

        if (CliffCreatures.Contains(creatureName)) return EnumBiomes.Cliffs;
        else if (SeaCreatures.Contains(creatureName)) return EnumBiomes.Sea;
        else return EnumBiomes.None;
    }

    /// <summary>
    /// Creates a looping sequence of sprite changes for a given Image component, 
    /// cycling through the provided array of Sprites at a specified frame time.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="cycle"></param>
    /// <param name="frameTime"></param>
    /// <returns></returns>
    public static Sequence CreateSequenceCycle(Image image, Sprite[] cycle, float frameTime) {
        Sequence sequence = DOTween.Sequence().Pause();

        for (int i = 0; i < cycle.Length; i++) {
            int index = i;

            // Add a callback to change the sprite of the image
            sequence.AppendCallback(() => {
                image.sprite = cycle[index];
            });

            // Add a delay after each sprite change
            sequence.AppendInterval(frameTime);
        }

        sequence.SetLoops(-1);
        return sequence;
    }

    /// <summary>
    /// Returns the next biome in the EnumBiomes enumeration, cycling back to the first biome if the current biome is the last one.
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="setValue"></param>
    /// <returns>The next biome in the enumeration</returns>
    public static EnumBiomes NextBiome(EnumBiomes biome, bool setValue = false) {
        List<EnumBiomes> biomes = Enum.GetValues(typeof(EnumBiomes)).Cast<EnumBiomes>().Where(b => b != EnumBiomes.None).ToList();
        EnumBiomes nextBiome;

        foreach (var item in biomes) {
            Debug.Log($"{biomes.IndexOf(item)}, {item}");
        }

        if (biome == EnumBiomes.None) nextBiome = biomes.First();
        else if (biome == biomes.Last()) nextBiome = biomes.First();
        else nextBiome = biomes[biomes.IndexOf(biome) + 1];

        if (setValue) GameManager.Instance.CurrentBiomeSelected = nextBiome;
        return nextBiome;
    }

    /// <summary>
    /// Returns the previous biome in the EnumBiomes enumeration, cycling back to the last biome if the current biome is the first one.
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="setValue"></param>
    /// <returns></returns>
    public static EnumBiomes PreviousBiome(EnumBiomes biome, bool setValue = false) {
        List<EnumBiomes> biomes = Enum.GetValues(typeof(EnumBiomes)).Cast<EnumBiomes>().Where(b => b != EnumBiomes.None).ToList();
        EnumBiomes previousBiome;

        if (biome == EnumBiomes.None) previousBiome = biomes.Last();
        else if (biome == biomes.First()) previousBiome = biomes.Last();
        else previousBiome = biomes[biomes.IndexOf(biome) - 1];

        if (setValue) GameManager.Instance.CurrentBiomeSelected = previousBiome;
        return previousBiome;
    }

}
