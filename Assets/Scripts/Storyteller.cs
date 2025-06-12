using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Storyteller : MonoBehaviour
{
    public static Storyteller Instance { get; private set; }

    [Tooltip("Storyteller ile karakter sorusu arasındaki bekleme süresi")]
    public float askDelay = 1.0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void OnWorkerEnteredRoom(Room room, NPCWorker worker)
    {
        StartCoroutine(HandleWorkerEntry(room, worker));
    }


    IEnumerator HandleWorkerEntry(Room room, NPCWorker worker)
    {
        yield return SendToAPIAndMaybeSpeak(
            character: "Storyteller",
            prompt: $"{worker.npcName} has entered the {room.roomName}."
        );

        yield return new WaitForSeconds(askDelay);

        string question = $"{worker.npcName}, ne yapıyorsun?";
        room.BroadcastMessageToWorkers(question);
        worker.OnSignal("What are you doing?");
    }


    IEnumerator SendToAPIAndMaybeSpeak(string character, string prompt)
    {
        string url = $"http://localhost:8000/{character}?prompt={UnityWebRequest.EscapeURL(prompt)}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                yield break;

            string cleaned = System.Text.RegularExpressions.Regex.Unescape(www.downloadHandler.text.Trim('"'));
            JObject jobj = JObject.Parse(cleaned);

            string action = jobj["action"]?.Value<string>();
            if (!string.IsNullOrEmpty(action) && action.Contains("<Speak>"))
            {
                string spoken = action.Replace("<Speak>", "").Replace("</Speak>", "");
                Debug.Log($"Storyteller says: {spoken}");
                foreach (Room room in FindObjectsOfType<Room>())
                    room.BroadcastMessageToWorkers(spoken);
            }
        }
    }
}
