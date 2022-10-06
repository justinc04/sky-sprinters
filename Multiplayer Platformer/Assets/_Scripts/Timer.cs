using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timeText;
    public bool timerIsRunning = false;
    public float time;

    void Update()
    {
        if (timerIsRunning)
        {
            time += Time.deltaTime;
            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = Mathf.FloorToInt(time % 60);
            float milliSeconds = (time % 1) * 1000;
            timeText.text = string.Format("{0:0}:{1:00}.{2:000}", minutes, seconds, milliSeconds);
        }
    }
}
