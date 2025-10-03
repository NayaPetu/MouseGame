using UnityEngine;

// Интерфейс для объектов, с которыми можно взаимодействовать (поднимать, открывать и т.д.)
public interface IInteractable
{
    void Interact(PlayerController player);
}
