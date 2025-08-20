using UnityEngine;
using System.IO;

public class ConfigLoader : MonoBehaviour
{
    public string csvFileName = "config.csv"; // StreamingAssets �µ��ļ���
    public UDPManager targetConfig;        // Ŀ��ű�����

    void Start()
    {
        LoadConfigFromCSV();
    }

    void LoadConfigFromCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError("�����ļ�δ�ҵ�: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
        {
            Debug.LogError("�����ļ����ݲ���: " + filePath);
            return;
        }

        // �����ڶ��У���һ��Ϊ��ͷ��
        string[] values = lines[1].Split(',');

        if (values.Length < 3)
        {
            Debug.LogError("�����ֶβ�����: " + lines[1]);
            return;
        }

        // ��ֵ��Ŀ�����
        if (targetConfig != null)
        {
            int.TryParse(values[0], out targetConfig.localPort);
            targetConfig.remoteIp = values[1];
            int.TryParse(values[2], out targetConfig.remotePort);

            Debug.Log($"����������: localPort={targetConfig.localPort}, remoteIp={targetConfig.remoteIp}, remotePort={targetConfig.remotePort}");
        }
    }
}
