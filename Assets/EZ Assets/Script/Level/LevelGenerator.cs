using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static List<LevelConfig> levels = new List<LevelConfig>();

    public static void GenerateLevels()
    {
        levels.Clear(); 

        for (int i = 0; i < 10; i++)
        {
            levels.Add(new LevelConfig
            {
                levelIndex = i + 1,
                botMoveSpeed = 2f + i * 0.3f,
                botAttackFrequency = Mathf.Max(0.6f - i * 0.05f, 0.2f),
                botDamage = 10 + i * 5
            });
        }
    }

    private void Awake()
    {
        if (levels == null || levels.Count == 0)
        {
            GenerateLevels();
        }
    }


}
