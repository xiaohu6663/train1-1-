using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class PNGLoader : MonoBehaviour
{
    public string folderPath = "GraffitiTest"; // ��Assets�´������ļ���
    public float loadInterval = 2.0f; // ���ؼ��

    private List<Texture2D> loadedTextures = new List<Texture2D>();

    IEnumerator Start()
    {
        yield return StartCoroutine(LoadAllPNGFiles());

        // ��������Ϳѻ
        StartCoroutine(AddGraffitiPeriodically());
    }

    // ��������PNG�ļ�
    IEnumerator LoadAllPNGFiles()
    {
        // �����������
        foreach (var tex in loadedTextures)
        {
            Destroy(tex);
        }
        loadedTextures.Clear();

        // ��ȡ����PNG�ļ�·��
        string fullPath = Path.Combine(Application.dataPath, folderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"Directory not found: {fullPath} - PNGLoader will be disabled");
            yield break;
        }

        string[] files = Directory.GetFiles(fullPath, "*.png");

        if (files.Length == 0)
        {
            Debug.LogWarning($"No PNG files found in: {fullPath}");
            yield break;
        }

        // ������������
        foreach (string filePath in files)
        {
            // ����.meta�ļ�
            if (filePath.EndsWith(".meta")) continue;

            // �������·��������Unity���ԣ�
            string relativePath = "Assets/" + folderPath + "/" + Path.GetFileName(filePath);

            // ��������
            Texture2D tex = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(filePath);
            bool success = tex.LoadImage(fileData);

            if (success)
            {
                tex.name = Path.GetFileNameWithoutExtension(filePath);
                loadedTextures.Add(tex);
                Debug.Log($"Loaded texture: {relativePath} ({tex.width}x{tex.height})");
            }
            else
            {
                Debug.LogWarning($"Failed to load texture: {relativePath}");
            }

            yield return null; // ÿ֡����һ�������⿨��
        }

        Debug.Log($"Loaded {loadedTextures.Count} PNG images from {folderPath}");
    }

    IEnumerator AddGraffitiPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(loadInterval);

            if (loadedTextures.Count > 0)
            {
                AddRandomGraffiti();
            }
        }
    }

    public void AddRandomGraffiti()
    {
        if (loadedTextures.Count == 0)
        {
            Debug.LogWarning("No textures loaded. Cannot add graffiti.");
            return;
        }

        Texture2D randomTex = loadedTextures[Random.Range(0, loadedTextures.Count)];

        // ȷ��GraffitiManagerʵ������
        if (GraffitiManager.Instance == null)
        {
            Debug.LogError("GraffitiManager instance is missing!");
            return;
        }

        // ������ȷ�ķ���
        GraffitiManager.Instance.AddGraffiti(randomTex);
        Debug.Log($"Added graffiti: {randomTex.name}");
    }

    // ���¼������������ķ���
    public IEnumerator ReloadTextures()
    {
        Debug.Log("Reloading textures...");
        yield return StartCoroutine(LoadAllPNGFiles());
        Debug.Log("Texture reload complete.");
    }
}