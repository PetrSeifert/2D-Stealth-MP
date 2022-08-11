using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private Text fpsText;
    private int updateCount = 0;
    private float deltaTimesSum = 0;

    void Awake()
    {
        fpsText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        updateCount++;
        deltaTimesSum += Time.deltaTime;
        if (deltaTimesSum > 1)
        {
            int fps = Mathf.RoundToInt(updateCount / deltaTimesSum);
            fpsText.text = $"{fps} fps";
            updateCount = 0;
            deltaTimesSum = 0;
        }
    }
}
