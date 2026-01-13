using UnityEngine;

public class BillBoard_General : MonoBehaviour
{
    void LateUpdate()
    {
        // Mirar solo en el eje Y (vertical) para mantenerlos erguidos
        Vector3 lookDir = Camera.main.transform.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
