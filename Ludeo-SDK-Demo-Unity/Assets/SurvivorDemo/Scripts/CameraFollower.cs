using System;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    Transform _target;

    private void Update()
    {
        if (_target != null)
        {
            Follow();
        }
    }

    public void Init(Transform target)
    {
        _target = target;
    }

    private void Follow()
    {
        var d = _target.position - transform.position;
        d.y = 0;
        transform.Translate(d * Time.deltaTime, Space.World);
    }
}
