using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public string roomName = "Room";
    List<NPCWorker> workersInRoom = new List<NPCWorker>();
    public void AddWorker(NPCWorker worker)
    {
        if (!workersInRoom.Contains(worker))
        {
            workersInRoom.Add(worker);
            Debug.Log($"{worker.npcName} added to {roomName}");
        }
    }
    public void RemoveWorker(NPCWorker worker)
    {
        if (workersInRoom.Contains(worker))
        {
            workersInRoom.Remove(worker);
            Debug.Log($"{worker.npcName} removed from {roomName}");
        }
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.TryGetComponent<NPCWorker>(out NPCWorker npc))
        {
            BroadcastMessageToWorkers($"{npc.npcName} has entered the {roomName}");
            AddWorker(npc);
            npc.currentRoom = this;
            Debug.Log($"{collision.transform.name} has entered the {roomName}");
        }
    }
    void OnTriggerExit(Collider collision)
    {
        if (collision.transform.TryGetComponent<NPCWorker>(out NPCWorker npc))
        {
            RemoveWorker(npc);
            Debug.Log($"{collision.transform.name} has exited the {roomName}");
        }
    }

    public void BroadcastMessageToWorkers(string message)
    {
        Debug.Log($"Broadcasting message to workers in {roomName}: {message}");
        foreach (var worker in workersInRoom)
        {
            worker.OnSignal(message);
        }
    }
    public void BroadcastMessageToWorkersCD(string message)
    {
        Debug.Log($"Broadcasting message to workers in {roomName}: {message}");
        foreach (var worker in workersInRoom)
        {
            worker.GetSpeak(message);
        }
    }
}