using UnityEngine;

public class ShieldButton : MonoBehaviour, IUsable
{
    public int durability = 2; // выдерживает 2 удара кота

    public void Use(PlayerController player)
    {
        // этот предмет просто хранится, использование вручную не нужно
        Debug.Log("Щит-пуговка активен. Защита от кота!");
    }

    public bool AbsorbHit()
    {
        durability--;
        if (durability <= 0)
        {
            Destroy(gameObject);
            return false; // больше не защищает
        }
        return true;
    }
}
