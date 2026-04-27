using TMPro;
using UnityEngine;

public class LogMessage : MonoBehaviour
{
    [SerializeField] TextMeshPro tmpro;
    [SerializeField] float lifespan;

    public void Display(string text)
    {
        lifespan = GameConstants.MessageLifespan;
        tmpro.text = text;
        tmpro.transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (lifespan <= 0) gameObject.SetActive(false);
        lifespan -= Time.deltaTime;
    }
}