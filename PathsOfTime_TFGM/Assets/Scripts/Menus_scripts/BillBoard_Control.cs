using UnityEngine;

public class BillBoard_Control : MonoBehaviour
{
    void LateUpdate()
    {
        // Mirar solo en el eje Y (vertical) para mantenerlos erguidos
        Vector3 lookDir = Camera.main.transform.position - transform.position;
        //lookDir.y = 0; // esto es pa bloquear rotacion en Y
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
