using System;
using Unity.VisualScripting;
using UnityEngine;

public class Chest_Opener : MonoBehaviour
{
    public bool looted = false;
    public GameObject coin;
    public GameObject cherry;
    GameObject _closed1;
    GameObject _open2;

    void Start()
    {
        _closed1 = transform.GetChild(0).gameObject;
        _open2 = transform.GetChild(1).gameObject;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && looted==false)
        {
            Instantiate(coin, transform.position + Vector3.up * 2f, transform.rotation);
            Instantiate(cherry, transform.position + Vector3.up * 2f, transform.rotation);
            _closed1.gameObject.SetActive(false);
            _open2.gameObject.SetActive(true);
            looted = true;
        }
    }
}
