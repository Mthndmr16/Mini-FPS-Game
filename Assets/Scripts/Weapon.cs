using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 19. scriptableObjects / 20. silahýmýzý oluþturalým

// bunu ben editörden oluþturmak için (yani çoðaltmak için) , editöre ulaþýp bunu yapmaný istiyorum dememiz lazým o yüzden de böyle bir komutumuz var.
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
    

    private GameObject weaponClone; // oluþturduðumuz (Instantiate ettiðimiz) silahý burada tutacaðýz.

    // Üstteki deðerler private olduðu için onlara bir property yapmamýz lazým 
    public GameObject GetWeaponPrefab { get { return weaponPrefab; } }
    public int GetDamage { get { return damage; } }
    public float GetAttackRate { get { return attackRate; } }

    public void SpawnNewWeapon(Transform parent ,Animator anim)  // Hangi objenin içinde oluþacaðýný bilmesi için (parentinin kim olduðunu bilmesi için) yazýlan kod.
                // Ayný zamanda hangi animator'u kontrol edeceðimi bilmediðimden bunu spawnladýðýmýz esnada isteyelim.
                // Hangi animatoru kontrol edeceðimi bilmediðimden , bunu spawnladýðýmýz esnada isyeyelim diye ,Animator anim diye bir deðiþken oluþturdum.
    {
        if (weaponPrefab != null)
        {
            weaponClone = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity, parent);
            weaponClone.transform.position = parent.position;  // objemizin pozisyonu
            weaponClone.transform.rotation = parent.rotation; // objemizin rotasyonu
            weaponClone.transform.localScale = weaponClone.transform.localScale + scaleOffset; // objemizin ölçüleri (büyüklük küçüklük)
            weaponClone.transform.localPosition = Vector3.zero + positionOffset;  // silahýmýzýn elimizde birazcýk saða veya sola dönmesini saðlamak için yazýlan kod.  
        }
        if (animatorOverride != null)
        {
            anim.runtimeAnimatorController = animatorOverride;  // animasyonu, oyun esnasýnda çalýþan AnimatorControlüne kilitliyorum ve benim animatorOverride'mý bu kýsma yerleþtiriyorum.
                                                                // yani örnek olarak Player > Animator > controller kýsmý.
        }
            
    }

    public void Drop()
    {
        Destroy(weaponClone);
    }
}
