using UnityEngine;
using UnityEngine.SceneManagement;

public class CheeseFactory : MonoBehaviour
{
    [SerializeField] private string endCutsceneSceneName = "EndCutscene"; // сцена с финальной катсценой

    private bool playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (PlayerInventory.Instance.HasRecipe())
            {
                PlayerInventory.Instance.UseRecipe();
                Debug.Log("Рецепт использован в CheeseFactory! Игра завершена!");
                EndGame();
            }
            else
            {
                Debug.Log("У тебя нет рецепта!");
            }
        }
    }

    private void EndGame()
    {
        // Вместо прямой загрузки EndGame сначала загружаем сцену с финальной катсценой
        SceneManager.LoadScene(endCutsceneSceneName);
    }
}
