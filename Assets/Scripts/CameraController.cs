using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // mouse saða sola kaydýrýrken karakter dönüþ yapacak, yukarý aþaðý kaydýrýrken CameraPoint tagýný verdiðimiz obje yukarý aþaðý dönecek.

    private Transform target;   // hedefini takip etmesini istiyoruz. bu bir fps oyunu .
    private void Awake()

    {
        target = GameObject.FindWithTag("CameraPoint").transform;    // CameraPoint tagýný verdiðimiz objemizin transformunu target adlý deðiþkinimize kopyaladýk.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;   // CameraPoint'in pozisyonunu target pozisyonuna kopyaladýk.
            transform.rotation = target.rotation;   // CameraPoint'in rotasyonunu target rotasyonuna kopyaladýk.
        }    
    }
}
