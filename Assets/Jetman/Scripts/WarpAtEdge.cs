using DG.Tweening;
using UnityEngine;

public class WarpAtEdge : MonoBehaviour {
    void Start() {
    }

    void Update() {
    }

    void OnTriggerEnter2D(Collider2D collider) {
        var xd = transform.position.x < 0 ? 45 : -45;
        collider.transform.position += new Vector3(xd, 0, 0);

        // Restart any particle systems after teleport or they display random artefacts over the screen
        var pe = collider.GetComponentInChildren<ParticleSystem>();
        if (pe != null) {
            pe.Stop();
            pe.Play();
        }
    }
}

