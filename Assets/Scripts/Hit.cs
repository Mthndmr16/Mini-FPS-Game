using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]  // Hit scriptini bir objeye eklersek bu iki sat�rdaki BoxCollider ve RigidBody componentleri bu scriptle beraber otomatik olarak eklenecek.
[RequireComponent(typeof(Rigidbody))]
public class Hit : MonoBehaviour
{

    private Transform owner;
    private int damage;
    private Collider hitCollider;  // Ontrigger metodunu kullanabilmek i�in bu Collider'a ihtiyac�m�z olacak.
    
    private Rigidbody rigidbody;

    private Animator anim;


    private void Awake()
    {
        owner = transform.root;  // bu scripte sahip olan objeyi , bu scrriptin tak�l� oldu�u objenin en b�y�k parent'�na ata. 
                                // yani bu script hand objesine eklenecek ama transform.root komutu sayesinde hand'�n en �stteki parent'�na ula�aca��z

        hitCollider = GetComponent<BoxCollider>(); 
        rigidbody = GetComponent<Rigidbody>();
        // referans al�mlar� i�in �stteki iki sat�r� kulland�m

        

        hitCollider.isTrigger = true; 
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        // Yukar�daki 3 sat�rdaki i�lemleri inspector penceresinden de yapabilirdik ama buradan da bu �ekilde yap�labiliyor.

        hitCollider.enabled = false; // hasar verece�imiz zaman a�aca��z.
        
    }


    private void Start()
    {
        if (owner.tag == "Player")  // E�er hasar verenin Tag'i Player ise ;
        {
           damage = owner.GetComponent<AttackController>().GetDamagePlayer(); // hasar verenin sahibine git. ondan AttackController componentini (script) �ek.
                                                                              // ve bana i�ingeki GetDamagePlayer() metodunu d�nd�r.

           anim = GetComponentInParent<Transform>().GetComponentInParent<Animator>(); // Biz �uan hand'�n i�ineyiz yani parentimiz hand.
                                                                                      // Hand objesinin Transformunu , WeaponRoot objesinin de Animatorunu bana getir.
        }
        else if (owner.tag == "Enemy")
        {
           damage =  owner.GetComponent<EnemyController>().GetDamageEnemy(); // hasar verenin sahibine git. ondan EnemyController componentini (script) �ek.
                                                                             // ve bana i�ingeki GetDamageEnemy() metodunu d�nd�r.

            anim = GetComponentInParent<Animator>();  // Bu script ayn� zamanda Enemy objesinin i�indeki HitBox objesinin i�inde oldu�u i�in bu bir �stteki parent'a gidip oradan
                                                      // animator'u referans al�yoruz. ��nk� biz animasyon i�lemlerini Enemy'de yapt�k. 
        }
        else
        {
            enabled = false;  // e�er enemy veya player de�il ise kendini kapacatak. bu da console da hata almam�z� �nleyecek.
        }
    }

    void Update()
    {
        // Yukar�da awake metodunda Weapon root'un referans�n� alm��t�k. Burada da ��yle bir�ey diyoruz. Weapoon root'un sahip oldu�u animatorun 0. layer'�nda (base layerda)
        // herhangi bir ge�i� yoksa (Yani herhangi bit ata�a ge�miyorsak) && animatorde bana 0. layerdaki ("Attack") hakk�nda bir bilgi al.(Yani Atack ad�ndaki animasyonda m�y�z) &&
        // animasyonumuz 0.5f 'den b�y�kse && animasyonumuz 0.55f' den k���kse. (animasyon hareketimizin tamamina 1 dersek e�er)
        if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && 
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.55f)
        {
            ControlTheCollider(true);
            print("Hit!");
        }
        else
        {
            ControlTheCollider(false);
        }
    }
      
    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();  // temas etti�imiz objenin Health scriptini getir. Burada onu referans al.
        if (health != null && health.gameObject != owner.gameObject)  // �arpt���m�z nesnede health scripti varsa && ve health scriptine sahip obje owner(biz) de�il ise
        {
            health.GiveDamage(damage);
           
        }
    }

    private void ControlTheCollider(bool open)  // Update metodundaki karaktere vurma animasyonumuzda Collider'� a��p kapamaya yarayacak bool bir metod.
    {
        hitCollider.enabled = open;  // Yani collider a��ld���nda bize 'Open' olarak d�necek.
    }
}
