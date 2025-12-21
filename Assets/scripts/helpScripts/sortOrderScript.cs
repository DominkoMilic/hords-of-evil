using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class sortOrderScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Awake(){
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate(){
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
