// 1. (필수) 이 스크립트는 'Editor'라는 이름의 폴더 안에 있어야 합니다.
// 2. (필수) UnityEditor 네임스페이스를 사용합니다.
using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
    // 1. 교체할 '설계도'
    public GameObject newPrefab;
    
    // 2. '옛날 방식' 오브젝트들이 공통으로 가진 이름 (검색용)
    public string oldObjectNamePrefix = "OldDoor_"; 

    // 3. 에디터 상단에 "Tools/Replace Objects" 메뉴를 만듭니다.
    [MenuItem("Tools/Replace Objects With Prefab")]
    static void CreateWizard()
    {
        GetWindow<ReplaceWithPrefab>("Replace Objects");
    }

    // 4. 에디터 윈도우 UI
    void OnGUI()
    {
        newPrefab = (GameObject)EditorGUILayout.ObjectField("New Prefab", newPrefab, typeof(GameObject), false);
        oldObjectNamePrefix = EditorGUILayout.TextField("Old Object Name Prefix", oldObjectNamePrefix);

        if (GUILayout.Button("EXECUTE REPLACEMENT"))
        {
            ReplaceObjects();
        }
    }

    // 5. ◀◀◀ 핵심 교체 로직
    void ReplaceObjects()
    {
        if (newPrefab == null || string.IsNullOrEmpty(oldObjectNamePrefix))
        {
            Debug.LogError("프리팹과 이름을 지정하세요.");
            return;
        }

        // 씬의 모든 오브젝트를 뒤집니다.
        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID))
        {
            // 만약 이름이 "OldDoor_"로 시작한다면
            if (obj.name.StartsWith(oldObjectNamePrefix))
            {
                // 1. 새 프리팹 인스턴스를 '연결된 상태'로 생성
                GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab, obj.transform.parent);

                // 2. 옛날 오브젝트의 위치/회전/스케일을 그대로 복사
                newInstance.transform.position = obj.transform.position;
                newInstance.transform.rotation = obj.transform.rotation;
                newInstance.transform.localScale = obj.transform.localScale;
                newInstance.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());

                // 3. (중요) 옛날 오브젝트는 '즉시' 삭제
                DestroyImmediate(obj);
            }
        }
        
        Debug.Log("교체 완료!");
    }
}