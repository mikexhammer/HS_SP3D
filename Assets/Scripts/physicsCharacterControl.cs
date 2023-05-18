using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class physicsCharacterControl : MonoBehaviour
{
    private Rigidbody m_Rigidbody;
    private Animator anim;
    private Vector3 startPos;
    public TextMeshPro textElement;

    [Header("Movement Settings")]
    public float m_WalkSpeed = 5f;
    public float m_RunSpeed = 10f;
    public bool canMove = true; //if player is not hitted
    private float m_Speed;
    
    // Jump Variables | Source: https://stackoverflow.com/questions/58377170/how-to-jump-in-unity-3d
    private Vector3 jump;
    [Header("Jump Settings")]
    
    public float jumpHeight = 2.0f;
    [Tooltip("The force of the jump.")]
    public float jumpForce = 10.0f;
    [Tooltip("If the player isnt grounded, he cant move.")]
    public bool isGrounded;
    
    //Idle Variables
    private float stillTime = 0f;
    [Header("Idle Settings")] 
    [Tooltip("Player starts to idle after x seconds.")]
    public float startIdle = 5f;
    
    //Ragdoll Variables
    private float pushForce;
    private Vector3 pushDir;
    

    void Start()
    {
        startPos = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        //Set Jump
        jump = new Vector3(0.0f, jumpHeight, 0.0f);
        m_Speed = m_WalkSpeed;
    }
    private void Update()
    {

        if (canMove)
        {
            if(Input.GetKeyDown(KeyCode.Space) && isGrounded){
                Jump();
            }
        
            if ((Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) && isGrounded == true) //Wenn Input != 0, dann bewege
            {
                MoveCharacter();
            }
            
        }
        else
        {
            m_Rigidbody.velocity = pushDir * pushForce;
        }
        
        anim.SetFloat("Speed", m_Rigidbody.velocity.magnitude / m_Speed);
        
        CheckIdle();
    }



    private void FixedUpdate()
    {
        if(transform.position.y < -5)
        {
            ResetPosition();
        }
    }

    
    //*** Collision Detection
    void OnCollisionStay(){
        isGrounded = true;
        anim.SetBool("Grounded", isGrounded);
    }
    void OnCollisionEnter(Collision collision){
        isGrounded = true;
        anim.SetBool("Grounded", isGrounded);
        CheckPlatform(collision);

        if (collision.gameObject.name == "GoalPlatform")
        {
            Debug.Log("GoalPlatform");
            textElement.text = "You Won!";
        }
    }
    void OnCollisionExit(Collision other)
    {
        isGrounded = false;
        anim.SetBool("Grounded", isGrounded);
    }
    private void CheckPlatform(Collision collision)
    {
        if(collision.gameObject.tag == "Platform"){
            TipToePlatform tipToePlatform = collision.gameObject.GetComponent<TipToePlatform>();
            tipToePlatform.CharacterTouches();
            isGrounded = true;
            anim.SetBool("Grounded", isGrounded);
        }
    }

    //*** Movement
    private void MoveCharacter()
    {
        Vector3 m_Input = Camera.main.transform.forward * Input.GetAxis("Vertical") +
                          Camera.main.transform.right * Input.GetAxis("Horizontal");
        
            m_Input.y = 0;  //y = 0, damit Höhe nicht mit einberechnet wird
            m_Input.Normalize(); // normalisieren für länge = 1; konstante Geschwindigkeit
        
           
            // Wenn Shift Taste gedrückt soll m_speed * 2
            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_Speed = m_RunSpeed;
            }
            else
            {
                m_Speed = m_WalkSpeed;
            }
            
            
            // Move per velocity
            // m_Rigidbody.velocity = m_Speed * m_Input;

            // Move per force
            m_Rigidbody.AddForce(m_Speed * m_Input, ForceMode.Acceleration);
            
            
            //Move per position
            // m_Rigidbody.MovePosition(Time.deltaTime * m_Speed * m_Input + transform.position);
            
            // https://docs.unity3d.com/ScriptReference/Rigidbody.MoveRotation.html
            // Wenn Bewegung in x oder z Richtung, dann rotiere
            if (m_Input.sqrMagnitude > 0) //sqrtMagnitude == Betrag vom Vektor == Länge
            {
                // https://docs.unity3d.com/ScriptReference/Quaternion.html 
                m_Rigidbody.MoveRotation(
                    Quaternion.LookRotation(
                        m_Input)); //Quaternion.LookRotation = Rotation von m_Input // Quaternion (von lateinisch quaternio ‚Vierheit') steht für: in der Mathematik ein Zahlbereich, der häufig für die Darstellung von Drehungen verwendet wird,
            }
            
            if(Input.GetKeyDown(KeyCode.Space) && isGrounded){
                Jump();
            }
            
            
    }
    
    private void Jump()
    {
        m_Rigidbody.AddForce(jump * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        anim.SetBool("Grounded", isGrounded);
    }
    
    private void CheckIdle()
    {
        if (m_Rigidbody.velocity.magnitude <= 0.1f)
        {
            stillTime += Time.deltaTime;
            if (stillTime >= startIdle)
            {
                anim.SetBool("Idle", true);
            }
        }
        else
        {
            stillTime = 0f;
            anim.SetBool("Idle", false);
        }
    }

    private void ResetPosition()
    {
        transform.position = startPos;
    }
    
    //*** Hit
    public void HitPlayer(Vector3 velocityF, float time)
    {
        //Speed of rigidbody
        m_Rigidbody.velocity = velocityF;
        //Speed value of rigidbody
        pushForce = velocityF.magnitude;
        //Direction of rigidbody
        pushDir = Vector3.Normalize(velocityF);
        //Start Coroutine to decrease speed
        StartCoroutine(DecreaseHit(velocityF.magnitude, time));
    }
    
    private IEnumerator DecreaseHit(float value, float duration)
    {
        RagdollMode(true);

        float delta = 0;
        delta = value / duration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            Debug.Log("canMove: " + canMove + " pushForce: " + pushForce);
            yield return null;
            pushForce = pushForce - Time.deltaTime * delta;
            pushForce = pushForce < 0 ? 0 : pushForce;
            m_Rigidbody.AddForce(new Vector3(0, -100 * GetComponent<Rigidbody>().mass, 0)); //Add gravity
        }
        
            RagdollMode(false);
        }

    private void RagdollMode(bool isOn)
    {
        canMove = !isOn;
        m_Rigidbody.freezeRotation = !isOn;
    }
    }


