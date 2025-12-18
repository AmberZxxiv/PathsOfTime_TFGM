using UnityEngine;

public class Add_to_Map : MonoBehaviour
{ // este script está en el empty padre de cada sala

    public Room_Manager _RM; //singleton ROM_MAN
   
    void Start()
    {
        // pillo el singleton del ROOM_MAN
        _RM = Room_Manager.instance;
        // añado la sala spawneada al mapa del ROOM_MAN
        _RM.roomMap.Add(this.gameObject);

    }
}
