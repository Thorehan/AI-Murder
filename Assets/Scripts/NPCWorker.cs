using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.Json;



public class NPCWorker : MonoBehaviour
{
    public string npcName = "Worker";
    public CoolDown SpeakCD = new CoolDown(2f);
    public Room currentRoom;
    public float taskDuration = 3f;
    public float minDistanceToOther = 2f;

    private static List<NPCWorker> allNPCs = new List<NPCWorker>();

    private List<Transform> taskPoints = new List<Transform>();
    private Transform currentTarget;
    private NavMeshAgent agent;
    private bool isDoingTask = false;



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
        StartCoroutine(SendSignalToLocalhost(npcName,"Game Started, pick where will you go"));
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
        StartCoroutine(SendSignalToLocalhost(npcName,"you have waited for " + taskDuration));
    }



    public void OnSignal(string signal)
    {
        // Send API call to localhost with the signal
        StartCoroutine(SendSignalToLocalhost(npcName,signal));
    }

    IEnumerator SendSignalToLocalhost(string npcname,string signal)
    {
        //Debug.Log($"Sending signal: {signal}");
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
                //Debug.Log($"{www.downloadHandler.text}");
                // Fallback: simple string extraction for 'action' if JObject fails
                string json = www.downloadHandler.text.ToString();
                //Debug.Log($"Received JSON: {json}");
                string action = null;
                string cleaned = json.Trim('"');
                string unescapedJson = System.Text.RegularExpressions.Regex.Unescape(cleaned);
                // Try to parse with JObject first
                var jobj = JObject.Parse(unescapedJson);
                if (jobj != null && jobj["action"] != null)
                {
                    action = jobj["action"].Value<string>();
                    if (!string.IsNullOrEmpty(action))
                        Debug.Log($"The {npcName} did {action}\n thinking: {jobj["thinking"]}\n message: {jobj["message"]}");
                }

                //if (jobj["message"]?.Value<string>() != null)
                //{
                //    if (currentRoom != null)
                //        currentRoom.BroadcastMessageToWorkers($"{npcName}: {jobj["message"].Value<string>()}");
                //}
                
                if (action == null)
                    {
                        Debug.Log("message null");
                        goto fk_it;
                    }
                if (action.Contains("<DinnerRoom>"))
                {
                    Transform newTarget = TaskRooms["DinnerRoom"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (action.Contains("<Kicten>"))
                {
                    Transform newTarget = TaskRooms["Kicten"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (action.Contains("<TeaRoom>"))
                {
                    Transform newTarget = TaskRooms["TeaRoom"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (action.Contains("<Garden>"))
                {
                    Transform newTarget = TaskRooms["Garden"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (action.Contains("<Corridor>"))
                {
                    Transform newTarget = TaskRooms["Corridor"];
                    currentTarget = newTarget;
                    agent.SetDestination(currentTarget.position);
                    isDoingTask = false;
                }
                else if (action.Contains("<Speak>"))
                {
                    string message = action.Replace("<Speak>", "").Replace("</Speak>", "");
                    if (currentRoom != null)
                    {
                        StartCoroutine(Speak(message));
                    }
                }
                
                fk_it:;
                
            }
        }
    }

    public IEnumerator AppendToNpc(string npc,string signal)
    {
        
        Debug.Log($"Sending signal: {signal} to NPC: {npc}");
        string temp = npc.Replace(" ", "");
        string url = $"http://localhost:8000/{temp}?prompt={UnityWebRequest.EscapeURL(signal)}&character={UnityWebRequest.EscapeURL(temp)}";
        yield return null;
    }
    public IEnumerator Speak(string message)
    {
        yield return new WaitForSeconds(2f); // Simulate speaking delay
        if ((bool)SpeakCD)
        {
            Debug.Log($"{npcName} says: {message}");
            currentRoom?.BroadcastMessageToWorkersCD($"{npcName}: {message}");
        }
    }

    public void GetSpeak(string message)
    {
        SpeakCD.putCD();
        string url = $"http://localhost:8000/getAddToMemory?character={npcName}&prompt={message}";
        UnityWebRequest.Get(url);
        StartCoroutine(Speak(message));
    }
    void OnDestroy()
    {
        allNPCs.Remove(this);
    }
}
