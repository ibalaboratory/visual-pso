using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TimeScaleController : MonoBehaviour
{
    private Slider slider;
    private Text label;

    void Start()
    {
        slider = GetComponent<Slider>();
        if(slider == null)
        {
            Debug.LogError("Slider not found.");
            this.enabled = false;
            return;
        }

        Text[] texts = GetComponentsInChildren<Text>();
        foreach(Text text in texts)
        {
            if(text.name == "Label")
            {
                label = text;
                break;
            }
        }
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = Mathf.Clamp(timeScale, slider.minValue, slider.maxValue);

        if(label != null)
        {
            label.text = $"Time Scale x{timeScale:F1}";
        }
    }
}
