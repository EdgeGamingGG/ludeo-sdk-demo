using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10;
    public int Damage = 10;
    private Vector3 _delta;
    Vector3 lastFramePosition = Vector3.zero;
    Vector3 delta = Vector3.zero;

    public void Init(Transform target)
    {
        _delta = target.position - transform.position;
    }

    private void Update()
    {
        Follow();
        //delta = transform.position - lastFramePosition;
        //Debug.DrawLine(transform.position, transform.position + delta.normalized, Color.red, 0f);
        //lastFramePosition = transform.position;

        //if (delta == Vector3.zero || lastFramePosition == Vector3.zero)
        //    delta = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f));

        //transform.Translate(delta.normalized * Time.deltaTime * speed, Space.World);

    }

    private void Follow()
    {
        transform.Translate(_delta.normalized * Time.deltaTime * speed, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(Damage);
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
