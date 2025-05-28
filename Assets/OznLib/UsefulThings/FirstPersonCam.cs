using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// HOW TO USE
/// put this on you camera, add speed or something
/// 
/// i forgot jump and running, 
/// when i made it before it was not important
/// </summary>
/// made it when i started, learned things from a tutorial, added walking sound thing and etc
/// 
[DisallowMultipleComponent]
public class FirstPersonCam : MonoBehaviour
{
    public bool isWorking = true;

    Vector3 move;
    Vector2 mov;
    float[] mouse = new float[2];
    public float sensivitivity = 1.5f;
    public float speed = 3;
    float rotation = 0f;
    [Tooltip("Even if you dont add a player it will make a new one, so dont worry, tho please do add player")]
    public GameObject Player;
    private Rigidbody PlayerRB;
    Vector3 velocityDown = Vector3.zero;
    float lookpos;

    /// <summary>
    /// it takes audio source from player you can put step sound there
    /// </summary>
    /// 
    AudioSource sound;
    [SerializeField]
    float step_time;
    float time_step;
    float speedofstep;
    bool stopped;

    public Vector3 Position = new Vector3(0,0.75f,0);

    private void Awake()
    {
        TimeObject.VibeCheck();
        if(Player == null)
        {
            Player = new GameObject("Player");
            Player.transform.position = transform.position;
        }



        Cursor.lockState = CursorLockMode.Locked;
        if(!Player.TryGetComponent<Rigidbody>(out PlayerRB))
        {
            PlayerRB = Player.AddComponent<Rigidbody>();
            PlayerRB.constraints = RigidbodyConstraints.FreezeRotation;
            Player.AddComponent<CapsuleCollider>();
        }
        sound = Player.GetComponent<AudioSource>();
    }

    void Update()
    {
        if(!stopped && move.magnitude == 0)
        {
            stopped = true;
        }
        if (stopped && move.magnitude > 0)
        {
            stopped = false;
            time_step = TimeObject.time;
        }
        if (isWorking)
        {
            CameraCalculate();
        }
        MoveCalculate();
        
    }
    private void FixedUpdate()
    {
        //this code is very old, dont judge me


        velocityDown.y = PlayerRB.velocity.y;

        PlayerRB.velocity = velocityDown + move * speed;
        speedofstep = PlayerRB.velocity.magnitude / 3f;
        if (sound != null && move.magnitude > 0f && TimeObject.time > time_step + (step_time / speedofstep))
        {
            sound.Play();
            time_step = TimeObject.time;
        }

    }
    private void LateUpdate()
    {
        if (isWorking)
        {
            CameraRotation();
        }
    }

    Vector2 MovCalculate()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void MoveCalculate()
    {
        mov = isWorking ? MovCalculate() : Vector2.zero;
        move = Player.transform.right * mov.x + Player.transform.forward * mov.y;
        move = Vector3.ClampMagnitude(move, 1);
    }

    void CameraCalculate()
    {
        if(lookpos > 180 || lookpos < -180)
        {
            lookpos = lookpos < 0 ? -180: 180;
            lookpos *= -1;
        }
        var calculated = sensivitivity;
        mouse[0] = Input.GetAxis("Mouse X") * calculated;
        mouse[1] = Input.GetAxis("Mouse Y") * calculated;

        rotation -= mouse[1];
        rotation = Mathf.Clamp(rotation, -90, 90);
        lookpos += mouse[0];

        

        //player rotation
        
    }

    void CameraRotation()
    {
        transform.localRotation = Quaternion.Euler(rotation, lookpos, 0f);
        Player.transform.rotation = Quaternion.Euler(0,lookpos,0);
        transform.position = Player.transform.position + Position;
    }
}
