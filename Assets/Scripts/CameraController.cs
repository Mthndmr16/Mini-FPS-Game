using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // mouse sa�a sola kayd�r�rken karakter d�n�� yapacak, yukar� a�a�� kayd�r�rken CameraPoint tag�n� verdi�imiz obje yukar� a�a�� d�necek.

    private Transform target;   // hedefini takip etmesini istiyoruz. bu bir fps oyunu .
    private void Awake()

    {
        target = GameObject.FindWithTag("CameraPoint").transform;    // CameraPoint tag�n� verdi�imiz objemizin transformunu target adl� de�i�kinimize kopyalad�k.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;   // CameraPoint'in pozisyonunu target pozisyonuna kopyalad�k.
            transform.rotation = target.rotation;   // CameraPoint'in rotasyonunu target rotasyonuna kopyalad�k.
        }    
    }
}
