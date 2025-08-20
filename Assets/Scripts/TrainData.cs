using UnityEngine;

public class TrainData : MonoBehaviour
{
    [Header("列车信息")]
    public string trainID;
    public string currentTrack = "Track1";
    public float speed = 1f;
    
    [Header("状态信息")]
    public bool isActive = true;
    public bool isPaused = false;
    
    void Start()
    {
        // 如果没有设置trainID，自动生成一个
        if (string.IsNullOrEmpty(trainID))
        {
            trainID = System.Guid.NewGuid().ToString();
        }
    }
    
    public void SetTrack(string trackName)
    {
        currentTrack = trackName;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void Pause()
    {
        isPaused = true;
    }
    
    public void Resume()
    {
        isPaused = false;
    }
}


