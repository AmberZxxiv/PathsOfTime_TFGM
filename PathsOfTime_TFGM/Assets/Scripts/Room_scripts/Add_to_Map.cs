using UnityEngine;

public class Add_to_Map : MonoBehaviour
{ // este script está en el empty padre de cada sala
    public Room_Manager _RM; //singleton Room_Manager

    void Start()
    {
        // pillo single del ROOM_MAN para añadir sala a lista
        _RM = Room_Manager.instance;
        _RM.roomMap.Add(this.gameObject);
    }
}
