
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay")]
    public int currentLevel = 0;
    public static bool isGameOver = false;

    [Header("Player & Bots")]
    public GameObject player;
    public GameObject bot;  
    public Transform playerSpawn;
    public Transform botSpawn;
    public Transform[] extraBotSpawns;

    [Header("UI")]
    public GameObject menuUI;

    private GameMode selectedMode = GameMode.OneVsOne;
    private List<GameObject> extraBots = new List<GameObject>();

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
            Debug.LogWarning("Level list is empty. Generating default levels...");
            LevelGenerator.GenerateLevels();
        }

        ShowMenu();
    }

    private void ShowMenu()
    {
        menuUI.SetActive(true);
        player.SetActive(false);
        bot.SetActive(false);
    }

    public void Select1vs1()
    {
        Debug.Log("Selected mode: 1 vs 1");
        selectedMode = GameMode.OneVsOne;
        StartGame();
    }

    public void Select1vsMany()
    {
        Debug.Log("Selected mode: 1 vs Many");
        selectedMode = GameMode.OneVsMany;
        StartGame();
    }

    public void SelectManyVsMany()
    {
        Debug.Log("Selected mode: Many vs Many");
        selectedMode = GameMode.ManyVsMany;
        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("Starting game with mode: " + selectedMode);
        menuUI.SetActive(false);

        player.SetActive(true);
        player.transform.position = playerSpawn.position;

        bot.SetActive(true);
        bot.transform.position = botSpawn.position;

        ClearExtraBots();

        switch (selectedMode)
        {
            case GameMode.OneVsMany:
                Debug.Log("Spawning 2 extra bots");
                SpawnExtraBots(2);
                break;
            case GameMode.ManyVsMany:
                Debug.Log("Spawning 3 extra bots");
                SpawnExtraBots(3);
                break;
        }

        player.GetComponent<PlayersController>().RestartAutoCombo();

        StartLevel();
    }

    private void SpawnExtraBots(int count)
    {
        Debug.Log($"SpawnExtraBots called with count = {count}");
        if (extraBotSpawns == null || extraBotSpawns.Length == 0)
        {
            Debug.LogError("extraBotSpawns is empty! Please assign spawn points in Inspector.");
            return;
        }

        for (int i = 0; i < count && i < extraBotSpawns.Length; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

            Vector3 spawnPosition = extraBotSpawns[i].position + spawnOffset;

            Debug.Log($"Spawning extra bot at position {spawnPosition}");

            GameObject newBot = Instantiate(bot, spawnPosition, Quaternion.identity);
            extraBots.Add(newBot);

            var botAI = newBot.GetComponent<BotsController>();
            if (botAI != null)
            {
                var config = LevelGenerator.levels[currentLevel];
                int aiLevel = Mathf.Clamp(config.levelIndex, 0, 9);
                botAI.SetAILevel(aiLevel);
            }
            else
            {
                Debug.LogWarning("Spawned bot does not have BotsController component!");
            }

            var health = newBot.GetComponent<HealthCharacter>();
            if (health != null)
            {
                health.ResetHealth();
            }
            else
            {
                Debug.LogWarning("Spawned bot does not have HealthCharacter component!");
            }
        }

    }

    private void ClearExtraBots()
    {
        foreach (var bot in extraBots)
        {
            Destroy(bot);
        }
        extraBots.Clear();
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
            Debug.LogError($"Invalid level index: {currentLevel}, levels count: {LevelGenerator.levels.Count}");
            return;
        }

        var config = LevelGenerator.levels[currentLevel];
        Debug.Log($"⚔️ Starting Level {config.levelIndex}");

        int aiLevel = Mathf.Clamp(config.levelIndex, 0, 9);

        var botAI = bot.GetComponent<BotsController>();
        if (botAI != null)
            botAI.SetAILevel(aiLevel);

        foreach (var extraBot in extraBots)
        {
            var botExtraAI = extraBot.GetComponent<BotsController>();
            if (botExtraAI != null)
                botExtraAI.SetAILevel(aiLevel);
        }
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
            Debug.LogError("GameManager.Instance is null. Make sure GameManager is in the scene!");
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
            Debug.Log("🎉 All levels complete!");
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
}
