using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SimpleCharacterControl : MonoBehaviour
{
    private Rigidbody m_Rigidbody;
    private Animator anim;
    private Vector3 startPos;
    public TextMeshProUGUI textElement;
    public Text timerText;
    private float timer;


    //SphereCast Variablen
    private Vector3 origin;
    private float originHeight = 2f; //Höhe der Kugel
    private float radius = 0.5f; //Radius der Kugel, die geschossen wird (Lücken beachten zwischen den Platformen)
    private float castDistance = 2f; //Wie weit die Kugel in die Richtung geschossen wird

    [Range(0, 10)] public float m_speed = 5f;
    [Range(0, 10)] public float fallSpeed = 10f;


    void Start()
    {
        //Setzt Startposition für Reset Funktion
        startPos = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        textElement.text = "";

    }
    void FixedUpdate()
    {
        //Timer 
        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer % 60F);
        int milliseconds = Mathf.FloorToInt((timer * 100F) % 100F);
        timerText.text = minutes.ToString ("00") + ":" + seconds.ToString ("00") + ":" + milliseconds.ToString("00");

        
        
        //Vektor für Bewegung in Bezug auf Kamera
        Vector3 m_Input = Camera.main.transform.forward * Input.GetAxis("Vertical") +
                          Camera.main.transform.right * Input.GetAxis("Horizontal");
       
        m_Input.y = 0;  //y = 0, damit Höhe nicht mit einberechnet wird
        m_Input.Normalize(); // normalisieren für länge = 1; konstante Geschwindigkeit
        
        // https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html
        m_Rigidbody.MovePosition(Time.deltaTime * m_speed * m_Input + transform.position);

        // https://docs.unity3d.com/ScriptReference/Rigidbody.MoveRotation.html
        // Wenn Bewegung in x oder z Richtung, dann rotiere
        if (m_Input.sqrMagnitude > 0) //sqrtMagnitude == Betrag vom Vektor == Länge
        {
            // https://docs.unity3d.com/ScriptReference/Quaternion.html 
            m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Input)); //Quaternion.LookRotation = Rotation von m_Input // Quaternion (von lateinisch quaternio ‚Vierheit') steht für: in der Mathematik ein Zahlbereich, der häufig für die Darstellung von Drehungen verwendet wird,
        }
        
        // rigidbody.velocity.magnitude = Geschwindigkeit des Rigidbodys, muss mit m_speed zurückgerechnet werden
        anim.SetFloat("Speed", m_Rigidbody.velocity.magnitude / m_speed);
        
        // Wenn Shift Taste gedrückt soll m_speed * 2
        if (Input.GetKey(KeyCode.LeftShift))
        {
            m_speed = 10f;
        }
        else
        {
            m_speed = 5f;
        }
        
          
        //Falls Fallguy aus dem Level fällt wird er zurückgesetzt
        if(transform.position.y < -5)
        {
            ResetPosition();
        }

        // Kollisionsabfrage mit SphereCast
        RaycastHit hit;
        origin = transform.position + new Vector3(0, originHeight, 0); // Höhe Startpunkt der Kugel
        if(Physics.SphereCast(origin, radius, Vector3.down, out hit, castDistance)) //Schiesst eine Kugel nach Unten
        {
            // Falls Kugel auf Objekt trifft, ist Figur grounded
            Debug.Log("Grounded auf " + hit.collider.gameObject.name + " " + transform.position);
            anim.SetBool("Grounded", true);

            
            if(hit.collider.gameObject.name == "GoalPlatform")
            {
                //Bei Erreichen der Plattform wird der Text angezeigt und nach 2 Sekunden zurückgesetzt
                textElement.text = "You Win!";
                StartCoroutine(waiter());
   
            }
            
            if(hit.collider.gameObject.tag == "Platform")
            {
                TipToePlatform tipToePlatform = hit.collider.gameObject.GetComponent<TipToePlatform>();
                // tipToePlatform.isPath = true;
                tipToePlatform.CharacterTouches();
            } 
        }
        else
        {
            anim.SetBool("Grounded", false);
            Debug.Log("Falling");
            m_Rigidbody.MovePosition(Time.deltaTime * fallSpeed * Vector3.down + transform.position);
        }
    }
    
    //Warten https://stackoverflow.com/questions/30056471/how-to-make-the-script-wait-sleep-in-a-simple-way-in-unity
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(2);
        textElement.text = "3";
        yield return new WaitForSeconds(1);
        textElement.text = "2";
        yield return new WaitForSeconds(1);
        textElement.text = "1";
        yield return new WaitForSeconds(1);
        textElement.text = "";
        timer += Time.deltaTime;
        ResetPosition();
    }
    
    //Setzt Fallguy zurück
    private void ResetPosition()
    {
        transform.position = startPos;
    }
}
    

