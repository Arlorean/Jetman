using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util {
    public static void MakeDynamic(Collider2D collider) {
        var rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
        }
        collider.enabled = true;
    }
    public static void MakeKinematic(Collider2D collider) {
        var rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
        collider.enabled = false;
    }
}
