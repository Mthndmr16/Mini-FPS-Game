using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    [SerializeField] int maxHealth = 5;  // Burada karakterimiz için maxHealth deðiþkeni belirledik.
    [SerializeField] int damageAmount = 2;  // Hasar yeme miktarýmý için bir deðiþken
    [SerializeField] int healthAmount = 2;  // Can alma miktarýmýz için bir deðiþken

    private int currentHealth;  // Burada da yazýlýmda kullanmak için currentHealth deðiþkeni belirledik.
   
    void Start()
    {
        currentHealth = maxHealth;  // oyun baþýnda bu deðerleri eþitliyoruz.
    }
    
    void Update()
    {
        if (currentHealth <= 0 )  // Eðer canýmýz 0 veya 0'dan küçükse Karakterimizi yok et .
        {
            Destroy(gameObject);
        }
    }

    public void GiveDamage(int damageAmount)  // Hasar yediðimizde canýmýzýn azalmasý için metod;
    {
        currentHealth -= damageAmount;
    }
    public void AddHealth(int healthAmount)  // can aldýðýmýzda canýmýzýn artmasý için metod.
    {
        currentHealth += healthAmount;
    }
}
