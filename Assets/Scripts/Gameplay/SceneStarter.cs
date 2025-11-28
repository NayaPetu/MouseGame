using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    public FloorGenerator floorGenerator;

    void Start()
    {
        if (floorGenerator == null)
        {
            Debug.LogError("FloorGenerator не назначен!");
            return;
        }

        // «апускаем музыку игры
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();

        // √енерируем первый этаж
        floorGenerator.SpawnFloor(); // теперь метод существует
    }
}
