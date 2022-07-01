using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // mouse inputunu kullanabilmemiz için bu kütüphaneyi çaðýrýyoruz

public class AttackController : MonoBehaviour
{
    // Bu scriptimizi saldýrý iþlemleri için kullanacaðýz

    [SerializeField] Weapon currentWeapon;  // Tek silah kullanmayacaðýmýz için bu þekilde bir deðiþken oluþturduk. Böylelikle istediðimiz silahý inspectordan sürükleyebiliriz.
    private Transform mainCamera;
  

    private Animator anim;

    private bool isAttacking = false; // bu þekilde bir bool oluþturuyoruz. AttackRoutine() kýsmýna geldiðinde true olacak ve böylelikle Coroutine 2 kez çaðýrýlmamýþ olacak. 
                                      // 2 kez çaðýrýlýrsa animasyonla üstüste oynar.bu da istenmeyen birþey
       
    private void Awake()
    {
        mainCamera = GameObject.FindWithTag("CameraPoint").transform;  // ilk önce player içindeki Camera pointi referans alýyoruz.
        anim = mainCamera.transform.GetChild(0).GetComponent<Animator>();  // Camrera pointin ilk cocupuna git (WeaponRoot) ve ondan Animator Componentini getir.
        if (currentWeapon != null)  // Oluþabilecek bir bug'ý önlemek için 
        {
            SpawnWeapon();
        }
        
    }

    void Update()
    {
        Attack();
    }

    private void Attack()
    {
        if (Mouse.current.leftButton.isPressed && !isAttacking) // Mouse sol click basýlý tuttuðumuz sürece ve saldýrmýyorsak
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void SpawnWeapon()
    {
        if (currentWeapon == null)
        {
            return;
        }
        currentWeapon.SpawnNewWeapon(mainCamera.GetChild(0).GetChild(0),anim);
    }

    public void EquipWeapon(Weapon weaponType)  // Weapon classý alacak (yani scriptable object)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Drop();  // currentWeapon içindeki Drop() kýsmýna git ve aktif olan silahýmýzý yok et.    
        }
        currentWeapon = weaponType;  // current weaponumu yeni bir eapon ile deðiþtirdim
        SpawnWeapon();  // ve yokettiðim silah yerine yenisini spawn edeceðim.
    }

     private IEnumerator AttackRoutine()
    {
        // burada amacýmýz silahýmýza bir saldýrý hýzý vermek

        isAttacking = true;  // burasý true olduðu gibi Attack() metodundaki if çalýþmayacak.
        anim.SetTrigger("Attack"); // Player controller kýsmýnda Run ve Walk ile yaptýðýmýzýn aynýsý fakat burada trigger kullandýðýmýz için bu þekilde yazýlýyor.
        yield return new WaitForSeconds(currentWeapon.GetAttackRate);  // burada da Weapon scriptinde oluþturduðumuz property'yi buraya çekip GetAttackRate deðerine kaç verdiysek
                                                                       // o kadar süre beklemesini saðlýyoruz
        isAttacking = false;  // boolu tekrar false yapýyoruz. böylece yukarýdaki Attack() metodu tekrar kullanýlabilir oluyor.
       
    }

    public int GetDamagePlayer()  // Player hasarýný kontrol eden PUBLIC bir Metod.
    {
        if (currentWeapon != null)  // Eðer elimizde silah varsa
        {
            return currentWeapon.GetDamage;  // 
        }
        return 0;
    }
}
