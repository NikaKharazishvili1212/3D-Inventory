using UnityEngine;
using UnityEngine.U2D;

/// <summary> Singleton that provides global access to core scene references. </summary>
[DefaultExecutionOrder(-100)] // Ensures GlobalReferences's awake initialization runs first
public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; private set; }

    [field: SerializeField] public Camera cam { get; private set; }
    [field: SerializeField] public Transform playerCamera { get; private set; }
    [field: SerializeField] public SpriteAtlas spriteAtlas { get; private set; }
    [field: SerializeField] public Inventory inventory { get; private set; }
    [field: SerializeField] public ItemSpawner itemSpawner { get; private set; }
    [field: SerializeField] public LogMessage[] logMessages { get; private set; }

    void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
    }
}