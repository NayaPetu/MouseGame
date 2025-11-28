using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    public FloorGenerator floorGenerator;

    void Start()
    {
        if (floorGenerator == null)
        {
            Debug.LogError("RoomGenerator не назначен!");
            return;
        }

        // Запускаем музыку игры
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();

        // Генерируем первый этаж
        floorGenerator.SpawnFloor(); 
    }
}
