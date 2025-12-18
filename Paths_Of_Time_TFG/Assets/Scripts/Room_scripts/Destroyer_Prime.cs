using UnityEngine;

public class Destroyer_Prime : MonoBehaviour
{ // esto está en el trigger Spawn 00 de cada sala

    private void OnTriggerEnter(Collider other)
    {
        // si un spawnroom choca con el origen de una sala, se elimina
        if (other.CompareTag("spawnroom"))
        {
            Destroy(other.gameObject);
        }
        // si una sala cerrada se genera sobre una sala normal, se elimina
        if (other.CompareTag("closedspawn"))
        {
            Destroy(other.gameObject.transform.parent.gameObject);
        }
    }
}
