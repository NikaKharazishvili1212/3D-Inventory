using UnityEngine;

public enum Type { Food, Potion, Weapon }

/// <summary> Defines the static data for an item — id, name, weight, sound, and type. </summary>
[CreateAssetMenu(fileName = "Item")]
public class ScriptableItem : ScriptableObject
{
    [field: SerializeField] public int id { get; private set; }
    [field: SerializeField] public new string name { get; private set; }
    [field: SerializeField] public float weight { get; private set; } // Heavier items fall more forcefully
    [field: SerializeField] public AudioClip sound { get; private set; } // Plays a unique sound based on the item type when the item is dropped
    [field: SerializeField] public Type type { get; private set; }
}