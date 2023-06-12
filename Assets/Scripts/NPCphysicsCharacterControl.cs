
using System.Collections;

using UnityEngine;

using TMPro;

using UnityEngine.AI;
using UnityEngine.Serialization;

public class NPCphysicsCharacterControl : MonoBehaviour
{
    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
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
    
    //NPC Variables
    private NavMeshAgent m_Agent;
    
    void Start()
    {
        startPos = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        //Set Jump
        jump = new Vector3(0.0f, jumpHeight, 0.0f);
        m_Speed = m_WalkSpeed;
        
        //NPC
        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.updateRotation = false;
        m_Agent.updatePosition = false;
    }
    private void Update()
    {

        if (canMove)
        {
            //Tastatureingabe entfernt, falls NPC laufen will, lauf
            if (m_Agent.desiredVelocity.magnitude > 0 && isGrounded == true) //Wenn Input != 0, dann bewege
            {
                MoveCharacter();
                m_Agent.nextPosition = m_Rigidbody.position;
            }
            
        }
        else
        {
            m_Rigidbody.velocity = pushDir * pushForce;
        }
        //Geschummelt ;) /m_Speed rausgenommen damit Figur besser laeuft
        m_Animator.SetFloat("Speed", m_Rigidbody.velocity.magnitude);
        
        CheckIdle();
    }



    private void FixedUpdate()
    {
        if(transform.position.y < -.5)
        {
            ResetPosition();
        }
    }

    
    //*** Collision Detection
    void OnCollisionStay(){
        isGrounded = true;
        m_Animator.SetBool("Grounded", isGrounded);
    }
    void OnCollisionEnter(Collision collision){
        isGrounded = true;
        m_Animator.SetBool("Grounded", isGrounded);
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
        m_Animator.SetBool("Grounded", isGrounded);
    }
    private void CheckPlatform(Collision collision)
    {
        if(collision.gameObject.tag == "Platform"){
            TipToePlatform tipToePlatform = collision.gameObject.GetComponent<TipToePlatform>();
            tipToePlatform.CharacterTouches();
            isGrounded = true;
            m_Animator.SetBool("Grounded", isGrounded);
        }
    }

    //*** Movement
    private void MoveCharacter()
    {
        // Tastureingabe entfernt
        // Vector3 m_Input = Camera.main.transform.forward * Input.GetAxis("Vertical") +
        //                   Camera.main.transform.right * Input.GetAxis("Horizontal");
        
        //NPC Einghabe
        Vector3 m_Input = m_Agent.desiredVelocity;

        m_Input.y = 0;  //y = 0, damit Höhe nicht mit einberechnet wird
            m_Input.Normalize(); // normalisieren für länge = 1; konstante Geschwindigkeit
            

            // Move per force //Forcemode entfernt
            m_Rigidbody.AddForce(m_Speed * m_Input);

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
            
            //Jump entfernt
            // if(Input.GetKeyDown(KeyCode.Space) && isGrounded){
            //     Jump();
            // }
    }
    
    private void Jump()
    {
        m_Rigidbody.AddForce(jump * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        m_Animator.SetBool("Grounded", isGrounded);
    }
    
    private void CheckIdle()
    {
        if (m_Rigidbody.velocity.magnitude <= 0.1f)
        {
            stillTime += Time.deltaTime;
            if (stillTime >= startIdle)
            {
                m_Animator.SetBool("Idle", true);
            }
        }
        else
        {
            stillTime = 0f;
            m_Animator.SetBool("Idle", false);
        }
    }

    private void ResetPosition()
    {
        transform.position = m_Agent.nextPosition;
        // transform.position = startPos;

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



