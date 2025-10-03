using UnityEngine;

public class CheeseKey : MonoBehaviour, IUsable
{
    public void Use(PlayerController player)
    {
        // логика обезвреживани€ ловушки при взаимодействии
        Debug.Log("—ыроключ обезвредил мышеловку!");
    }
}
