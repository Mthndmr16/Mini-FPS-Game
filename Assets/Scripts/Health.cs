using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    [SerializeField] int maxHealth = 5;  // Burada karakterimiz i�in maxHealth de�i�keni belirledik.
    [SerializeField] int damageAmount = 2;  // Hasar yeme miktar�m� i�in bir de�i�ken
    [SerializeField] int healthAmount = 2;  // Can alma miktar�m�z i�in bir de�i�ken

    private int currentHealth;  // Burada da yaz�l�mda kullanmak i�in currentHealth de�i�keni belirledik.
   
    void Start()
    {
        currentHealth = maxHealth;  // oyun ba��nda bu de�erleri e�itliyoruz.
    }
    
    void Update()
    {
        if (currentHealth <= 0 )  // E�er can�m�z 0 veya 0'dan k���kse Karakterimizi yok et .
        {
            Destroy(gameObject);
        }
    }

    public void GiveDamage(int damageAmount)  // Hasar yedi�imizde can�m�z�n azalmas� i�in metod;
    {
        currentHealth -= damageAmount;
    }
    public void AddHealth(int healthAmount)  // can ald���m�zda can�m�z�n artmas� i�in metod.
    {
        currentHealth += healthAmount;
    }
}
