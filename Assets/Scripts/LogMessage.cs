using TMPro;
using UnityEngine;
using static GameConstants;

public class LogMessage : MonoBehaviour
{
    [SerializeField] TextMeshPro tmpro;
    [SerializeField] float lifespan;

    public void Display(string text)
    {
        lifespan = MessageLifespan;
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