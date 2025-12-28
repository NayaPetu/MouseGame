using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LetterData
{
    public int id;
    public string title; // Заголовок письма
    public string text;
    public FloorManager.FloorCategory floorCategory;
}

public class LetterManager : MonoBehaviour
{
    public static LetterManager Instance;

    [Header("Письма для этажа")]
    [SerializeField] private List<LetterData> mainFloorLetters = new List<LetterData>();
    
    [Header("Письма для подвала")]
    [SerializeField] private List<LetterData> basementLetters = new List<LetterData>();

    private Dictionary<int, LetterData> allLetters = new Dictionary<int, LetterData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeLetters();
    }

    private void InitializeLetters()
    {
        allLetters.Clear();

        // Добавляем письма для этажа
        foreach (var letter in mainFloorLetters)
        {
            if (!allLetters.ContainsKey(letter.id))
            {
                allLetters[letter.id] = letter;
            }
            else
            {
                Debug.LogWarning($"Письмо с ID {letter.id} уже существует!");
            }
        }

        // Добавляем письма для подвала
        foreach (var letter in basementLetters)
        {
            if (!allLetters.ContainsKey(letter.id))
            {
                allLetters[letter.id] = letter;
            }
            else
            {
                Debug.LogWarning($"Письмо с ID {letter.id} уже существует!");
            }
        }
    }

    public LetterData GetLetter(int id)
    {
        if (allLetters.ContainsKey(id))
        {
            return allLetters[id];
        }

        Debug.LogWarning($"Письмо с ID {id} не найдено!");
        return null;
    }

    public List<LetterData> GetLettersForFloor(FloorManager.FloorCategory floor)
    {
        List<LetterData> letters = new List<LetterData>();
        
        foreach (var letter in allLetters.Values)
        {
            if (letter.floorCategory == floor)
            {
                letters.Add(letter);
            }
        }

        return letters;
    }

    public LetterData GetRandomLetterForFloor(FloorManager.FloorCategory floor)
    {
        List<LetterData> availableLetters = GetLettersForFloor(floor);
        
        if (availableLetters.Count == 0)
        {
            Debug.LogWarning($"Нет писем для этажа {floor}!");
            return null;
        }

        return availableLetters[Random.Range(0, availableLetters.Count)];
    }
}

