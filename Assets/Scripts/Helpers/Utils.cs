using UnityEngine;
using System.Threading.Tasks;

/// <summary> General-purpose utility methods shared across the project. </summary>
public static class Utils
{
    // Executes action after delay with safety check for destroyed objects. this.Wait(5f, () => AnyCodeHere());
    public static async void Wait(this MonoBehaviour mono, float delay, System.Action action)
    {
        await Task.Delay((int)(delay * 1000));
        if (mono) action?.Invoke();
    }

    // Returns a random float between min and max, rounded to one decimal place
    public static float RandomRounded(float min, float max) => Mathf.Round(Random.Range(min, max) * 10f) / 10f;

    // Formats a fraction as "used / total" string — used for UI counters
    public static string FormatCounter(int used, int total) => $"{used} / {total}";

    // Wraps text in TMP color tag. "text".Colored("green")
    public static string Colored(this string text, string color) => $"<color={color}>{text}</color>";

    // Displays a log message — reuses an inactive slot if available, otherwise recycles the oldest (first sibling)
    public static void SendLogMessage(string text)
    {
        var logMessages = GlobalReferences.Instance.logMessages;

        foreach (var logMessage in logMessages)
        {
            if (!logMessage.gameObject.activeSelf)
            {
                logMessage.Display(text);
                return;
            }
        }


        var parent = logMessages[0].transform.parent;
        foreach (var logMessage in logMessages)
        {
            if (logMessage.transform == parent.GetChild(0))
            {
                logMessage.Display(text);
                return;
            }
        }
    }
}