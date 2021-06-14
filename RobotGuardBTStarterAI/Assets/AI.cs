using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //variavel do player
    public Transform player;
    //onde a bala spawna
    public Transform bulletSpawn;
    //slider da barra de vida
    public Slider healthBar;
    //prefab que vai aparecer no tiro  
    public GameObject bulletPrefab;

    //agente do navmesh
    NavMeshAgent agent;
    //destino
    public Vector3 destination;
    //alvo
    public Vector3 target;
    //vida do robo
    float health = 100.0f;
    //velocidade de rotação
    float rotSpeed = 5.0f;
    
    //raio que ve o player
    float visibleRange = 80.0f;
    //define a stopping distance no navmesh
    public float shotRange = 40.0f;

    //local que o robo vai fugir
    public Transform fuga;

    //locais que vai fazer patrulha
    public Transform[] patrulha;
    void Start()
    {
        //pega o agente
        agent = this.GetComponent<NavMeshAgent>();
        //seta a stopping distance
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //deixa a vida igual a 100
        health = 100;
        //aumenta a vida dos robos
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //atualiza a barra de vida na tela
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    void UpdateHealth()
    {
       if(health < 100)
        health ++;
    }

    [Task]
    public void PickRandomDestination()
    {
        //escolhe um destino aleatório dentro de 100 de distancia
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void MoveToDestination()
    {
        //move para o destino aleatorio
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);

            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                Task.current.Succeed();
            }
    }

    [Task]
    public void PickDestination(int x, int z)
    {
        //vai para um lugar fixo
        Vector3 dest = new Vector3(x, 0, z);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayer()
    {
        //deixa o player como alvo
        target = player.transform.position;
        Task.current.Succeed();
    }

    [Task]
    public bool Fire()
    {
        //atira
        GameObject bullet = GameObject.Instantiate(bulletPrefab,
            bulletSpawn.transform.position, bulletSpawn.transform.rotation);

        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);

        return true;
    }

    [Task]
    public void LookAtTarget()
    {
        //olha para o alvo
        Vector3 direction = target - this.transform.position;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        if(Vector3.Angle(this.transform.forward, direction) < 0.5f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    bool SeePlayer()
    {
        //decide se o player esta na visao do robo
        Vector3 distance = player.transform.position - this.transform.position;
        RaycastHit hit;
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        }

        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task]
    bool Turn(float angle)
    {
        //vira o robo pra uma direção
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }

    [Task]
    //foge do player indo para o local da fuga
    public void Fugir()
    {
        agent.SetDestination(fuga.position);

        //quando tiver perto diz que completou a tarefa
        if (Vector3.Distance(transform.position, fuga.position) < 5f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void RecuperaVida()
    {
        //volta a vida para o maximo
        health = 100;
    }

    [Task]
    public bool IsHealthLessThan(float valor)
    {
        //vida menor que o valor retorna true
        if (health <= valor * 0.1)
            return true;
        else
            return false;
    }

    [Task]
    public void Explode()
    {
        //destroi o bot e a barra de vida
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    [Task]
    public void Patrulha01()
    {
        //faz a patrulha para o ponto 1
        agent.SetDestination(patrulha[0].position);

        if (Vector3.Distance(transform.position, patrulha[0].position) <= 5f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void Segue()
    {
        //segue o player
        agent.SetDestination(player.position);
    }

    [Task]
    public void Patrulha02()
    {
        //faz a patrulha para o ponto 2
        agent.SetDestination(patrulha[1].position);

        if (Vector3.Distance(transform.position, patrulha[1].position) <= 5f)
        {
            Task.current.Succeed();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        //se collidir com a bala leva dano
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }
}

