using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
public class TipToeLogic : MonoBehaviour
{
    [Header("Zeige den Path im Spiel an")]
    [SerializeField] public bool showPath = false;
    [SerializeField] private GameObject platformPrefab;
    //Anzahl der Platten auf dem Feld
    [Header("Anzahl der Platformen")]
    [SerializeField] private int width = 10;
    [SerializeField] private int depth = 13;

    //Abmasse des Feldes
    [Header("Abmasse des Feldes")]
    [SerializeField] private float areaWidth = 30;
    [SerializeField] private float areaDepth = 39;
    //Abstand zwischen den Platten
    [Header("Abstand zwischen den Platformen")]
    [SerializeField] private float spacing = 0.3f;

    private NavMeshSurface nms;
    
    void Start()
    {
        // Setze NavMeshSurface
        nms = GetComponent<NavMeshSurface>();
        
        // Berechnung der Grösse der Platten
        float platformWidth = (areaWidth - (width - 1) * spacing) / width;
        float platformDepth = (areaDepth - (depth - 1) * spacing) / depth;
        // Generierung des Pfades in Form einer Menge von Koordinaten (x,y)
        // https://docs.unity3d.com/Packages/com.unity.shadergraph@10.0/api/UnityEditor.ShaderGraph.Internal.KeywordDependentCollection.ISet-1.html
        HashSet<Vector2Int> path = GeneratePath(width, depth);

        for (int zIndex = 0; zIndex < depth; zIndex++)
        {
            for (int xIndex = 0; xIndex < width; xIndex++)
            {
                // Erzeugung der Platten in verschaltelter Schleife
                GameObject platform = Instantiate(platformPrefab, transform);
                platform.transform.localPosition = new Vector3(
                    -areaWidth / 2 + platformWidth / 2 + xIndex * (platformWidth + spacing),
                    0,
                    -areaDepth / 2 + platformDepth / 2 + zIndex * (platformDepth + spacing));
                platform.transform.localScale = new Vector3(platformWidth, 0.1f, platformDepth);
                TipToePlatform tipToePlatform = platform.GetComponent<TipToePlatform>();
                tipToePlatform.showPath = showPath;

                // Setzen des Pfades durch Überprüfung ob die Koordinate in der Menge enthalten ist
                //VectorInt2 ist ein selbstgeschriebener Datentyp, der zwei int-Werte enthält
                tipToePlatform.isPath = path.Contains(new Vector2Int(xIndex, zIndex));
                NavMeshModifier nmm = platform.GetComponent<NavMeshModifier>(); 
                nmm.overrideArea = true;
                if (tipToePlatform.isPath)
                {
                    nmm.area = 0;
                }
                else
                {
                    nmm.area = 1;
                }
            }
        }
        
        // Berechnung des NavMeshes
        nms.BuildNavMesh();
        
    }
    
    //Generierung des Pfades, gibt eine Menge von Koordinaten (x,y) zurück
    // HashSet ist eine Menge, welche keine doppelten Elemente enthalten kann
    // ISet ist ein Interface, welches von HashSet implementiert wird
    private static HashSet<Vector2Int> GeneratePath(int width, int depth)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        //Random.Range gibt eine zufällige Zahl zwischen min (inklusive) und max (exklusive) zurück
        int startX = Random.Range(0, width);
        //Falls die Generierung des Pfades fehlschlägt, wird der Pfad neu generiert
        //HasGoodNeighborhood gibt true zurück, wenn der Pfad erfolgreich generiert wurde
        if (!GenerateRecursive(width, depth, new Vector2Int(startX, 0), path)){
            Debug.LogError("Error generating path");
        }
        return path;
    }


  

    // Rekursive Funktion zur Generierung des Pfades, verwendet Backtracking 
    // https://en.wikipedia.org/wiki/Backtracking
    // wählt zufällig eine Richtung aus und prüft ob diese gültig ist
    // https://gamedev.stackexchange.com/questions/162915/creating-random-path-in-grid
    private static bool GenerateRecursive(int width, int depth, Vector2Int pos, HashSet<Vector2Int> path)
    {
        // Prüfung ob die Koordinate ausserhalb des Feldes liegt oder bereits im Pfad enthalten ist
        if (pos.x >= width || pos.x < 0 || pos.y >= depth || pos.y < 0 || path.Contains(pos))
        {
            return false;
        }
        // Prüfung ob die Koordinate am Ende des Pfades liegt
        if (pos.y == depth - 1)
        {
            path.Add(pos);
            return true;
        }
        // Prüfung ob die Koordinate mehr als einen Nachbarn im Pfad hat
        if (GetAppropriateNeighbors(pos, path) > 1)
        {
            return false;
        }
        // Hinzufügen der Koordinate zum Pfad, wenn die Prüfungen erfolgreich waren
        path.Add(pos);
        // Zufällige Reihenfolge der Richtungen
        Vector2Int[] toExplore = { pos + Vector2Int.up, pos + Vector2Int.down, pos + Vector2Int.left, pos + Vector2Int.right };
        // Fisher-Yates Shuffle
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        // https://stackoverflow.com/questions/273313/randomize-a-listt
        // https://stackoverflow.com/questions/5383498/shuffle-rearrange-randomly-a-liststring
        for(int i = toExplore.Length - 1; i > 0; i--)
        {
            int swapI = Random.Range(0, i);
            Vector2Int temp = toExplore[i];
            toExplore[i] = toExplore[swapI];
            toExplore[swapI] = temp;
        }
        
        // Rekursiver Aufruf für jede Richtung
        foreach(Vector2Int next in toExplore)
        {
            if (GenerateRecursive(width, depth,next, path))
            {
                return true;
            }
        }
        // Entfernen der Koordinate aus dem Pfad, wenn keine Richtung erfolgreich war
        path.Remove(pos);
        return false;
    }


    
    // Zählt die Anzahl Nachbarn einer Koordinate im Pfad
    // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iset-1?view=net-5.0
    private static int GetAppropriateNeighbors(Vector2Int pos, HashSet<Vector2Int> path)
    {
        int counter = 0;
        if (path.Contains(pos + Vector2Int.up))
        {
            counter++;
        }
        if (path.Contains(pos + Vector2Int.down))
        {
            counter++;
        }
        if (path.Contains(pos + Vector2Int.left))
        {
            counter++;
        }
        if (path.Contains(pos + Vector2Int.right))
        {
            counter++;
        }
        return counter;
    }
}
