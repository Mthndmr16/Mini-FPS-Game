using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class Drop : MonoBehaviour
{
    [SerializeField] Weapon weaponToDrop;
    [SerializeField] Vector3 angle = Vector3.zero;

    private BoxCollider dropBox;

    private void Awake()
    {
        dropBox = GetComponent<BoxCollider>();
        dropBox.isTrigger = true;
        dropBox.size *= 3;
    }

    private void Start()
    {
        if (weaponToDrop != null)   // Yani droplayacak bir silahýmýz varsa.
        {
            Instantiate(weaponToDrop.GetWeaponPrefab, transform.position, transform.rotation, transform);
        }
    }

    private void Update()
    {
        transform.Rotate(angle, Space.World);
    }

    private void OnTriggerStay(Collider other)  // oyuncu triggerý açýk bir boxCollider'ýn içinde kaldýðý sürece çalýþacak
    {
        if (other.gameObject.tag == "Player")
        {
            if (Keyboard.current.gKey.wasPressedThisFrame) // g tuþuna 1 kez bastýysak
            {
                if (weaponToDrop != null)
                {
                    other.GetComponent<AttackController>().EquipWeapon(weaponToDrop);
                }
                Destroy(gameObject);
            }
        }
    }
}
