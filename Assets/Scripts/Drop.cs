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
        if (weaponToDrop != null)   // Yani droplayacak bir silah�m�z varsa.
        {
            Instantiate(weaponToDrop.GetWeaponPrefab, transform.position, transform.rotation, transform);
        }
    }

    private void Update()
    {
        transform.Rotate(angle, Space.World);
    }

    private void OnTriggerStay(Collider other)  // oyuncu trigger� a��k bir boxCollider'�n i�inde kald��� s�rece �al��acak
    {
        if (other.gameObject.tag == "Player")
        {
            if (Keyboard.current.gKey.wasPressedThisFrame) // g tu�una 1 kez bast�ysak
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
