﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipToePlatform : MonoBehaviour
{
    public bool showPath;
    //Test for Path
    public Material testMaterial;
    enum State
    {
        Default,
        Touched,
        Dead,
    }

    State state = State.Default;
    MeshRenderer meshRend;
    BoxCollider bCollider;
    public bool isPath = false;
    public Material defaultMaterial;

    //Variables Touched State
    public Material touchedMaterial;
    float touchedTimer = 0.0f;
    public float maxTouchedTime = 5.0f;

    //Variables Dead State
    float deadTimer = 0.0f;
    public float maxDeadTime = 3.0f;

    void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        meshRend.material = defaultMaterial;
        bCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (state == State.Dead)
        {
            //Count down timer until respawn of platform
            deadTimer -= Time.deltaTime;
            meshRend.material.SetFloat("_Vanishing", Mathf.Max(0, deadTimer - maxDeadTime + 0.5f));
            //Debug.Log(Mathf.Max(0, deadTimer - 2));
            if (deadTimer <= 0.0f)
            {
                ChangeState(State.Default);
                deadTimer = 0.0f;
                //meshRend.enabled = true;
                bCollider.enabled = true;
                meshRend.material.SetFloat("_Vanishing", 1);
            }
        }
        if (state == State.Touched)
        {
            //Count down timer until reversion to unlit platform
            touchedTimer -= Time.deltaTime;
            if (touchedTimer <= 0.0f)
            {
                ChangeState(State.Default);
                touchedTimer = 0.0f;
                meshRend.material = defaultMaterial;
            }
        }
        
        if (showPath)
        {
            if (isPath)
            {
                meshRend.material = testMaterial;
            }
        }


    }

    private void ChangeState(State s)
    {
        state = s;
    }

    public void CharacterTouches()
    {
        if (!isPath)
        {
            ChangeState(State.Dead);
            deadTimer = maxDeadTime;
            //meshRend.enabled = false;
            bCollider.enabled = false;
        }
        else
        {
            if (state == State.Touched)
            {
                touchedTimer = maxTouchedTime;
            }
            else
            {
                ChangeState(State.Touched);
                touchedTimer = maxTouchedTime;
                meshRend.material = touchedMaterial;
            }
        }

    }
}
