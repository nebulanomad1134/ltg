using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("#Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("#Map Control")]
    public int currentMap = 1;
    public int maxMap = 5;

    [Header("#Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = {3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
    // Store the base multipliers / or extra factor from store
    public float damageBoostFactor;         // Default = 0 => no extra
    public float movementSpeedBoostFactor;  // Default = 0 => no extra
    public float weaponSpeedBoostFactor;    // Default = 0 => no extra (for weapon ID 0)
    public float weaponRateBoostFactor;     // Default = 0 => no extra (for weapon ID 1)

    [Header("#Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;
    public GameObject pausePanel;

    [Header("#Coin System")]
    public int coin;
    public int totalCoin;
    public Text coinText;

    [Header("#Store System")]
    public GameObject storePanel;
    // Optionally reference a list of ItemStoreData so we can re-apply purchased items
    public List<StoreItemData> storeItems;
    // (Optional) UI objects you want to hide when store is shown
    public List<GameObject> gameplayUIObjects;
    private bool isStoreOpen = false;

    void Awake()
    {
        instance = this;

        totalCoin = PlayerPrefs.GetInt("TotalCoin", 0);

        // Make sure store is hidden at start (if assigned)
        if (storePanel != null)
            storePanel.SetActive(false);

        // Restore previously purchased store items
        RestorePurchasedItems();

        // Initialize player's stats by applying stored boosts, if any
        health = maxHealth;
    }
    public void GameStart(int id)
    {
        playerId = id;
        health = maxHealth;

        coin = 0; // Reset coin each run

        StartCoroutine(ApplyBoostsDelayed());

        player.gameObject.SetActive(true);
        uiLevelUp.Select(playerId%2);
        Resume();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }
    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }
    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }
    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();

        uiResult.ShowNextMapButton(currentMap < maxMap);
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }
    public void GameRetry()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        SceneManager.LoadScene(0);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isLive && pausePanel.activeSelf)
            {
                AudioManager.instance.EffectBgm(false);
                Resume();
            }
            else if (isLive)
            {
                AudioManager.instance.EffectBgm(true);
                ShowPausePanel(); 
            }
        }

        if (!isLive) return;
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }
    public void GetExp()
    {
        if(!isLive) return;

        exp++;

        if (exp >= nextExp[Mathf.Min(level, nextExp.Length-1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
    }
    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }
    public void NextMap()
    {
        if (currentMap < maxMap)
        {
            currentMap++;
            SceneManager.LoadScene("Map" + currentMap);
        }
        else
        {
            Debug.Log("Finish all the levels!");
            SceneManager.LoadScene(0);
        }
    }
    public void ShowPausePanel()
    {
        Stop();
        pausePanel.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }
    public void AddCoin(int amount)
    {
        if (!isLive) return;

        coin += amount;       // Increase in-run coin count
        totalCoin += amount;  // Increase total coin count for persistent storage
        PlayerPrefs.SetInt("TotalCoin", totalCoin);
        PlayerPrefs.Save();
    }
    public void ToggleStore()
    {
        // Flip the boolean state
        isStoreOpen = !isStoreOpen;
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);

        // Show / hide the store panel
        if (storePanel != null)
            storePanel.SetActive(isStoreOpen);

        // Optionally hide or show other UI objects during store display
        foreach (GameObject uiObj in gameplayUIObjects)
        {
            uiObj.SetActive(!isStoreOpen);
        }
    }
    void RestorePurchasedItems()
    {
        foreach (StoreItemData data in storeItems)
        {
            bool isPurchased = PlayerPrefs.GetInt(data.itemID, 0) == 1;
            if (isPurchased)
            {
                maxHealth += data.healthBoost;
                damageBoostFactor += data.damageBoost;
                movementSpeedBoostFactor += data.movementSpeedBoost;
                weaponSpeedBoostFactor += data.weaponSpeedBoost;
                weaponRateBoostFactor += data.weaponRateBoost;
            }
        }
        health = maxHealth;
    }
    public void UpdateAllGears()
    {
        Gear[] gears = player.GetComponentsInChildren<Gear>();
        foreach (Gear gear in gears)
        {
            gear.ApplyGear();
        }
    }
    private void ApplyStoreBoosts()
    {
        if (player == null) return;

        Debug.Log("Applying store boosts..."); // Debug log

        // Apply movement speed boost
        float currentPlayerSpeed = player.speed;
        float speedBoost = currentPlayerSpeed * movementSpeedBoostFactor;
        player.speed += speedBoost;
        Debug.Log($"Player Speed: current={currentPlayerSpeed}, boost={speedBoost}, final={player.speed}");

        // Apply weapon boosts
        Weapon[] weapons = player.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            float currentDamage = weapon.damage;
            float damageBoost = currentDamage * damageBoostFactor;
            weapon.damage += damageBoost;

            if (weapon.id == 0)
            {
                // Weapon 0 - Apply speed boost
                float currentWeaponSpeed = weapon.speed;
                float speedBoostAmount = currentWeaponSpeed * weaponSpeedBoostFactor;
                weapon.speed += speedBoostAmount;

                Debug.Log($"Weapon {weapon.id}: " +
                         $"Speed(current={currentWeaponSpeed}, boost={speedBoostAmount}, final={weapon.speed}), " +
                         $"Damage(current={currentDamage}, boost={damageBoost}, final={weapon.damage})");
            }
            else
            {
                // Other weapons - Apply rate boost
                float currentRate = weapon.speed;
                float rateBoostAmount = currentRate * weaponRateBoostFactor;
                weapon.speed -= rateBoostAmount;

                Debug.Log($"Weapon {weapon.id}: " +
                         $"Rate(current={currentRate}, decrease={rateBoostAmount}, final={weapon.speed}), " +
                         $"Damage(current={currentDamage}, boost={damageBoost}, final={weapon.damage})");
            }
        }
    }
    private IEnumerator ApplyBoostsDelayed()
    {
        yield return null; // wait 1 frame
        ApplyStoreBoosts();
    }
}