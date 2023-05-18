using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TipToeLogic : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    private int width = 10;
    private int depth = 13;
    private float distance = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 startPosition = transform.position;
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector3 position = new Vector3(startPosition.x + x * distance, 
                    startPosition.y, 
                    startPosition.z + z * distance);
                Instantiate(platformPrefab, position, Quaternion.identity);
                // Benoetigt Skript um an Platform zu kommen
                TipToePlatform tipToePlatform = platformPrefab.GetComponent<TipToePlatform>();
                float randFloat = Random.Range(0.0f,1.0f);
                bool isPath = (randFloat < 0.5f) ? true : false;
                tipToePlatform.isPath = isPath;
            }
        }
    }
    

    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
