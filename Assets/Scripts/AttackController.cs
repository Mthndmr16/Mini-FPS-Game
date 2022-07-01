using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // mouse inputunu kullanabilmemiz i�in bu k�t�phaneyi �a��r�yoruz

public class AttackController : MonoBehaviour
{
    // Bu scriptimizi sald�r� i�lemleri i�in kullanaca��z

    [SerializeField] Weapon currentWeapon;  // Tek silah kullanmayaca��m�z i�in bu �ekilde bir de�i�ken olu�turduk. B�ylelikle istedi�imiz silah� inspectordan s�r�kleyebiliriz.
    private Transform mainCamera;
  

    private Animator anim;

    private bool isAttacking = false; // bu �ekilde bir bool olu�turuyoruz. AttackRoutine() k�sm�na geldi�inde true olacak ve b�ylelikle Coroutine 2 kez �a��r�lmam�� olacak. 
                                      // 2 kez �a��r�l�rsa animasyonla �st�ste oynar.bu da istenmeyen bir�ey
       
    private void Awake()
    {
        mainCamera = GameObject.FindWithTag("CameraPoint").transform;  // ilk �nce player i�indeki Camera pointi referans al�yoruz.
        anim = mainCamera.transform.GetChild(0).GetComponent<Animator>();  // Camrera pointin ilk cocupuna git (WeaponRoot) ve ondan Animator Componentini getir.
        if (currentWeapon != null)  // Olu�abilecek bir bug'� �nlemek i�in 
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
        if (Mouse.current.leftButton.isPressed && !isAttacking) // Mouse sol click bas�l� tuttu�umuz s�rece ve sald�rm�yorsak
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

    public void EquipWeapon(Weapon weaponType)  // Weapon class� alacak (yani scriptable object)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Drop();  // currentWeapon i�indeki Drop() k�sm�na git ve aktif olan silah�m�z� yok et.    
        }
        currentWeapon = weaponType;  // current weaponumu yeni bir eapon ile de�i�tirdim
        SpawnWeapon();  // ve yoketti�im silah yerine yenisini spawn edece�im.
    }

     private IEnumerator AttackRoutine()
    {
        // burada amac�m�z silah�m�za bir sald�r� h�z� vermek

        isAttacking = true;  // buras� true oldu�u gibi Attack() metodundaki if �al��mayacak.
        anim.SetTrigger("Attack"); // Player controller k�sm�nda Run ve Walk ile yapt���m�z�n ayn�s� fakat burada trigger kulland���m�z i�in bu �ekilde yaz�l�yor.
        yield return new WaitForSeconds(currentWeapon.GetAttackRate);  // burada da Weapon scriptinde olu�turdu�umuz property'yi buraya �ekip GetAttackRate de�erine ka� verdiysek
                                                                       // o kadar s�re beklemesini sa�l�yoruz
        isAttacking = false;  // boolu tekrar false yap�yoruz. b�ylece yukar�daki Attack() metodu tekrar kullan�labilir oluyor.
       
    }

    public int GetDamagePlayer()  // Player hasar�n� kontrol eden PUBLIC bir Metod.
    {
        if (currentWeapon != null)  // E�er elimizde silah varsa
        {
            return currentWeapon.GetDamage;  // 
        }
        return 0;
    }
}
