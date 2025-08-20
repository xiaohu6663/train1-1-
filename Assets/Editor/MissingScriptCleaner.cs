#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class MissingScriptCleaner
{
	[MenuItem("Tools/Missing Scripts/Clean In Selection")] 
	public static void CleanInSelection()
	{
		int total = 0;
		foreach (var obj in Selection.gameObjects)
		{
			total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
			foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
			{
				total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
			}
		}
		Debug.Log($"Removed {total} missing script component(s) from selection.");
	}

	[MenuItem("Tools/Missing Scripts/Clean All Prefabs")] 
	public static void CleanAllPrefabs()
	{
		string[] guids = AssetDatabase.FindAssets("t:Prefab");
		int total = 0;
		for (int i = 0; i < guids.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null) continue;

			int removed = 0;
			removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
			foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
			{
				removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
			}
			if (removed > 0)
			{
				EditorUtility.SetDirty(prefab);
				total += removed;
			}
		}
		AssetDatabase.SaveAssets();
		Debug.Log($"Removed {total} missing script component(s) from all prefabs.");
	}
}
#endif


