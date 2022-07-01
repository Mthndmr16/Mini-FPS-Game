using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float attackRange = 2f;  // sald�rma mesafesi
    [SerializeField] private float chaseRange = 5f;   // kovalama mesafesi
    [SerializeField] private float turnSpeed = 15f;  // yapay zekan�n d�nme h�z�
    [SerializeField] private float patrolRadius = 5f; // yapay zekam�z�n rastgele devriye gezece�i dairenin yar��ap�n� art�rmak i�in gerekli de�i�ken
    [SerializeField] private float patrolWaitTime = 2f; // yapay zekam�z�n rastgele olarak gitti�i yerde bir s�re beklemesi i�in gereken de�i�ken
    [SerializeField] private float chaseSpeed = 4f;  // yapay zekan�n kovalarken sahip oldu�u h�z. 
    [SerializeField] private float searchSpeed = 3.5f; // yapay zekan�n ararken sahip oldu�u h�z
    [Header("Attack Settings")]
    [SerializeField] int damage = 2;  // Enemy hasar� (en altta public metod kullan�larak Hit adl� scripte aktar�l�yor.)
    [SerializeField] float enemyAttackRate = 2f; // Enemy'nin sald�rma h�z�.


    private Animator anim;
    private NavMeshAgent navMeshAgent;
    private Transform player;

    private bool isSearched = false;
    private bool isAttacking = false;  // Enemy'nin attacklar� aras�nda s�re olaca�� i�in Coroutine metodunda kullanmak ama�� burada bu de�i�keni olu�turduk

    enum State  // yapay zekam�za enum class'�n� kullanarak durum at�yoruz. k�saca k�t�phanedeki raflara alttaki durumlar� koyuyoruz diyebiliriz.
    {
        Idle,    // Oyuna ilk ba�land��� anda olmas�n� istedi�im durum.
        Search,  // E�er �evresinde oyuncu yoksa olmas�n� istedi�im durum.
        Chase,   // Oyuncu belli bir alana girince olmas�n� istedi�im durum.
        Attack   // Oyuncu �ok yak�nla�t���nda olmas�n� istedi�im durum.
    }

    [SerializeField] private State currentState = State.Idle;  // currentState de�i�kenimizi sabit bit de�ere atam�� olduk. Bu de�i�keni Inspector k�sm�ndan da de�i�tirebiliriz.

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        StateCheck();
        StateExecute();
    }

    private void OnDrawGizmos()  // Edit�r �al��t��� s�rece �al���r
    {        
        Gizmos.color = Color.red;   // Gizmosun rengini belirler
        Gizmos.DrawWireSphere(transform.position, chaseRange);  // Gizmosun alan�n� belirler. yapay zekam�z�n oyuncuyu kovalamas� i�in girmesi gereken mesafe.
        switch (currentState)
        {        
            case State.Search:
                Gizmos.color = Color.green;
                Vector3 targetPosition = new Vector3(navMeshAgent.destination.x, transform.position.y, navMeshAgent.destination.z);
                // vekt�r�m�z zemine s�f�r bir �ekilde �izilecek yani �apraz gitmeyecek. y a��s� hep 1 (��nk� enemy'nin y pozisyonu 1) di�erleri de hedef pozisyonun x ve z degerleri.

                Gizmos.DrawLine(transform.position, targetPosition);
                break;
            case State.Chase:
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, player.position);
                break;
            case State.Attack:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
                break;
        }
    }

    private void StateCheck()
    {
        float distanceToTarget = Vector3.Distance(player.position, transform.position);   // Oyuncuya olan uzakl���m�z. 
        // buradaki distanceToTarget de�i�kenini kullanabilmek i�in yukar�da de�er atamam�z gerek. (attachRange , chaseRange)

        if (distanceToTarget < chaseRange && distanceToTarget > attackRange)
        {
            currentState = State.Chase;  // kovala
        }
        else if (distanceToTarget <= attackRange)
        {
            currentState = State.Attack; // sald�r
        }
        else
        {
            currentState = State.Search;  // ara 
        }




    }

    private void StateExecute()   // Buradaki k�s�m sadece bizim statelerimizin kullan�laca�� zaman �al��acak .
                                  // Statelerimizizi de update metodunun i�inde StateCheck(); metodu kullanarak kendimiz belirleyece�iz.
    {
        switch (currentState)
        {
            case State.Idle:
                print("Idle");
                break;
            case State.Search:
                // if(bir kere �al��mas� i�in bunu yaz�yoruz (bir kez aramam��sak) && benim yapay zekam hedef noktaya vard�ysa demeye yarayan kod)
                if (!isSearched && navMeshAgent.remainingDistance <= 0.1f || !navMeshAgent.hasPath && !isSearched)
                //  navMeshAgent.remainingDistance : yapay zekan�n kendi pozisyonuyla hedefe olan mesafesinden kalan pozisyon.
                //  !navMeshAgent.hasPath : yapay zekan�n gidecek yeri yoksa.
                {
                    // Olur da remainingDistance 0.1 den b�y�k olursa ; yapay zekam�z�n pozisyonunu hedef konuma e�itleyece�iz.
                    Vector3 navMeshAgentTarget = new Vector3(navMeshAgent.destination.x, transform.position.y, navMeshAgent.destination.z);  
                    // vekt�r�m�z zemine s�f�r bir �ekilde �izilecek yani �apraz gitmeyecek. y a��s� hep 1 (��nk� enemy'nin y pozisyonu 1) di�erleri de hedef pozisyonun x ve z degerleri.

                    navMeshAgent.enabled = false;
                    transform.position = navMeshAgentTarget;
                    navMeshAgent.enabled = true;

                    // �imdi de bizim bu searh i�lemini k�sa aral�klarla yapmam�z laz�m.
                    Invoke(nameof(Search), patrolWaitTime);
                    anim.SetBool("Walk", false);
                    isSearched = true;  // ara
                }
                break;
            case State.Chase:               
                Chase();
                break;
            case State.Attack:
               // print("Attack");
                Attack();
                break;
            default:
                break;
        }
    }

    // Bu metotta yapay zekam�za , �uraya gideceksin komutunu verecek olan kod yaz�lacak.
    private void Search()
    {
        navMeshAgent.isStopped = false;
        isSearched = false;
        navMeshAgent.SetDestination(GetRandomPosition()); // Gidilecek yeri random olarak belirlemek i�in yaz�lan kod.
        navMeshAgent.speed = searchSpeed;
        anim.SetBool("Walk", true); 
        //   print("Search");

    }

    private void Attack()
    {
        if (player == null)
        {
            return;
        }
        if (!isAttacking)  // E�er enemy sald�rm�yorsa
        {
            StartCoroutine(AttackRoutine());  // Coroutine'yi �a��r.
        }
        anim.SetBool("Walk", false); 
        navMeshAgent.velocity = Vector3.zero; // Yapay zekam�z sald�rmaya ba�lad��� an h�z�n� birden kesiyoruz. araban�n tekerlerini birden s�km���z gibi.
        navMeshAgent.isStopped = true;  // hareketi kesip d��mana sald�r�yor. ama burada belli bir yava�lama s�reci oluyor. drift atar gibi
        LookTheTarget(player.position);
    }

    private void Chase()
    {
        navMeshAgent.isStopped = false;  // kovalama mekanizmas� i�in d��man�n hareket etmesi gerekiyor. o y�zden false

        if (player == null)  // oyuncumuz yok oldugunda nullReferance hatas� verece�i i�in burada bu kodu yaz�yoruz.
        {
            return;
        }
        navMeshAgent.SetDestination(player.position); // oyuncumuzu takip etmesi i�in gereken kod.
        navMeshAgent.speed = chaseSpeed;
        anim.SetBool("Walk", true);
        //print("Chase");
    }

    private IEnumerator AttackRoutine()  // Ayn� attackController adl� scriptte yapt���m�z gibi burada da d��man�n sald�r� h�z�n� dengelemek amac�yla bir Coroutine yapaca��z
    {
        isAttacking = true;
        yield return new WaitForSeconds(enemyAttackRate);
        anim.SetTrigger("Attack");
        yield return new WaitUntil(IsAttackAnimationFinished);  // bu sat�r�n anlam�: bana gelen fonksiyon(altta yazd���m�z) true ise i�lemi devam ettirip alt sat�ra ge�irir 
        isAttacking = false;
    }

    private bool IsAttackAnimationFinished()  // Yukar�daki Coroutine'da AttackController scriptinde yaptu��m gibi 'isAttacking = false' dersem , sald�r� animasyonu daha bitmeden
                                              //  tekrar bir coroutine �a��rm�� olaca��m i�in burada animasyonun bitip bitmedi�ini kontrol etmek ama�l� bir metod d�nd�rebilirim.
    {
        //Burada da yine Hit scriptindeki update metodunda dedi�imiz gibi Enemy objesinin sahip oldu�u animatorun 0. layer'�nda (base layerda)
        // herhangi bir ge�i� yoksa (Yani herhangi bir ata�a ge�miyorsak) && animatorde bana 0. layerdaki ("Attack") hakk�nda bir bilgi al.(Yani Atack ad�ndaki animasyonda m�y�z)
        // && bu animasyonun %95'i tamamland�ysa.
        if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") 
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            return true;  // Attack animasyonunu bitir
        }
        else
        {
            return false;
        }

    }
   
    // NavMeshAgent kullan�rken genelde kulland���m�z objenin d�nmesine yard�mc� oluruz ��nk� bu mekanik dipdibe gelindi�inde tam �al��m�yor.
    private void LookTheTarget(Vector3 target)
    {
        Vector3 lookPosition = new Vector3(target.x, transform.position.y, target.z); // hedefin x ve z pozisyonunu almam�z�n sebebi ona sa�-sol ve ileri- geri olarak bakmak istememiz.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPosition - transform.position)
        , turnSpeed * Time.deltaTime);
    }

    // Bu kod sayesinde(Devriye mekani�i) etraf�m�zda random bir nokta se�ip o noktay� Vector3 olarak d�nd�rebiliyoruz.
    private Vector3 GetRandomPosition() 
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        //Burada yaz�lan �ns�deUnitSphere adl� komut , bir daire olu�turur ve daire i�inde rastgele bir konum se�er. Ve o konumu bize Vector3 olarak d�ner. 

        randomDirection += transform.position;
        // olu�turdu�umuz randomDirection vekt�r�n� kendi pozisyonumuzla toplayaca��z ki dairenin merkezinde bizim yapay zekam�z olsun.

        NavMeshHit hit;
        //NavMesh �zerinden o alan�n m�sait olup olmad���n� bilmemiz laz�m. yani yapay zekam�z�n her yere �zg�rce gidebilme durumu yok. 

        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
        // O noktan�n NavMesh �zerinde gidilebilirli�ini kontrol etmek i�in yaz�lan kod.
        // kodda �unu diyoruz : randomDirection noktas�n� kontrol et , ��kt�s�n� bana hit olarak ver (e�er gidilebiliyorsa o pozisyonun ��kt�s�n�
        // ; gidilemiyorsa o pozisyona yak�n bir pozisyonun ��kt�s�n� verecek) , bunun i�in bir mesafe belirle , katman belirle (ya default olarak 1 ya da NavMesh.AllAreas).

        return hit.position;

    }

    public int GetDamageEnemy()  // Enemy hasar�n� d�nd�ren PUBLIC bir metod.
    {
        return damage;
    }
}
