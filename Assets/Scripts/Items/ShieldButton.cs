using UnityEngine;

public class ShieldButton : MonoBehaviour, IUsable
{
    public int durability = 2;

    public void Use(PlayerController player)
    {
        Debug.Log("Щит-пуговка активен");
        // можно повесить на игрока, если нужно
    }

    public bool AbsorbHit()
    {
        durability--;

        if (durability <= 0)
        {
            Destroy(gameObject);
            return false;
        }

        return true;
    }
}
