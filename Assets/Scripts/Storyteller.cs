using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;


public class Storyteller : MonoBehaviour
{
    public static Storyteller Instance { get; private set; }

    public float askInterval = 5f;

    private int currentIndex = 0;
    private Coroutine askLoop;

    #region Singleton
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
    #endregion

    void Start()
    {
        askLoop = StartCoroutine(AskWorkersLoop());
    }

    IEnumerator AskWorkersLoop()
    {
        while (true)
        {
            IReadOnlyList<NPCWorker> workers = NPCWorker.All;
            if (workers.Count > 0)
            {
                if (currentIndex >= workers.Count) currentIndex = 0;

                NPCWorker npc = workers[currentIndex];
                currentIndex++;

                if (npc != null)
                    yield return AskWorkerWhatDoing(npc);
            }

            yield return new WaitForSeconds(askInterval);
        }
    }

    IEnumerator AskWorkerWhatDoing(NPCWorker npc)
    {
        string questionEn = "What are you doing?";

        

        npc.OnSignal(questionEn);

        yield return null;
    }
}
