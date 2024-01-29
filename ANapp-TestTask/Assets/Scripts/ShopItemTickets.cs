using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemTickets : MonoBehaviour
{
    public int price = 500;
    public bool available = true;
    public GameObject purchased;

    public void SetAvailable()
    {
        available = true;
    }
}
