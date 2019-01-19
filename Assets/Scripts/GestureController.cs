using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GestureController : MonoBehaviour
{
    public void Ping()
    {
        Debug.Log("Ping");
    }

    public void Vibrate() {
        Handheld.Vibrate();
    }
}
