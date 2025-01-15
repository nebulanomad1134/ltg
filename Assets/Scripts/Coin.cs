using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Coin : MonoBehaviour
{
    public int value = 1;
    private bool isCollected = false;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(PopupEffect());
    }

    IEnumerator PopupEffect()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            GameManager.instance.AddCoin(value);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.CoinCollect);
            Destroy(gameObject);
        }
    }
}