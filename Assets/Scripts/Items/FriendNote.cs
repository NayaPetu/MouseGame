using UnityEngine;

public class FriendNote : BaseItem
{
    [TextArea] public string noteText = "Друг в подвале!";

    private void Awake()
    {
        itemName = "Записка друга";
        isConsumable = false;
    }

    public override void Use(PlayerController playerController)
    {
        Debug.Log($"Записка: {noteText}");
        // Здесь можно добавить вызов UI для отображения текста
    }
}
