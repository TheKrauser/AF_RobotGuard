using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour {

    //velocidade
	float speed = 20.0F;
    //rotação
    float rotationSpeed = 120.0F;
    //prefab da bala
    public GameObject bulletPrefab;
    //local que a bala vai aparecer
    public Transform bulletSpawn;

    //vida
    public float health = 100f;
    //barra de vida
    public Slider healthBar;
    //o ponto de respawn
    public Transform pontoDeRespawn;

    void Update()
    {
        //move e rotaciona baseado no vertical e horizontal
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

        //se aperta espaço atira
        if(Input.GetKeyDown("space"))
        {
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
        }

        //faz a barra de vida aparecer e ficar acima do player
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);

        //se vida menor ou igual a 0 faz ele aparecer no ponto de respawn
        if (health <= 0)
        {
            transform.position = pontoDeRespawn.position;
            health = 100;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //leva dano se colide com inimigo ou bullet
        if (collision.collider.CompareTag("bullet"))
        {
            health -= 15f;
        }

        if (collision.collider.CompareTag("inimigo"))
        {
            health -= 30f;
        }
    }
}
