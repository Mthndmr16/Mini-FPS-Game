using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 19. scriptableObjects / 20. silah�m�z� olu�tural�m

// bunu ben edit�rden olu�turmak i�in (yani �o�altmak i�in) , edit�re ula��p bunu yapman� istiyorum dememiz laz�m o y�zden de b�yle bir komutumuz var.
[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon", order = 0)]

public class Weapon : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] GameObject weaponPrefab;
    [SerializeField] AnimatorOverrideController animatorOverride ;
   
    [SerializeField] int damage;
    [SerializeField] float attackRate;
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] Vector3 scaleOffset = Vector3.zero;
    

    private GameObject weaponClone; // olu�turdu�umuz (Instantiate etti�imiz) silah� burada tutaca��z.

    // �stteki de�erler private oldu�u i�in onlara bir property yapmam�z laz�m 
    public GameObject GetWeaponPrefab { get { return weaponPrefab; } }
    public int GetDamage { get { return damage; } }
    public float GetAttackRate { get { return attackRate; } }

    public void SpawnNewWeapon(Transform parent ,Animator anim)  // Hangi objenin i�inde olu�aca��n� bilmesi i�in (parentinin kim oldu�unu bilmesi i�in) yaz�lan kod.
                // Ayn� zamanda hangi animator'u kontrol edece�imi bilmedi�imden bunu spawnlad���m�z esnada isteyelim.
                // Hangi animatoru kontrol edece�imi bilmedi�imden , bunu spawnlad���m�z esnada isyeyelim diye ,Animator anim diye bir de�i�ken olu�turdum.
    {
        if (weaponPrefab != null)
        {
            weaponClone = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity, parent);
            weaponClone.transform.position = parent.position;  // objemizin pozisyonu
            weaponClone.transform.rotation = parent.rotation; // objemizin rotasyonu
            weaponClone.transform.localScale = weaponClone.transform.localScale + scaleOffset; // objemizin �l��leri (b�y�kl�k k���kl�k)
            weaponClone.transform.localPosition = Vector3.zero + positionOffset;  // silah�m�z�n elimizde birazc�k sa�a veya sola d�nmesini sa�lamak i�in yaz�lan kod.  
        }
        if (animatorOverride != null)
        {
            anim.runtimeAnimatorController = animatorOverride;  // animasyonu, oyun esnas�nda �al��an AnimatorControl�ne kilitliyorum ve benim animatorOverride'm� bu k�sma yerle�tiriyorum.
                                                                // yani �rnek olarak Player > Animator > controller k�sm�.
        }
            
    }

    public void Drop()
    {
        Destroy(weaponClone);
    }
}
