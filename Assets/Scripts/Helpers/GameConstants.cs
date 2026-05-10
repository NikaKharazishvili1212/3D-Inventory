/// <summary> Centralized magic numbers and configuration constants for the project. </summary>
public static class GameConstants
{
    public const int TotalInventorySlots = 16;
    public const int InventoryColumns = 4;
    public const int MaxSpawnedItems = 20;
    public const float SpawnCDMin = 3;
    public const float SpawnCDMax = 6;

    public const float ItemMaxPickupDistance = 5;
    public const float ItemMinGroundY = 0.2f;
    public const float ItemScrollSensitivity = 3;
    public const float ItemDistanceMin = 1;
    public const float ItemDistanceMax = 4;
    public const float ItemCollisionSoundThreshold = 6;
    public const float ItemRotateSensitivity = 1;
    public const float ItemThrowStrength = 20;

    public const float MovementThreshold = 0.3f;
    public const float MessageLifespan = 15;
}