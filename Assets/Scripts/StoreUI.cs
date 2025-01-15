using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StoreUI : MonoBehaviour
{
    [Header("Store Items List")]
    public List<StoreItemData> storeItems;
    public GameObject storeItemPrefab;
    public Transform itemContainer;
    public Text totalCoinText;

    private void OnEnable()
    {
        if (!ValidateReferences()) return;

        // Add this debug
        Debug.Log($"Store Items Count: {storeItems.Count}");

        // Ensure Grid Layout is set up correctly
        GridLayoutGroup grid = itemContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            Debug.Log($"Grid Layout - Cell Size: {grid.cellSize}, Spacing: {grid.spacing}, " +
                      $"Constraint: {grid.constraint}, Column Count: {grid.constraintCount}");
        }
        else
        {
            Debug.LogError("Missing Grid Layout Group on itemContainer!");
            return;
        }

        ClearContainer();
        CreateStoreItems();
        UpdateCoinUI();
    }

    private bool ValidateReferences()
    {
        if (storeItems == null || storeItemPrefab == null ||
            itemContainer == null || totalCoinText == null)
        {
            Debug.LogError("Missing references in StoreUI! Check Inspector.");
            return false;
        }
        return true;
    }

    private void ClearContainer()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateStoreItems()
    {
        foreach (StoreItemData data in storeItems)
        {
            if (data == null) continue;

            GameObject itemUI = Instantiate(storeItemPrefab, itemContainer);
            SetupStoreItem(itemUI, data);
        }
    }
    private void SetupStoreItem(GameObject itemUI, StoreItemData data)
    {
        try
        {
            Debug.Log($"Starting setup for item: {data.displayName}");

            // Get references
            var iconObj = itemUI.transform.Find("Icon");
            var nameObj = itemUI.transform.Find("NameText");
            var descObj = itemUI.transform.Find("DescriptionText");
            var costObj = itemUI.transform.Find("CostText");
            var buyObj = itemUI.transform.Find("BuyButton");

            // Get components
            Image panelImage = iconObj?.GetComponent<Image>();        // Panel background
            Image buyButtonImage = buyObj?.GetComponent<Image>();     // Buy button image
            Text nameText = nameObj?.GetComponent<Text>();
            Text descText = descObj?.GetComponent<Text>();
            Text costText = costObj?.GetComponent<Text>();
            Button buyButton = buyObj?.GetComponent<Button>();

            // Validate components
            if (panelImage == null || buyButtonImage == null || nameText == null ||
                descText == null || costText == null || buyButton == null)
            {
                Debug.LogError($"Missing components in prefab for item: {data.displayName}");
                return;
            }

            // Set data
            nameText.text = data.displayName;
            descText.text = data.description;
            costText.text = $"$: {data.cost} coin";

            // Set icon to buy button
            if (data.icon != null)
            {
                buyButtonImage.sprite = data.icon;
                buyButtonImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning($"No icon for item: {data.displayName}");
            }

            // Check if already purchased
            bool isPurchased = PlayerPrefs.GetInt(data.itemID, 0) == 1;
            if (isPurchased)
            {
                Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                panelImage.color = grayColor;
                buyButtonImage.color = grayColor;
                costText.text = "OWNED";
                buyButton.interactable = false;
            }
            else
            {
                panelImage.color = Color.white;
                buyButtonImage.color = Color.white;
                buyButton.interactable = true;

                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => OnBuyClicked(data, panelImage, buyButtonImage, buyButton, costText));
            }

            Debug.Log($"Successfully set up item: {data.displayName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up item {data.displayName}: {e.Message}\n{e.StackTrace}");
        }
    }
    private void OnBuyClicked(StoreItemData data, Image panelImage, Image buyButtonImage,
                             Button buyButton, Text costText)
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);

        if (GameManager.instance == null) return;

        if (PlayerPrefs.GetInt(data.itemID, 0) == 1)
        {
            Debug.Log($"{data.displayName} is already purchased!");
            return;
        }

        if (GameManager.instance.totalCoin >= data.cost)
        {
            // Purchase success
            GameManager.instance.totalCoin -= data.cost;
            PlayerPrefs.SetInt(data.itemID, 1);
            PlayerPrefs.Save();

            // Update UI to show purchased state
            Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            panelImage.color = grayColor;
            buyButtonImage.color = grayColor;
            costText.text = "OWNED";
            buyButton.interactable = false;

            UpdateCoinUI();

            // Apply stats
            ApplyStatBoost(data);

            GameManager.instance.UpdateAllGears();

            Debug.Log($"Successfully purchased {data.displayName}!");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    private void ApplyStatBoost(StoreItemData data)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.maxHealth += data.healthBoost;
            GameManager.instance.health = GameManager.instance.maxHealth;

            GameManager.instance.damageBoostFactor += data.damageBoost;
            GameManager.instance.movementSpeedBoostFactor += data.movementSpeedBoost;
            GameManager.instance.weaponSpeedBoostFactor += data.weaponSpeedBoost;
            GameManager.instance.weaponRateBoostFactor += data.weaponRateBoost;

            Debug.Log($"Applied boosts: +{data.healthBoost} HP, +{data.damageBoost} DMG, " +
                     $"+{data.movementSpeedBoost} SPD, +{data.weaponSpeedBoost} WSPD, " +
                     $"+{data.weaponRateBoost} WRATE");
        }
    }

    private void UpdateCoinUI()
    {
        if (GameManager.instance != null && totalCoinText != null)
        {
            totalCoinText.text = $"Total Coin: {GameManager.instance.totalCoin}";
        }
    }

}