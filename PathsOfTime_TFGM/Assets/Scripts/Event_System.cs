using System;
using UnityEngine;

public class Event_System : MonoBehaviour
{
    public static event Action<int> OnDash;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnDash?.Invoke(10000);
        }
    }
    private void OnEnable()
    {
        Event_System.OnDash += Dash;
    }
    private void OnDisable()
    {
        Event_System.OnDash -= Dash;
    }

    void Dash(int speed)
    {
        Debug.Log("IM " + speed + " SPEED!");
    }
}
