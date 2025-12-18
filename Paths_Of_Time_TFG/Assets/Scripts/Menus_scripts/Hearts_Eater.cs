using UnityEngine;

public class Hearts_Eater : MonoBehaviour
{
   public GameObject leftHalf;
   public GameObject rightHalf;

   // 2 = lleno, 1 = mordido, 0 = vacío
   public void EatHeart(int value)
    {
     leftHalf.SetActive(value >= 1);
     rightHalf.SetActive(value >= 2);
    }
}
