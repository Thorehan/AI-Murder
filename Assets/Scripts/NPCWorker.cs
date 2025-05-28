using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Networking;
using System.Text.Json;
using Newtonsoft.Json.Linq;


public class NPCWorker : MonoBehaviour
{
    public string npcName = "Worker";
    public float taskDuration = 3f;
    public float minDistanceToOther = 2f;

    private static List<NPCWorker> allNPCs = new List<NPCWorker>();

    private List<Transform> taskPoints = new List<Transform>();
    private Transform currentTarget;
    private NavMeshAgent agent;
    private bool isDoingTask = false;

    Dictionary<NPCWorker, Cooldown> cooldowns = new Dictionary<NPCWorker, Cooldown>();

    Dictionary<string, Transform> TaskRooms = new Dictionary<string, Transform>();
    public void AddTaskRoomsFromList(List<Transform> roomList)
    {
        foreach (Transform room in roomList)
        {
            var ee = room.GetComponent<TaskPoint>().Name;
            if (room != null && !TaskRooms.ContainsKey(ee))
            {
                string temp = ee;
                TaskRooms.Add(temp, room);
            }
        }
    }


    //public Transform sleepPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        allNPCs.Add(this);

        foreach (TaskPoint tp in FindObjectsOfType<TaskPoint>())
        {
            taskPoints.Add(tp.transform);
        }

        if (taskPoints.Count == 0)
        {
            Debug.LogError("No task points");
            return;
        }
        AddTaskRoomsFromList(taskPoints);
        StartCoroutine(SendSignalToLocalhost("Game Started, pick where will you go"));
    }

    void Update()
    {
        //if (isDoingTask || currentTarget == null)
        //    return;
//
        //foreach (NPCWorker other in allNPCs)
        //{
        //    if (other == this) continue;
        //    if (other.currentTarget == currentTarget)
        //    {
        //        float distance = Vector3.Distance(transform.position, other.transform.position);
        //        if (distance < minDistanceToOther)
        //        {
        //            //StartCoroutine(SendSignalToLocalhost("TooCloseToOtherNPC"));
        //            //PickNewTarget();
        //            return;
        //        }
        //    }
        //}

        if (!isDoingTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log($"{npcName} gets task");
            StartCoroutine(DoTask());
        }
    }

    void PickNewTarget()
    {
        if (taskPoints.Count == 0) return;

        Transform newTarget = taskPoints[Random.Range(0, taskPoints.Count)];
        currentTarget = newTarget;
        agent.SetDestination(currentTarget.position);
    }

    IEnumerator DoTask()
    {
        isDoingTask = true;
        yield return new WaitForSeconds(taskDuration);
        //PickNewTarget();
        StartCoroutine(SendSignalToLocalhost("you have waited for " + taskDuration));
    }



    public void OnSignal(string signal)
    {
        // Send API call to localhost with the signal
        StartCoroutine(SendSignalToLocalhost(signal));
    }

    IEnumerator SendSignalToLocalhost(string signal)
    {
        Debug.Log($"Sending signal: {signal}");
        string temp = npcName.Replace(" ", "");
        string url = $"http://localhost:8000/{temp}?prompt={UnityWebRequest.EscapeURL(signal)}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //Debug.LogError($"Error sending signal: {www.error}");
            }
            else
            {
                Debug.Log($"{www.downloadHandler.text}");
                // Extract 'message' using JObject
                string json = www.downloadHandler.text;
                string message = null;
                try
                {
                    var jobj = JObject.Parse(json);
                    message = jobj["message"]?.ToString();
                    if (!string.IsNullOrEmpty(message))
                        Debug.Log($"Extracted message: {message}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to parse JSON: {ex.Message}");
                }

                if (message.Contains("<DinnerRoom>"))
                {
                    Transform newTarget = TaskRooms["DinnerRoom"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (message.Contains("<Kicten>"))
                {
                    Transform newTarget = TaskRooms["Kicten"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (message.Contains("<TeaRoom>"))
                {
                    Transform newTarget = TaskRooms["TeaRoom"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (message.Contains("<Garden>"))
                {
                    Transform newTarget = TaskRooms["Garden"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (message.Contains("<Corridor>"))
                {
                    Transform newTarget = TaskRooms["Corridor"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }

                
            }
        }
    }

    void OnDestroy()
    {
        allNPCs.Remove(this);
    }
}
