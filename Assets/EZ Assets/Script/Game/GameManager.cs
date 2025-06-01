using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLevel = 0;
    public static bool isGameOver = false;

    public GameObject player;
    public GameObject bot;
    public Transform playerSpawn;
    public Transform botSpawn;

   private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        if (LevelGenerator.levels == null || LevelGenerator.levels.Count == 0)
        {
            Debug.LogWarning(" Level list is empty. Generating default levels...");
            LevelGenerator.GenerateLevels();
        }

        StartLevel();
    }

    public static void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over! Preparing for next round...");

        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.NextRoundAfterDelay(5f));
        }
        else
        {
            Debug.LogError(" GameManager.Instance is null. Make sure GameManager is in the scene!");
        }
    }


    private IEnumerator NextRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetCharacters();

        currentLevel++;
        if (currentLevel < LevelGenerator.levels.Count)
        {
            StartLevel();
            isGameOver = false;
        }
        else
        {
            Debug.Log(" All levels complete!");
        }
    }

    public void ResetCharacters()
    {
        player.transform.position = playerSpawn.position;
        bot.transform.position = botSpawn.position;

        player.GetComponent<HealthCharacter>().ResetHealth();
        bot.GetComponent<HealthCharacter>().ResetHealth();

        player.GetComponent<PlayersController>().RestartAutoCombo();

        StartCoroutine(ResetAnimatorTriggers(player));
        StartCoroutine(ResetAnimatorTriggers(bot));
    }

    private IEnumerator ResetAnimatorTriggers(GameObject character)
    {
        Animator animator = character.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            yield return null;

            animator.ResetTrigger("Punch1");
            animator.ResetTrigger("Hit1");

            animator.SetTrigger("Punch1");
            animator.SetTrigger("Hit1");

            Debug.Log(character.name + " reset and triggered animations");
        }
    }

    private void StartLevel()
    {
        if (LevelGenerator.levels == null || LevelGenerator.levels.Count == 0)
        {
            Debug.LogError("Level list is empty! Cannot start level.");
            return;
        }

        if (currentLevel >= LevelGenerator.levels.Count)
        {
            Debug.LogError($" Invalid level index: {currentLevel}, levels count: {LevelGenerator.levels.Count}");
            return;
        }

        var config = LevelGenerator.levels[currentLevel];
        Debug.Log($"⚔️ Starting Level {config.levelIndex}");

        var botAI = bot.GetComponent<BotsController>();

        int aiLevel = Mathf.Clamp(config.levelIndex, 0, 9);
        botAI.SetAILevel(aiLevel);

    }



}
