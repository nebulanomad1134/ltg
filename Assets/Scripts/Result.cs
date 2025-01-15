using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;
    public GameObject btnNext; 

    public void Lose()
    {
        titles[0].SetActive(true);
        if (btnNext != null)
            btnNext.SetActive(false);
    }

    public void Win()
    {
        titles[1].SetActive(true);
        ShowNextMapButton(GameManager.instance.currentMap < GameManager.instance.maxMap);
    }

    public void ShowNextMapButton(bool show)
    {
        if (btnNext != null)
            btnNext.SetActive(show);
    }

}