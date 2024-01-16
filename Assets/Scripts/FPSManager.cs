using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
    public TextMeshProUGUI minFPSText;
    private int minFPS;

    public TextMeshProUGUI maxFPSText;
    private int maxFPS;

    public TextMeshProUGUI curFPSText;
    private int curFPS;

    public TextMeshProUGUI avgFPSText;
    private int avgFPS;

    private List<int> fpsPerFrames;

    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 1.0f;      //Time to measure the amount of frames per refresh time, set to 1.0f for fps

    private void Start()
    {
        //Set the refresh rate to the screen max refresh rate in hz
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

        minFPSText.text = "MIN FPS: " + minFPS.ToString();
        maxFPSText.text = "MAX FPS: " + maxFPS.ToString();
        curFPSText.text = "FPS: " + curFPS.ToString();
        avgFPSText.text = "AVG FPS: " + avgFPS.ToString();

        fpsPerFrames = new List<int>();
    }

    

    void Update()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;

            fpsPerFrames.Add((int)m_lastFramerate);

            minFPS = fpsPerFrames.Min();
            maxFPS = fpsPerFrames.Max();
            curFPS = (int)m_lastFramerate;
            avgFPS = (int)fpsPerFrames.Average();

            minFPSText.text = "MIN FPS: " + minFPS.ToString();
            maxFPSText.text = "MAX FPS: " + maxFPS.ToString();
            curFPSText.text = "FPS: " + curFPS.ToString();
            avgFPSText.text = "AVG FPS: " + avgFPS.ToString();
        }
    }
}
