
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortByY : MonoBehaviour {
    SpriteRenderer sr;
    void Awake() { sr = GetComponent<SpriteRenderer>(); }
    void LateUpdate() {
        sr.sortingOrder = -(int)(transform.position.y * 100);
    }
}
