using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPCWorker : MonoBehaviour
{
    public float taskDuration = 3f;
    public float minDistanceToOther = 2f;

    private static List<NPCWorker> allNPCs = new List<NPCWorker>();

    private List<Transform> taskPoints = new List<Transform>();
    private Transform currentTarget;
    private NavMeshAgent agent;
    private bool isDoingTask = false;

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

        PickNewTarget();
    }

    void Update()
    {
        if (isDoingTask || currentTarget == null)
            return;

        foreach (NPCWorker other in allNPCs)
        {
            if (other == this) continue;
            if (other.currentTarget == currentTarget)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance < minDistanceToOther)
                {
                    PickNewTarget();
                    return;
                }
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
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
        isDoingTask = false;
        PickNewTarget();
    }

    void OnDestroy()
    {
        allNPCs.Remove(this);
    }
}
