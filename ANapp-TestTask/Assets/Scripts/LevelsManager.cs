using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public Transform[] levels;
    private int _levelsAvailable = 5;
    
    private void Start()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            // levels[i].GetChild(1).gameObject.SetActive(i>_levelsAvailable);

        }
    }

    // public void PassLevel()
    // {
    //     if(_levelsAvailable+1<levels.Length)
    //         levels[++_levelsAvailable].GetChild(1).gameObject.SetActive(false);
    // }
}
