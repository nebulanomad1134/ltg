using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        //Basic Set
        name = "Gear" + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        //Property Set
        type = data.itemType;
        rate = data.damages[0];
        ApplyGear();
    }
    public void LevelUp(float rate)
    {
        this.rate = rate;
        ApplyGear();
    }
    public void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
        }
    }
    void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();

        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id)
            {
                case 0:
                    float currentSpeed = weapon.speed;
                    float speedIncrease = currentSpeed * rate;
                    weapon.speed += speedIncrease;

                    Debug.Log($"Weapon {weapon.id} Speed: current={currentSpeed}, " +
                             $"increase={speedIncrease}, final={weapon.speed}");
                    break;

                default:
                    float currentRate = weapon.speed;
                    float rateDecrease = currentRate * rate;
                    weapon.speed -= rateDecrease;

                    Debug.Log($"Weapon {weapon.id} Rate: current={currentRate}, " +
                             $"decrease={rateDecrease}, final={weapon.speed}");
                    break;
            }
        }
    }

    void SpeedUp()
    {
        float currentSpeed = GameManager.instance.player.speed;
        float speedIncrease = currentSpeed * rate;
        GameManager.instance.player.speed += speedIncrease;

        Debug.Log($"Player Speed: current={currentSpeed}, " +
                 $"increase={speedIncrease}, final={GameManager.instance.player.speed}");
    }
}