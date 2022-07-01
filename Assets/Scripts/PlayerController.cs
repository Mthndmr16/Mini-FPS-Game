using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // yeni input sistemini packege managerdan indirdik , burada da kütüphaneyi kullanýyoruz.


public class PlayerController : MonoBehaviour
{
    [Header("Player Control Settings")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float gravityModifier = 0.95f;
    [SerializeField] private float jumpPower = 0.25f;
    [SerializeField] private InputAction newMovementInput;  // yeni input sistemini bir deðiþken olarak atamamýz gerekiyor.
                                                            // bu newMovementInput deðiþkenini objemiz sahneye geldiðinde aktive etmemiz, objemiz sahneden yok olduðunda da deaktive etmemiz gerek(OnEnable ve OnDisable metodlarý kullanýlýr.) 

    [Header("Mouse Sensivity Settings")]
    [SerializeField] private float mouseSensivity = 1f;  // fare hassasiyeti
    [SerializeField] private float maxViewAngle = 90f;   // yukarýya doðru maksimum bakýþ açýmýz.
    [SerializeField] bool invertX;  //  Sað-sol ve yukarý-aþaðý eksenleri için ters yön deðiþkenleri.
    [SerializeField] bool invertY;


    // buraya bütün audioClipleri listelemek için bu þekilde baþlýk oluþturup tek tek bütün audio cliplerimizi yazýyoruz.
    [Header("Sound Settings")]
    [SerializeField] private List<AudioClip> footStepSounds = new List<AudioClip>();   // footstep tek olmadýðý için bütün footStepsleri bir liste halinde yazýyoruz.
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;


    // Yukarý açýda olan vektörümüzü ayrý bir þekilde tutmak istiyoruz çünkü oyunlarda çoðunlukla gökyüzü etkisi deðiþebilir
    // (mesela pervane açtýðýmýzý düþünürsek havada z ekseninde bile hareket edebilir) bu yüzden bunu input ile karýþtýrmamak için baþka bir vektör oluþturacaðýz.
    private Vector3 heightMovement;  // havadaki hareketimizi kontrol edecek olan vektör.


    private bool jump = false;

    private CharacterController characterController;  // içerisinde rigidbody , collider vs olan toplu bir karakter hareket sistemi.

    private Animator anim; // animatoru referans almamýz gerek çünkü oluþturduðumuz 'Walk' animasyonunu animatorda tutuyoruz.

    private AudioSource audioSource;   // AudioSource componentini referans almamýz gerek çünkü animation event ve diðer ses iþlemlerini bunu referenas alarak yapabiliriz

    private int lastIndex = -1;

    private bool landSoundPlayed = true;  // yere indiðimizde sesin iki defa çýkmasýný engellemek için


    private float currentSpeed = 8f;
    private float horizontalInput;
    private float verticalInput;

    // Player içinde oluþturmuþ olduðumuz CameraPoint objesini Main Camera olarak referans almamýz gerek çünkü yukarý aþaðý hareket etmesi gerek.
    private Transform mainCamera;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (Camera.main.GetComponent<CameraController>() == null)   // Main Cameranýn içinde CameraController adlý component yok ise   (Güvenlik amaçlý yazýlmýþ bir kýsým)
        {
            Camera.main.gameObject.AddComponent<CameraController>();  // o scripti bulup Main Cameranýn içine ekle.
        }
        mainCamera = GameObject.FindWithTag("CameraPoint").transform;
        audioSource.Stop();
    }

    private void OnEnable()   // obje aktif olunca çalýþýr.
    {
        newMovementInput.Enable();  // eðer bu adýmý yapmazsak input sistemimiz çalýþmaz
    }

    private void OnDisable()  // obje yok olunca çalýþýr.
    {
        newMovementInput.Disable(); // eðer bu adýmý yapmazsak input sistemimiz çalýþmaz 
    }

    void Update()
    {
        KeyboardInput();

        AnimationChanger();
    }



    private void FixedUpdate()
    {
        Move();
        Rotate();
    }



    private void Move()
    {
        // zýplama kodu
        if (jump)
        {
            heightMovement.y = jumpPower;
            jump = false;   // zýpladýktan sonra havada tekrar tekrar zýplamasýn diye sadece 1 kez çalýþacak. o yüzden false 
        }

        // yer çekimi kodu
        heightMovement.y -= gravityModifier * Time.deltaTime;


        Vector3 localVerticalVector = transform.forward * verticalInput;  // Baktýðýmýz yönün vertical input vektörü.
        Vector3 localHorizontalVector = transform.right * horizontalInput;  // Baktýðýmýz yönün horizontal input vektörü.

        Vector3 movementVector = localHorizontalVector + localVerticalVector;

        movementVector.Normalize();
        movementVector *= currentSpeed * Time.deltaTime;

        characterController.Move(movementVector + heightMovement);   // hareket vektörü ile yerçekimi vektörünü topladýk 


        // eðer karakterimiz yere deðiyorsa   heightMovement.y -= gravityModifier * Time.deltaTime;   kodunu yok sayacak. bu þekilde de boþu boþuna bir kuvvet uygulamamýþ olacak.
        if (characterController.isGrounded)
        {
            heightMovement.y = 0f;

            if (!landSoundPlayed)     // buradaki kod   
            {
                audioSource.PlayOneShot(landSound);
                landSoundPlayed = true;
            }
        }
    }

    private void Rotate()  // Oyuncuyu çevirme   
    {

        // saða sola çevirme 
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + MouseInput().x, transform.eulerAngles.z);
        // rotation yaparken transform.eulerAngles.y saða ve solu gösterir. z ise cismi saðdan sola döndürür (parande pozisyonu).x ise yukarýdan aþaðý döndürür(takla)


        // yukarý aþaðý cevirme        
        if (mainCamera != null) // Eger (burada main camera deðiþkeniyle kullandýðýmýz) CameraPoint boþ deðil ise.
        {

          //  print(mainCamera.rotation.eulerAngles.x);
            if (mainCamera.rotation.eulerAngles.x > maxViewAngle && mainCamera.rotation.eulerAngles.x < 180f)   // her tarafa istediðimiz þekilde dönemememiz için bu satýr gerekli
            {
                mainCamera.rotation = Quaternion.Euler(maxViewAngle, mainCamera.rotation.eulerAngles.y, mainCamera.rotation.eulerAngles.z);   // yukarýdaki ifade saðlanýrsa x açýmýzý maxViewAngle açýsýna aþitleyecek. 
                                                                                                                                              // bu þekilde her tarafa istenildiði gibi dönülmesi engellenmiþ olunacak.
            }
            else if (mainCamera.rotation.eulerAngles.x > 180f && mainCamera.rotation.eulerAngles.x < 360f - maxViewAngle)
            {
                mainCamera.rotation = Quaternion.Euler(360f - maxViewAngle, mainCamera.rotation.eulerAngles.y, mainCamera.rotation.eulerAngles.z);
            }
            else
            {
                mainCamera.rotation = Quaternion.Euler(mainCamera.rotation.eulerAngles
               + new Vector3(-MouseInput().y, 0f, 0f)); // MouseInput().y inputunu vector3'ün x eksenine koyduk çünkü yukarý aþaðý hareket etmesini istiyoruz.
                                                        // burada Quaternion.Euler metodunun farkli bir uygulanma þeklini ele aldýk. Açýlarý tek tek atamak yerine iki farklý açýyý topladýk.
                                                        // yukarýda yazýlan kodda MouseInput().y kýsmýnýn baþýna - koymazsak verdiðimiz derecenin en sonunda (90) baþlar yan, karakter yukarý bakarken oyun baþlar.
            }
        }

    }


    private void AnimationChanger()   // Karakterimizin animasyonunu kontrol ettiðimiz bölüm.
    {
        if (newMovementInput.ReadValue<Vector2>().magnitude > 0f && characterController.isGrounded)  // Eðer yürüyorsak ve karakterimiz zýplamýyorsa animasyonumuzu oynat  
        {
            if (currentSpeed == walkSpeed)   // yürüyorsak
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Run", false);
            }
            else if (currentSpeed == runSpeed) // koþuyorsak
            {
                anim.SetBool("Run", true);
                anim.SetBool("Walk", false);
            }
        }
        else                                   // yerimizde duruyorsak 
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }

    }


    private void KeyboardInput()
    {
        #region eskiInputKodu 
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");
        #endregion

        // YeniInputKodu (Sadece yeni input sistemiyle kullanýlabilir.)
        horizontalInput = newMovementInput.ReadValue<Vector2>().x; // Klavyeden a ve d tuþlarýnda basýp karakterimizi saða sola hareket ettirmemizi saðlayan kod.
        verticalInput = newMovementInput.ReadValue<Vector2>().y;   // Klavyeden w ve s tuþlarýnda basýp karakterimizi ileri geri hareket ettirmemizi saðlayan kod .

        #region eski space basma kodu
        //if (Input.GetKey(KeyCode.Space) && characterController.isGrounded) // space tuþuna basýlýrsa ve CharacterController componentimiz yere temas ediyorsa.
        //{
        //    jump = true;
        //}
        #endregion

        // Yeni space basma kodu
        if (Keyboard.current.spaceKey.wasPressedThisFrame && characterController.isGrounded) //  ve CharacterController componentimiz yere temas ediyorsa.
                                                                                             // þuan takýlý olan keyboarda git ve space key'i getir ve tuþa bastýðýmýz anda 1 defa input al. (wasPressedThisFrame = bu kare esnasýnda tuþa 1 kez basýldý mý)
        {
            jump = true;
            landSoundPlayed = false;
            audioSource.PlayOneShot(jumpSound);

        }
        #region eski koþma kodu
        //if (Input.GetKey(KeyCode.LeftShift))
        //{
        //    currentSpeed = runSpeed;
        //}
        //else
        //{
        //    currentSpeed = walkSpeed;
        //}
        #endregion

        // Yeni koþma kodu
        if (Keyboard.current.leftShiftKey.isPressed)  // isPressed = Tuþa basýlý tuttuðumuz sürece. 
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

    }


    private void PlayFootStepSound()   // animation event kýsmýndan þeçilebilecek bir metot oluþturuyoruz. Bu metot ayak sesi çýkartmamýzý saðlayacak.
    { // tek yapmamýz gereken burada doðru sesi oynatmak. 

        if (footStepSounds.Count > 0 && audioSource != null)  // atanmýþ bir ayak sesi klibimiz var ise ve audioSource'umuz null deðil ise;
        {
            int index;  // indexi buraya atadýðýmýzda, burdaki metot çaðýrýldýðý zaman son tutulan index her zaman otomatik olarak 0'a eþitleniyor.
                        // biz de yukarýda bir lastIndex deðiþkeni belirteceðiz çünkü son tutulan indexin 0 a eþit olmasýný istemiyoruz.

            do
            {

                index = UnityEngine.Random.Range(0, footStepSounds.Count);
                if (lastIndex != index)
                {
                    if (!audioSource.isPlaying)  // audioSource herhangi bir ses çalmýyorsa alttaki kod çalýþacak. bu sayede ayný anda iki ses üstüste binmemiþ olacak
                    {
                        audioSource.PlayOneShot(footStepSounds[index]);
                        lastIndex = index;
                        break;
                    }
                }
            }
            while (index == lastIndex);   // buradaki döngüyü yapma amacýmýz, her adým attýðýmýzda farklý sesin çýkmadýðýndan emin olmak. çünkü 4 adet footSteps sesi var
        }
    }


    private Vector2 MouseInput()
    {
        #region Eski Mouse Input kodu
        //return new Vector2(invertX ? -Input.GetAxisRaw("Mouse X") : Input.GetAxisRaw("Mouse X"),  //Mouse X : Horizontal (saða sola gittiði) açý  Mouse Y : Vertical(yukarý aþaðý gittiði) açý
        //    invertY ? -Input.GetAxisRaw("Mouse Y") : Input.GetAxisRaw("Mouse Y")) * mouseSensivity;
        #endregion

        #region if Input ile yazýlmýþ eski Mouse Input Kodu
        //Vector2 mouseÝnput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        //if (invertX)
        //{
        //    mouseÝnput.x = -mouseÝnput.x;
        //}
        //if (invertY)
        //{
        //    mouseÝnput.y = -mouseÝnput.y;
        //}
        //return mouseÝnput;
        #endregion

        // Yeni Mouse Input kodu
        return new Vector2(invertX ? -Mouse.current.delta.x.ReadValue() : Mouse.current.delta.x.ReadValue(),    //mouse.current.x : Horizontal (saða sola gittiði) açý. 
            invertY ? -Mouse.current.delta.y.ReadValue() : Mouse.current.delta.y.ReadValue()) * mouseSensivity; //  mouse.current.y : Vertical(yukarý aþaðý gittiði) açý.
    }
}
