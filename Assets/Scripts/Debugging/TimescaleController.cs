using UnityEngine;

public class TimescaleController : MonoBehaviour
{
    public void SetTimescale(float timescale)
    {
        Time.timeScale = Mathf.Clamp(timescale, 0, Mathf.Infinity);
    }
}
