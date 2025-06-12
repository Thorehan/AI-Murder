using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class SimpleNPCUI : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField npcNameInput;
    public TMP_InputField promptInput;
    public Button spawnButton;
    public Button deleteButton;
    public Button sendPromptButton;
    public TextMeshProUGUI feedbackText;

    [Header("Spawn Settings")]
    public GameObject npcPrefab;
    public Transform spawnLocation;
    public List<Transform> taskPoints = new List<Transform>();

    [Header("NPC Management")]
    public float defaultTaskDuration = 3f;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private int npcCounter = 0;

    void Start()
    {
        spawnButton.onClick.AddListener(SpawnNewNPC);
        deleteButton.onClick.AddListener(DeleteNPC);
        sendPromptButton.onClick.AddListener(SendPromptToNPC);
        ShowMessage("Enter NPC name and click Spawn", Color.white);

        FindAllTaskPoints();
    }

    void FindAllTaskPoints()
    {
        TaskPoint[] taskPointComponents = FindObjectsOfType<TaskPoint>();
        foreach (TaskPoint tp in taskPointComponents)
        {
            taskPoints.Add(tp.transform);
        }
        Debug.Log($"Found {taskPoints.Count} task points");
    }

    public void SpawnNewNPC()
    {
        string npcName = npcNameInput.text.Trim();
        string prompt = promptInput.text.Trim();

        if (string.IsNullOrEmpty(npcName))
        {
            npcName = GetUniqueNPCName(); // Otomatik isim ver
        }

        if (NPCExists(npcName))
        {
            ShowMessage($"NPC '{npcName}' already exists!", Color.yellow);
            return;
        }

        StartCoroutine(CreateNPCProcess(npcName, prompt));
    }

    public void DeleteNPC()
    {
        string npcName = npcNameInput.text.Trim();

        if (string.IsNullOrEmpty(npcName))
        {
            ShowMessage("Please enter a name to delete!", Color.red);
            return;
        }

        if (!NPCExists(npcName))
        {
            ShowMessage($"NPC '{npcName}' not found!", Color.yellow);
            return;
        }

        StartCoroutine(DeleteNPCProcess(npcName));
    }

    void SendPromptToNPC()
    {
        string npcName = npcNameInput.text.Trim();
        string prompt = promptInput.text.Trim();

        if (string.IsNullOrEmpty(npcName) || string.IsNullOrEmpty(prompt))
        {
            ShowMessage("Enter both NPC name and prompt!", Color.red);
            return;
        }

        NPCWorker[] allNPCs = FindObjectsOfType<NPCWorker>();
        foreach (NPCWorker npc in allNPCs)
        {
            if (npc.npcName == npcName)
            {
                npc.OnSignal(prompt);
                ShowMessage($"Sent prompt to '{npcName}': {prompt}", Color.cyan);
                return;
            }
        }

        ShowMessage($"NPC '{npcName}' not found!", Color.red);
    }

    IEnumerator CreateNPCProcess(string npcName, string prompt)
    {
        ShowMessage($"Creating '{npcName}'...", Color.blue);

        yield return StartCoroutine(CallPythonAPI($"http://localhost:8000/addAgent?name={npcName}&system_prompt={prompt}"));

        GameObject newNPC = SpawnNPCGameObject(npcName);

        if (newNPC != null)
        {
            ShowMessage($"Successfully created '{npcName}'!", Color.green);
            npcNameInput.text = "";
        }
        else
        {
            ShowMessage($"Failed to spawn '{npcName}'!", Color.red);
        }
    }

    IEnumerator DeleteNPCProcess(string npcName)
    {
        ShowMessage($"Deleting '{npcName}'...", Color.blue);

        yield return StartCoroutine(CallPythonAPI($"http://localhost:8000/removeAgent?name={npcName}"));

        if (RemoveNPCGameObject(npcName))
        {
            ShowMessage($"Deleted '{npcName}'!", Color.green);
            npcNameInput.text = "";
        }
        else
        {
            ShowMessage($"Could not find '{npcName}' in scene!", Color.yellow);
        }
    }

    GameObject SpawnNPCGameObject(string npcName)
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab is not assigned!");
            return null;
        }

        Vector3 spawnPos = spawnLocation != null ? spawnLocation.position : Vector3.zero;

        GameObject npcObject = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npcObject.name = npcName;

        NPCWorker npcWorker = npcObject.GetComponent<NPCWorker>();
        if (npcWorker != null)
        {
            npcWorker.npcName = npcName;
            npcWorker.taskDuration = defaultTaskDuration;
            npcWorker.AddTaskRoomsFromList(taskPoints);
        }
        else
        {
            Debug.LogError($"NPCWorker component not found on {npcName}!");
        }

        spawnedNPCs.Add(npcObject);
        Debug.Log($"Spawned NPC: {npcName} at {spawnPos}");

        return npcObject;
    }

    bool RemoveNPCGameObject(string npcName)
    {
        NPCWorker[] allNPCs = FindObjectsOfType<NPCWorker>();
        foreach (NPCWorker npc in allNPCs)
        {
            if (npc.npcName == npcName)
            {
                spawnedNPCs.Remove(npc.gameObject);
                DestroyImmediate(npc.gameObject);
                Debug.Log($"Removed NPC: {npcName}");
                return true;
            }
        }
        return false;
    }

    IEnumerator CallPythonAPI(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"API call failed: {www.error}");
            }
            else
            {
                Debug.Log($"API Response: {www.downloadHandler.text}");
            }
        }
    }

    bool NPCExists(string npcName)
    {
        NPCWorker[] allNPCs = FindObjectsOfType<NPCWorker>();
        foreach (NPCWorker npc in allNPCs)
        {
            if (npc.npcName == npcName)
                return true;
        }
        return false;
    }

    string GetUniqueNPCName()
    {
        string baseName = "SpawnedNPC";
        string candidateName;
        do
        {
            candidateName = $"{baseName}{npcCounter}";
            npcCounter++;
        } while (NPCExists(candidateName));
        return candidateName;
    }

    void ShowMessage(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log($"NPC UI: {message}");
    }

    [ContextMenu("Test Spawn Random NPC")]
    void TestSpawnRandomNPC()
    {
        npcNameInput.text = "TestNPC" + Random.Range(100, 999);
        SpawnNewNPC();
    }

    [ContextMenu("List All NPCs")]
    void ListAllNPCs()
    {
        NPCWorker[] allNPCs = FindObjectsOfType<NPCWorker>();
        Debug.Log($"=== Found {allNPCs.Length} NPCs ===");
        foreach (NPCWorker npc in allNPCs)
        {
            Debug.Log($"- {npc.npcName}");
        }
    }

    public int GetActiveNPCCount()
    {
        return FindObjectsOfType<NPCWorker>().Length;
    }
}
