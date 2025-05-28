using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;



public class RedirectCollisions : MonoBehaviour
{
    public event Action<Collider> TriggerEnter;
    public event Action<Collider> TriggerExit;
    public event Action<Collision> CollisionEnter;
    public event Action<Collision> CollisionExit;
    public event Action<Collider2D> TriggerEnter2D;
    public event Action<Collider2D> TriggerExit2D;
    public event Action<Collision2D> CollisionEnter2D;
    public event Action<Collision2D> CollisionExit2D;
    public event Action OnTagretStatusChange;

    public UnityEvent<Transform> ObjectEnter;

    public List<Transform> list = new List<Transform>();


    public string tagretTag = "Player";
    bool inside;
    bool insideChange
    {
        get
        {
            return inside;
        }
        set
        {
            inside = value;
            OnTagretStatusChange?.Invoke();
        }
    }
    public bool TagretInside
    {
        get
        {
            return inside;
        }
    }

    void Check(bool a, string b)
    {
        if (b == tagretTag)
            insideChange = a;
    }

    void AddToList(Transform a)
    {
        CheckDefects();
        if(!list.Contains(a))
        {
            list.Add(a);
            ObjectEnter?.Invoke(a);
        }
    }
    public void CheckDefects()
    {
        // Iterate backward to safely remove items without affecting indices
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null || list[i].transform == null)
            {
                list.RemoveAt(i); // Remove defective item
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        TriggerEnter?.Invoke(collision);
        Check(true, collision.tag);
        AddToList(collision.transform);

    }
    private void OnTriggerExit(Collider collision)
    {
        TriggerExit?.Invoke(collision);
        Check(false, collision.tag);
        list.Remove(collision.transform);
    }
    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
        Check(true, collision.transform.tag);
        AddToList(collision.transform);
    }
    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke(collision);
        Check(false, collision.transform.tag);
        list.Remove(collision.transform);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnter2D?.Invoke(collision);
        Check(true, collision.tag);
        AddToList(collision.transform);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        TriggerExit2D?.Invoke(collision);
        Check(false, collision.tag);
        list.Remove(collision.transform);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnter2D?.Invoke(collision);
        Check(true, collision.transform.tag);
        AddToList(collision.transform);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionExit2D?.Invoke(collision);
        Check(false, collision.transform.tag);
        list.Remove(collision.transform);
    }
}
