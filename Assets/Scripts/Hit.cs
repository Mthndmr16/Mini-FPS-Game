using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]  // Hit scriptini bir objeye eklersek bu iki satýrdaki BoxCollider ve RigidBody componentleri bu scriptle beraber otomatik olarak eklenecek.
[RequireComponent(typeof(Rigidbody))]
public class Hit : MonoBehaviour
{

    private Transform owner;
    private int damage;
    private Collider hitCollider;  // Ontrigger metodunu kullanabilmek için bu Collider'a ihtiyacýmýz olacak.
    
    private Rigidbody rigidbody;

    private Animator anim;


    private void Awake()
    {
        owner = transform.root;  // bu scripte sahip olan objeyi , bu scrriptin takýlý olduðu objenin en büyük parent'ýna ata. 
                                // yani bu script hand objesine eklenecek ama transform.root komutu sayesinde hand'ýn en üstteki parent'ýna ulaþacaðýz

        hitCollider = GetComponent<BoxCollider>(); 
        rigidbody = GetComponent<Rigidbody>();
        // referans alýmlarý için üstteki iki satýrý kullandým

        

        hitCollider.isTrigger = true; 
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        // Yukarýdaki 3 satýrdaki iþlemleri inspector penceresinden de yapabilirdik ama buradan da bu þekilde yapýlabiliyor.

        hitCollider.enabled = false; // hasar vereceðimiz zaman açacaðýz.
        
    }


    private void Start()
    {
        if (owner.tag == "Player")  // Eðer hasar verenin Tag'i Player ise ;
        {
           damage = owner.GetComponent<AttackController>().GetDamagePlayer(); // hasar verenin sahibine git. ondan AttackController componentini (script) çek.
                                                                              // ve bana içingeki GetDamagePlayer() metodunu döndür.

           anim = GetComponentInParent<Transform>().GetComponentInParent<Animator>(); // Biz þuan hand'ýn içineyiz yani parentimiz hand.
                                                                                      // Hand objesinin Transformunu , WeaponRoot objesinin de Animatorunu bana getir.
        }
        else if (owner.tag == "Enemy")
        {
           damage =  owner.GetComponent<EnemyController>().GetDamageEnemy(); // hasar verenin sahibine git. ondan EnemyController componentini (script) çek.
                                                                             // ve bana içingeki GetDamageEnemy() metodunu döndür.

            anim = GetComponentInParent<Animator>();  // Bu script ayný zamanda Enemy objesinin içindeki HitBox objesinin içinde olduðu için bu bir üstteki parent'a gidip oradan
                                                      // animator'u referans alýyoruz. çünkü biz animasyon iþlemlerini Enemy'de yaptýk. 
        }
        else
        {
            enabled = false;  // eðer enemy veya player deðil ise kendini kapacatak. bu da console da hata almamýzý önleyecek.
        }
    }

    void Update()
    {
        // Yukarýda awake metodunda Weapon root'un referansýný almýþtýk. Burada da þöyle birþey diyoruz. Weapoon root'un sahip olduðu animatorun 0. layer'ýnda (base layerda)
        // herhangi bir geçiþ yoksa (Yani herhangi bit ataða geçmiyorsak) && animatorde bana 0. layerdaki ("Attack") hakkýnda bir bilgi al.(Yani Atack adýndaki animasyonda mýyýz) &&
        // animasyonumuz 0.5f 'den büyükse && animasyonumuz 0.55f' den küçükse. (animasyon hareketimizin tamamina 1 dersek eðer)
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
        Health health = other.GetComponent<Health>();  // temas ettiðimiz objenin Health scriptini getir. Burada onu referans al.
        if (health != null && health.gameObject != owner.gameObject)  // çarptýðýmýz nesnede health scripti varsa && ve health scriptine sahip obje owner(biz) deðil ise
        {
            health.GiveDamage(damage);
           
        }
    }

    private void ControlTheCollider(bool open)  // Update metodundaki karaktere vurma animasyonumuzda Collider'ý açýp kapamaya yarayacak bool bir metod.
    {
        hitCollider.enabled = open;  // Yani collider açýldýðýnda bize 'Open' olarak dönecek.
    }
}
