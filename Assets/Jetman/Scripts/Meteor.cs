using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour {
    GameController gameController;
    Rigidbody2D body;

	// Use this for initialization
	void Start () {
        gameController = GameObject.FindObjectOfType<GameController>();
        body = GetComponent<Rigidbody2D>();

        var vx = Random.Range(4f,6f);
        var vy = Random.Range(0.1f,1f);
        var va = -45;

        // Starting on left
        if (transform.position.x > 0) {
            vx = -vx;
            va = -va;
        }

        body.velocity = new Vector2(vx, -vy);
        body.angularVelocity = va;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Destroy meteor if it hits the bottom ground level
        if (collision.gameObject.layer == Layers.Ground) {
            if (transform.position.y < -8) {
                HitGround();
            }
        }
    }

    void HitGround() {
        gameController.EnemyDies(this.gameObject);
    }
}
