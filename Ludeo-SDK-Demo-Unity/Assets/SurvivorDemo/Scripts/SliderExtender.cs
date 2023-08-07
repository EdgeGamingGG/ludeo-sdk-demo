using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderExtender : MonoBehaviour
{
    Slider _slider;
    Func<float> _binder;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (_binder != null)
            _slider.value = _binder();
    }

    public void Bind(Func<float> value)
    {
        _binder = value;
    }
}
