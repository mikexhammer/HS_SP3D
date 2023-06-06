using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bounce : MonoBehaviour
{
	public float force = 10f; //Force 10000f
	public float stunTime = 0.5f;
	private Vector3 hitDir;
	
	[Header("Textelement will be filled")]
	public TextMeshPro textElement;

	private void Start()
	{
		textElement.text = "Force: " + force + "\nStunTime: " + stunTime;
	}

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			Debug.DrawRay(contact.point, contact.normal, Color.white);
			if (collision.gameObject.tag == "Player")
			{
				//hitDir = contact.point - transform.position;
				hitDir = contact.normal;
				// collision.gameObject.GetComponent<CharacterControls>().HitPlayer(-hitDir * force, stunTime);
				collision.gameObject.GetComponent<physicsCharacterControl>().HitPlayer(-hitDir * force, stunTime);
				return;
			}
		}
	}
}
