// (필수) 'Editor' 폴더 안에 있어야 합니다.
using UnityEngine;
using UnityEditor;

public class ReplaceParentByChildPartialName : EditorWindow
{
    public GameObject newPrefab; // 1. 교체할 '새로운' 프리팹
    public string oldObjectNamePrefix; // 2. 검색할 '옛날' 부모 오브젝트의 이름 (예: "Openable_Door_")
    
    // 3. ◀◀◀ (수정) 위치를 가져올 자식의 '공통된 이름 앞부분'
    public string positionChildNamePrefix; 

    [MenuItem("Tools/Replace Parent With Prefab (Find Child By Partial Name)")]
    static void CreateWizard()
    {
        GetWindow<ReplaceParentByChildPartialName>("Replace Parent (By Child Partial Name)");
    }

    void OnGUI()
    {
        newPrefab = (GameObject)EditorGUILayout.ObjectField("New Prefab", newPrefab, typeof(GameObject), false);
        oldObjectNamePrefix = EditorGUILayout.TextField("Old Object Name Prefix", oldObjectNamePrefix);
        
        // 4. ◀◀◀ (수정) '공통된 이름 앞부분'을 받도록 변경
        EditorGUILayout.HelpBox("'Old Object Name Prefix'로 시작하는 부모를 찾은 뒤, 'Position Source Child Name Prefix'로 이름이 시작하는 첫 번째 자식의 '월드 위치'에 'New Prefab'을 생성하고 옛날 부모를 삭제합니다.", MessageType.Info);
        positionChildNamePrefix = EditorGUILayout.TextField("Position Source Child Name Prefix", positionChildNamePrefix);

        if (GUILayout.Button("EXECUTE REPLACEMENT"))
        {
            ReplaceObjects();
        }
    }

    void ReplaceObjects()
    {
        if (newPrefab == null || string.IsNullOrEmpty(oldObjectNamePrefix) || string.IsNullOrEmpty(positionChildNamePrefix))
        {
            Debug.LogError("모든 필드를 채워주세요.");
            return;
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
        int replacedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            // 5. '옛날 부모'를 이름으로 찾음
            if (obj.name.StartsWith(oldObjectNamePrefix) && obj.transform.parent != null)
            {
                // 6. ◀◀◀ (수정) 자식을 '이름 일부'로 찾습니다.
                Transform childTransform = null;
                foreach (Transform child in obj.transform)
                {
                    // 자식의 이름이 'positionChildNamePrefix'로 시작하는지 확인
                    if (child.name.StartsWith(positionChildNamePrefix))
                    {
                        childTransform = child;
                        break; // 첫 번째로 찾은 자식을 사용
                    }
                }

                if (childTransform == null)
                {
                    Debug.LogWarning(obj.name + "에서 '" + positionChildNamePrefix + " '(으)로 시작하는 자식을 찾지 못해 건너뜁니다.");
                    continue;
                }

                // 7. (이하 동일) '자식'의 월드 위치/회전/스케일을 가져옵니다.
                Vector3 spawnPosition = childTransform.position;
                Quaternion spawnRotation = childTransform.rotation;
                Vector3 spawnScale = childTransform.localScale; 

                // 8. 새 프리팹 인스턴스를 '연결된 상태'로 생성
                GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab, obj.transform.parent);
                Undo.RegisterCreatedObjectUndo(newInstance, "Replaced with prefab");

                // 9. 새 인스턴스의 '월드' 좌표를 '자식'의 월드 좌표로 설정
                newInstance.transform.position = spawnPosition;
                newInstance.transform.rotation = spawnRotation;
                newInstance.transform.localScale = spawnScale;
                newInstance.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());

                // 10. (중요) '옛날 부모 오브젝트(obj)'를 삭제
                Undo.DestroyObjectImmediate(obj);

                replacedCount++;
            }
        }
        
        Debug.Log(replacedCount + "개의 오브젝트 교체 완료!");
    }
}