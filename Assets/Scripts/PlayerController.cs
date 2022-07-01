using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // yeni input sistemini packege managerdan indirdik , burada da k�t�phaneyi kullan�yoruz.


public class PlayerController : MonoBehaviour
{
    [Header("Player Control Settings")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float gravityModifier = 0.95f;
    [SerializeField] private float jumpPower = 0.25f;
    [SerializeField] private InputAction newMovementInput;  // yeni input sistemini bir de�i�ken olarak atamam�z gerekiyor.
                                                            // bu newMovementInput de�i�kenini objemiz sahneye geldi�inde aktive etmemiz, objemiz sahneden yok oldu�unda da deaktive etmemiz gerek(OnEnable ve OnDisable metodlar� kullan�l�r.) 

    [Header("Mouse Sensivity Settings")]
    [SerializeField] private float mouseSensivity = 1f;  // fare hassasiyeti
    [SerializeField] private float maxViewAngle = 90f;   // yukar�ya do�ru maksimum bak�� a��m�z.
    [SerializeField] bool invertX;  //  Sa�-sol ve yukar�-a�a�� eksenleri i�in ters y�n de�i�kenleri.
    [SerializeField] bool invertY;


    // buraya b�t�n audioClipleri listelemek i�in bu �ekilde ba�l�k olu�turup tek tek b�t�n audio cliplerimizi yaz�yoruz.
    [Header("Sound Settings")]
    [SerializeField] private List<AudioClip> footStepSounds = new List<AudioClip>();   // footstep tek olmad��� i�in b�t�n footStepsleri bir liste halinde yaz�yoruz.
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;


    // Yukar� a��da olan vekt�r�m�z� ayr� bir �ekilde tutmak istiyoruz ��nk� oyunlarda �o�unlukla g�ky�z� etkisi de�i�ebilir
    // (mesela pervane a�t���m�z� d���n�rsek havada z ekseninde bile hareket edebilir) bu y�zden bunu input ile kar��t�rmamak i�in ba�ka bir vekt�r olu�turaca��z.
    private Vector3 heightMovement;  // havadaki hareketimizi kontrol edecek olan vekt�r.


    private bool jump = false;

    private CharacterController characterController;  // i�erisinde rigidbody , collider vs olan toplu bir karakter hareket sistemi.

    private Animator anim; // animatoru referans almam�z gerek ��nk� olu�turdu�umuz 'Walk' animasyonunu animatorda tutuyoruz.

    private AudioSource audioSource;   // AudioSource componentini referans almam�z gerek ��nk� animation event ve di�er ses i�lemlerini bunu referenas alarak yapabiliriz

    private int lastIndex = -1;

    private bool landSoundPlayed = true;  // yere indi�imizde sesin iki defa ��kmas�n� engellemek i�in


    private float currentSpeed = 8f;
    private float horizontalInput;
    private float verticalInput;

    // Player i�inde olu�turmu� oldu�umuz CameraPoint objesini Main Camera olarak referans almam�z gerek ��nk� yukar� a�a�� hareket etmesi gerek.
    private Transform mainCamera;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (Camera.main.GetComponent<CameraController>() == null)   // Main Cameran�n i�inde CameraController adl� component yok ise   (G�venlik ama�l� yaz�lm�� bir k�s�m)
        {
            Camera.main.gameObject.AddComponent<CameraController>();  // o scripti bulup Main Cameran�n i�ine ekle.
        }
        mainCamera = GameObject.FindWithTag("CameraPoint").transform;
        audioSource.Stop();
    }

    private void OnEnable()   // obje aktif olunca �al���r.
    {
        newMovementInput.Enable();  // e�er bu ad�m� yapmazsak input sistemimiz �al��maz
    }

    private void OnDisable()  // obje yok olunca �al���r.
    {
        newMovementInput.Disable(); // e�er bu ad�m� yapmazsak input sistemimiz �al��maz 
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
        // z�plama kodu
        if (jump)
        {
            heightMovement.y = jumpPower;
            jump = false;   // z�plad�ktan sonra havada tekrar tekrar z�plamas�n diye sadece 1 kez �al��acak. o y�zden false 
        }

        // yer �ekimi kodu
        heightMovement.y -= gravityModifier * Time.deltaTime;


        Vector3 localVerticalVector = transform.forward * verticalInput;  // Bakt���m�z y�n�n vertical input vekt�r�.
        Vector3 localHorizontalVector = transform.right * horizontalInput;  // Bakt���m�z y�n�n horizontal input vekt�r�.

        Vector3 movementVector = localHorizontalVector + localVerticalVector;

        movementVector.Normalize();
        movementVector *= currentSpeed * Time.deltaTime;

        characterController.Move(movementVector + heightMovement);   // hareket vekt�r� ile yer�ekimi vekt�r�n� toplad�k 


        // e�er karakterimiz yere de�iyorsa   heightMovement.y -= gravityModifier * Time.deltaTime;   kodunu yok sayacak. bu �ekilde de bo�u bo�una bir kuvvet uygulamam�� olacak.
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

    private void Rotate()  // Oyuncuyu �evirme   
    {

        // sa�a sola �evirme 
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + MouseInput().x, transform.eulerAngles.z);
        // rotation yaparken transform.eulerAngles.y sa�a ve solu g�sterir. z ise cismi sa�dan sola d�nd�r�r (parande pozisyonu).x ise yukar�dan a�a�� d�nd�r�r(takla)


        // yukar� a�a�� cevirme        
        if (mainCamera != null) // Eger (burada main camera de�i�keniyle kulland���m�z) CameraPoint bo� de�il ise.
        {

          //  print(mainCamera.rotation.eulerAngles.x);
            if (mainCamera.rotation.eulerAngles.x > maxViewAngle && mainCamera.rotation.eulerAngles.x < 180f)   // her tarafa istedi�imiz �ekilde d�nemememiz i�in bu sat�r gerekli
            {
                mainCamera.rotation = Quaternion.Euler(maxViewAngle, mainCamera.rotation.eulerAngles.y, mainCamera.rotation.eulerAngles.z);   // yukar�daki ifade sa�lan�rsa x a��m�z� maxViewAngle a��s�na a�itleyecek. 
                                                                                                                                              // bu �ekilde her tarafa istenildi�i gibi d�n�lmesi engellenmi� olunacak.
            }
            else if (mainCamera.rotation.eulerAngles.x > 180f && mainCamera.rotation.eulerAngles.x < 360f - maxViewAngle)
            {
                mainCamera.rotation = Quaternion.Euler(360f - maxViewAngle, mainCamera.rotation.eulerAngles.y, mainCamera.rotation.eulerAngles.z);
            }
            else
            {
                mainCamera.rotation = Quaternion.Euler(mainCamera.rotation.eulerAngles
               + new Vector3(-MouseInput().y, 0f, 0f)); // MouseInput().y inputunu vector3'�n x eksenine koyduk ��nk� yukar� a�a�� hareket etmesini istiyoruz.
                                                        // burada Quaternion.Euler metodunun farkli bir uygulanma �eklini ele ald�k. A��lar� tek tek atamak yerine iki farkl� a��y� toplad�k.
                                                        // yukar�da yaz�lan kodda MouseInput().y k�sm�n�n ba��na - koymazsak verdi�imiz derecenin en sonunda (90) ba�lar yan, karakter yukar� bakarken oyun ba�lar.
            }
        }

    }


    private void AnimationChanger()   // Karakterimizin animasyonunu kontrol etti�imiz b�l�m.
    {
        if (newMovementInput.ReadValue<Vector2>().magnitude > 0f && characterController.isGrounded)  // E�er y�r�yorsak ve karakterimiz z�plam�yorsa animasyonumuzu oynat  
        {
            if (currentSpeed == walkSpeed)   // y�r�yorsak
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Run", false);
            }
            else if (currentSpeed == runSpeed) // ko�uyorsak
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

        // YeniInputKodu (Sadece yeni input sistemiyle kullan�labilir.)
        horizontalInput = newMovementInput.ReadValue<Vector2>().x; // Klavyeden a ve d tu�lar�nda bas�p karakterimizi sa�a sola hareket ettirmemizi sa�layan kod.
        verticalInput = newMovementInput.ReadValue<Vector2>().y;   // Klavyeden w ve s tu�lar�nda bas�p karakterimizi ileri geri hareket ettirmemizi sa�layan kod .

        #region eski space basma kodu
        //if (Input.GetKey(KeyCode.Space) && characterController.isGrounded) // space tu�una bas�l�rsa ve CharacterController componentimiz yere temas ediyorsa.
        //{
        //    jump = true;
        //}
        #endregion

        // Yeni space basma kodu
        if (Keyboard.current.spaceKey.wasPressedThisFrame && characterController.isGrounded) //  ve CharacterController componentimiz yere temas ediyorsa.
                                                                                             // �uan tak�l� olan keyboarda git ve space key'i getir ve tu�a bast���m�z anda 1 defa input al. (wasPressedThisFrame = bu kare esnas�nda tu�a 1 kez bas�ld� m�)
        {
            jump = true;
            landSoundPlayed = false;
            audioSource.PlayOneShot(jumpSound);

        }
        #region eski ko�ma kodu
        //if (Input.GetKey(KeyCode.LeftShift))
        //{
        //    currentSpeed = runSpeed;
        //}
        //else
        //{
        //    currentSpeed = walkSpeed;
        //}
        #endregion

        // Yeni ko�ma kodu
        if (Keyboard.current.leftShiftKey.isPressed)  // isPressed = Tu�a bas�l� tuttu�umuz s�rece. 
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

    }


    private void PlayFootStepSound()   // animation event k�sm�ndan �e�ilebilecek bir metot olu�turuyoruz. Bu metot ayak sesi ��kartmam�z� sa�layacak.
    { // tek yapmam�z gereken burada do�ru sesi oynatmak. 

        if (footStepSounds.Count > 0 && audioSource != null)  // atanm�� bir ayak sesi klibimiz var ise ve audioSource'umuz null de�il ise;
        {
            int index;  // indexi buraya atad���m�zda, burdaki metot �a��r�ld��� zaman son tutulan index her zaman otomatik olarak 0'a e�itleniyor.
                        // biz de yukar�da bir lastIndex de�i�keni belirtece�iz ��nk� son tutulan indexin 0 a e�it olmas�n� istemiyoruz.

            do
            {

                index = UnityEngine.Random.Range(0, footStepSounds.Count);
                if (lastIndex != index)
                {
                    if (!audioSource.isPlaying)  // audioSource herhangi bir ses �alm�yorsa alttaki kod �al��acak. bu sayede ayn� anda iki ses �st�ste binmemi� olacak
                    {
                        audioSource.PlayOneShot(footStepSounds[index]);
                        lastIndex = index;
                        break;
                    }
                }
            }
            while (index == lastIndex);   // buradaki d�ng�y� yapma amac�m�z, her ad�m att���m�zda farkl� sesin ��kmad���ndan emin olmak. ��nk� 4 adet footSteps sesi var
        }
    }


    private Vector2 MouseInput()
    {
        #region Eski Mouse Input kodu
        //return new Vector2(invertX ? -Input.GetAxisRaw("Mouse X") : Input.GetAxisRaw("Mouse X"),  //Mouse X : Horizontal (sa�a sola gitti�i) a��  Mouse Y : Vertical(yukar� a�a�� gitti�i) a��
        //    invertY ? -Input.GetAxisRaw("Mouse Y") : Input.GetAxisRaw("Mouse Y")) * mouseSensivity;
        #endregion

        #region if Input ile yaz�lm�� eski Mouse Input Kodu
        //Vector2 mouse�nput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        //if (invertX)
        //{
        //    mouse�nput.x = -mouse�nput.x;
        //}
        //if (invertY)
        //{
        //    mouse�nput.y = -mouse�nput.y;
        //}
        //return mouse�nput;
        #endregion

        // Yeni Mouse Input kodu
        return new Vector2(invertX ? -Mouse.current.delta.x.ReadValue() : Mouse.current.delta.x.ReadValue(),    //mouse.current.x : Horizontal (sa�a sola gitti�i) a��. 
            invertY ? -Mouse.current.delta.y.ReadValue() : Mouse.current.delta.y.ReadValue()) * mouseSensivity; //  mouse.current.y : Vertical(yukar� a�a�� gitti�i) a��.
    }
}
