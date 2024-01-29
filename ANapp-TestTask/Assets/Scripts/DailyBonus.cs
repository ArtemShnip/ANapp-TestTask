using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonus : MonoBehaviour
{
    [SerializeField] private GameObject bonusPage;
    [SerializeField] private TextMeshProUGUI bonusPageDayTitle, balanceLabel, dailyBarLabel;
    [SerializeField] private Image dailyBar;

    public static int Balance
    {
        get => PlayerPrefs.GetInt("Balance");
        set
        {
            PlayerPrefs.SetInt("Balance", value);
            OnBalanceChange.Invoke();
        }
    }
    public static Action OnBalanceChange;

    private int[] _dayBonusValues = new[] { 5, 5, 10, 10, 15, 15 };

    private static DateTime? LastVisitTime
    {
        get
        {
            if (DateTime.TryParse(PlayerPrefs.GetString("LastVisitTime"), out var date))
                return date;

            return null;
        }
        set { PlayerPrefs.SetString("LastVisitTime", value.ToString()); }
    }

    private static int DaysStreak
    {
        get => PlayerPrefs.GetInt("DaysStreak", 0);
        set
        {
            if (value > 6)
                PlayerPrefs.SetInt("DaysStreak", 1);
            else
                PlayerPrefs.SetInt("DaysStreak", value);
        }
    }

    private void Start()
    {
        // PlayerPrefs.DeleteAll();
        OnBalanceChange += () =>
        {
            balanceLabel.text = Balance.ToString();
        };
        OnBalanceChange.Invoke();
        if (LastVisitTime == null || (DateTime.Now-LastVisitTime).Value.Days >= 1)
        {
            LastVisitTime = DateTime.Now;
            DaysStreak++;
            bonusPage.SetActive(true); 
            Balance += _dayBonusValues[DaysStreak-1];
            bonusPageDayTitle.text = $"DAY {DaysStreak}";
        }
        dailyBarLabel.text = $"{DaysStreak}/7";
        dailyBar.fillAmount = DaysStreak / 6f;
    }
}