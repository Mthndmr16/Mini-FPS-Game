using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float attackRange = 2f;  // saldýrma mesafesi
    [SerializeField] private float chaseRange = 5f;   // kovalama mesafesi
    [SerializeField] private float turnSpeed = 15f;  // yapay zekanýn dönme hýzý
    [SerializeField] private float patrolRadius = 5f; // yapay zekamýzýn rastgele devriye gezeceði dairenin yarýçapýný artýrmak için gerekli deðiþken
    [SerializeField] private float patrolWaitTime = 2f; // yapay zekamýzýn rastgele olarak gittiði yerde bir süre beklemesi için gereken deðiþken
    [SerializeField] private float chaseSpeed = 4f;  // yapay zekanýn kovalarken sahip olduðu hýz. 
    [SerializeField] private float searchSpeed = 3.5f; // yapay zekanýn ararken sahip olduðu hýz
    [Header("Attack Settings")]
    [SerializeField] int damage = 2;  // Enemy hasarý (en altta public metod kullanýlarak Hit adlý scripte aktarýlýyor.)
    [SerializeField] float enemyAttackRate = 2f; // Enemy'nin saldýrma hýzý.


    private Animator anim;
    private NavMeshAgent navMeshAgent;
    private Transform player;

    private bool isSearched = false;
    private bool isAttacking = false;  // Enemy'nin attacklarý arasýnda süre olacaðý için Coroutine metodunda kullanmak amaçý burada bu deðiþkeni oluþturduk

    enum State  // yapay zekamýza enum class'ýný kullanarak durum atýyoruz. kýsaca kütüphanedeki raflara alttaki durumlarý koyuyoruz diyebiliriz.
    {
        Idle,    // Oyuna ilk baþlandýðý anda olmasýný istediðim durum.
        Search,  // Eðer çevresinde oyuncu yoksa olmasýný istediðim durum.
        Chase,   // Oyuncu belli bir alana girince olmasýný istediðim durum.
        Attack   // Oyuncu çok yakýnlaþtýðýnda olmasýný istediðim durum.
    }

    [SerializeField] private State currentState = State.Idle;  // currentState deðiþkenimizi sabit bit deðere atamýþ olduk. Bu deðiþkeni Inspector kýsmýndan da deðiþtirebiliriz.

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

    private void OnDrawGizmos()  // Editör çalýþtýðý sürece çalýþýr
    {        
        Gizmos.color = Color.red;   // Gizmosun rengini belirler
        Gizmos.DrawWireSphere(transform.position, chaseRange);  // Gizmosun alanýný belirler. yapay zekamýzýn oyuncuyu kovalamasý için girmesi gereken mesafe.
        switch (currentState)
        {        
            case State.Search:
                Gizmos.color = Color.green;
                Vector3 targetPosition = new Vector3(navMeshAgent.destination.x, transform.position.y, navMeshAgent.destination.z);
                // vektörümüz zemine sýfýr bir þekilde çizilecek yani çapraz gitmeyecek. y açýsý hep 1 (çünkü enemy'nin y pozisyonu 1) diðerleri de hedef pozisyonun x ve z degerleri.

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
        float distanceToTarget = Vector3.Distance(player.position, transform.position);   // Oyuncuya olan uzaklýðýmýz. 
        // buradaki distanceToTarget deðiþkenini kullanabilmek için yukarýda deðer atamamýz gerek. (attachRange , chaseRange)

        if (distanceToTarget < chaseRange && distanceToTarget > attackRange)
        {
            currentState = State.Chase;  // kovala
        }
        else if (distanceToTarget <= attackRange)
        {
            currentState = State.Attack; // saldýr
        }
        else
        {
            currentState = State.Search;  // ara 
        }




    }

    private void StateExecute()   // Buradaki kýsým sadece bizim statelerimizin kullanýlacaðý zaman çalýþacak .
                                  // Statelerimizizi de update metodunun içinde StateCheck(); metodu kullanarak kendimiz belirleyeceðiz.
    {
        switch (currentState)
        {
            case State.Idle:
                print("Idle");
                break;
            case State.Search:
                // if(bir kere çalýþmasý için bunu yazýyoruz (bir kez aramamýþsak) && benim yapay zekam hedef noktaya vardýysa demeye yarayan kod)
                if (!isSearched && navMeshAgent.remainingDistance <= 0.1f || !navMeshAgent.hasPath && !isSearched)
                //  navMeshAgent.remainingDistance : yapay zekanýn kendi pozisyonuyla hedefe olan mesafesinden kalan pozisyon.
                //  !navMeshAgent.hasPath : yapay zekanýn gidecek yeri yoksa.
                {
                    // Olur da remainingDistance 0.1 den büyük olursa ; yapay zekamýzýn pozisyonunu hedef konuma eþitleyeceðiz.
                    Vector3 navMeshAgentTarget = new Vector3(navMeshAgent.destination.x, transform.position.y, navMeshAgent.destination.z);  
                    // vektörümüz zemine sýfýr bir þekilde çizilecek yani çapraz gitmeyecek. y açýsý hep 1 (çünkü enemy'nin y pozisyonu 1) diðerleri de hedef pozisyonun x ve z degerleri.

                    navMeshAgent.enabled = false;
                    transform.position = navMeshAgentTarget;
                    navMeshAgent.enabled = true;

                    // þimdi de bizim bu searh iþlemini kýsa aralýklarla yapmamýz lazým.
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

    // Bu metotta yapay zekamýza , þuraya gideceksin komutunu verecek olan kod yazýlacak.
    private void Search()
    {
        navMeshAgent.isStopped = false;
        isSearched = false;
        navMeshAgent.SetDestination(GetRandomPosition()); // Gidilecek yeri random olarak belirlemek için yazýlan kod.
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
        if (!isAttacking)  // Eðer enemy saldýrmýyorsa
        {
            StartCoroutine(AttackRoutine());  // Coroutine'yi çaðýr.
        }
        anim.SetBool("Walk", false); 
        navMeshAgent.velocity = Vector3.zero; // Yapay zekamýz saldýrmaya baþladýðý an hýzýný birden kesiyoruz. arabanýn tekerlerini birden sökmüþüz gibi.
        navMeshAgent.isStopped = true;  // hareketi kesip düþmana saldýrýyor. ama burada belli bir yavaþlama süreci oluyor. drift atar gibi
        LookTheTarget(player.position);
    }

    private void Chase()
    {
        navMeshAgent.isStopped = false;  // kovalama mekanizmasý için düþmanýn hareket etmesi gerekiyor. o yüzden false

        if (player == null)  // oyuncumuz yok oldugunda nullReferance hatasý vereceði için burada bu kodu yazýyoruz.
        {
            return;
        }
        navMeshAgent.SetDestination(player.position); // oyuncumuzu takip etmesi için gereken kod.
        navMeshAgent.speed = chaseSpeed;
        anim.SetBool("Walk", true);
        //print("Chase");
    }

    private IEnumerator AttackRoutine()  // Ayný attackController adlý scriptte yaptýðýmýz gibi burada da düþmanýn saldýrý hýzýný dengelemek amacýyla bir Coroutine yapacaðýz
    {
        isAttacking = true;
        yield return new WaitForSeconds(enemyAttackRate);
        anim.SetTrigger("Attack");
        yield return new WaitUntil(IsAttackAnimationFinished);  // bu satýrýn anlamý: bana gelen fonksiyon(altta yazdýðýmýz) true ise iþlemi devam ettirip alt satýra geçirir 
        isAttacking = false;
    }

    private bool IsAttackAnimationFinished()  // Yukarýdaki Coroutine'da AttackController scriptinde yaptuðým gibi 'isAttacking = false' dersem , saldýrý animasyonu daha bitmeden
                                              //  tekrar bir coroutine çaðýrmýþ olacaðým için burada animasyonun bitip bitmediðini kontrol etmek amaçlý bir metod döndürebilirim.
    {
        //Burada da yine Hit scriptindeki update metodunda dediðimiz gibi Enemy objesinin sahip olduðu animatorun 0. layer'ýnda (base layerda)
        // herhangi bir geçiþ yoksa (Yani herhangi bir ataða geçmiyorsak) && animatorde bana 0. layerdaki ("Attack") hakkýnda bir bilgi al.(Yani Atack adýndaki animasyonda mýyýz)
        // && bu animasyonun %95'i tamamlandýysa.
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
   
    // NavMeshAgent kullanýrken genelde kullandýðýmýz objenin dönmesine yardýmcý oluruz çünkü bu mekanik dipdibe gelindiðinde tam çalýþmýyor.
    private void LookTheTarget(Vector3 target)
    {
        Vector3 lookPosition = new Vector3(target.x, transform.position.y, target.z); // hedefin x ve z pozisyonunu almamýzýn sebebi ona sað-sol ve ileri- geri olarak bakmak istememiz.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPosition - transform.position)
        , turnSpeed * Time.deltaTime);
    }

    // Bu kod sayesinde(Devriye mekaniði) etrafýmýzda random bir nokta seçip o noktayý Vector3 olarak döndürebiliyoruz.
    private Vector3 GetRandomPosition() 
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        //Burada yazýlan ýnsýdeUnitSphere adlý komut , bir daire oluþturur ve daire içinde rastgele bir konum seçer. Ve o konumu bize Vector3 olarak döner. 

        randomDirection += transform.position;
        // oluþturduðumuz randomDirection vektörünü kendi pozisyonumuzla toplayacaðýz ki dairenin merkezinde bizim yapay zekamýz olsun.

        NavMeshHit hit;
        //NavMesh üzerinden o alanýn müsait olup olmadýðýný bilmemiz lazým. yani yapay zekamýzýn her yere özgürce gidebilme durumu yok. 

        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
        // O noktanýn NavMesh üzerinde gidilebilirliðini kontrol etmek için yazýlan kod.
        // kodda þunu diyoruz : randomDirection noktasýný kontrol et , çýktýsýný bana hit olarak ver (eðer gidilebiliyorsa o pozisyonun çýktýsýný
        // ; gidilemiyorsa o pozisyona yakýn bir pozisyonun çýktýsýný verecek) , bunun için bir mesafe belirle , katman belirle (ya default olarak 1 ya da NavMesh.AllAreas).

        return hit.position;

    }

    public int GetDamageEnemy()  // Enemy hasarýný döndüren PUBLIC bir metod.
    {
        return damage;
    }
}
