using UnityEngine;
using UnityEngine.Tilemaps;

public class CorrosionGridController : MonoBehaviour
{
    public CorrosionTilemapController[] myTilemaps;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myTilemaps = GetComponentsInChildren<CorrosionTilemapController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
