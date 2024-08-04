using UnityEngine;

/// <summary>
/// Static class that loads all Items in the Resources/Items folder into the an Item array
/// </summary>
public static class ItemDatabase 
{
    public static Item[] Items { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]private static void Initialize() => Items = Resources.LoadAll<Item>("Items/");
}
