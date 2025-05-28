using System.Collections;
using UnityEngine;

/// <summary>
/// dont mind this one, didnt even test it
/// </summary>
[Tooltip("ignore this")]
public class AnimatorSpeedUpdate : MonoBehaviour
{
    [SerializeField]
    Animator anim;


    void Update()
    {
        anim.speed = TimeManager.TimeCalculation(1);
    }
}