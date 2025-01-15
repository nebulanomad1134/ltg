using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreItemData", menuName = "Scriptable Objects/StoreItemData")]
public class StoreItemData : ScriptableObject
{
    [Header("Store Info")]
    public string itemID;        // Unique key (used in PlayerPrefs or other save system)
    public string displayName;   // Visible name in the store
    [TextArea]
    public string description;   // Description shown in the store UI
    public Sprite icon;          // Icon image
    public int cost;             // Price in coins

    [Header("Stat Boosts")]
    public float healthBoost;       // How much to increase maxHealth
    public float damageBoost;       // Additional damage multiplier
    public float movementSpeedBoost; // Additional speed multiplier
    public float weaponSpeedBoost;   // Additional weapon speed multiplier (for weapon ID 0)
    public float weaponRateBoost;    // Additional weapon rate multiplier (for weapon ID 1)
}