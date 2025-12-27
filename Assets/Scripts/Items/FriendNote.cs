using UnityEngine;

public class FriendNote : BaseItem
{
    [Header("ID письма")]
    [Tooltip("ID письма из LetterManager. Если -1, будет выбран случайное письмо для текущего этажа")]
    public int letterId = -1;

    private LetterData currentLetterData;

    private void Awake()
    {
        itemName = "Письмо друга";
        isConsumable = false;
    }

    private void Start()
    {
        // Определяем письмо при старте
        InitializeLetter();
    }

    private void InitializeLetter()
    {
        if (LetterManager.Instance == null)
        {
            Debug.LogWarning("LetterManager не найден! Письмо не будет работать.");
            return;
        }

        // Если ID не задан, выбираем случайное письмо для текущего этажа
        if (letterId == -1)
        {
            FloorManager.FloorCategory currentFloor = DetermineFloor();
            currentLetterData = LetterManager.Instance.GetRandomLetterForFloor(currentFloor);
        }
        else
        {
            currentLetterData = LetterManager.Instance.GetLetter(letterId);
        }

        if (currentLetterData == null)
        {
            Debug.LogWarning($"Не удалось получить письмо для FriendNote на {gameObject.name}");
        }
    }

    private FloorManager.FloorCategory DetermineFloor()
    {
        // Сначала пробуем получить из GameManager
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.PlayerCurrentFloor;
        }

        // Если GameManager недоступен, определяем по родительскому объекту
        Transform parent = transform.parent;
        while (parent != null)
        {
            string parentName = parent.name.ToLower();
            if (parentName.Contains("basement") || parentName.Contains("подвал"))
            {
                return FloorManager.FloorCategory.Basement;
            }
            if (parentName.Contains("main") || parentName.Contains("floor") || parentName.Contains("этаж"))
            {
                return FloorManager.FloorCategory.Main;
            }
            parent = parent.parent;
        }

        // По умолчанию возвращаем Main
        return FloorManager.FloorCategory.Main;
    }

    public override void Use(PlayerController playerController)
    {
        if (currentLetterData == null)
        {
            InitializeLetter();
            if (currentLetterData == null)
            {
                Debug.LogWarning("Не удалось инициализировать письмо!");
                return;
            }
        }

        // Показываем UI панель с письмом
        if (LetterUI.Instance != null)
        {
            LetterUI.Instance.ShowLetter(currentLetterData);
        }
        else
        {
            Debug.LogWarning("LetterUI не найден! Не удалось показать письмо.");
        }
    }

    public int GetLetterId()
    {
        if (currentLetterData != null)
        {
            return currentLetterData.id;
        }
        return letterId;
    }
}
