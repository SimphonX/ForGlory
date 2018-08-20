using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using Assets.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    public float TargetRadius = 1;
    public float LaunchAngle = 45.0f;
    public float deltaAngle = 0.5f;

    public string enemyName;
    public int damage;
    private bool isYours;
    private Rigidbody rigid;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    void Awake ()
    {
        rigid = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    void Start()
    {

    }
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Terrain")
            Destroy(gameObject);
        if (other.GetComponent<Soldier>() && isYours)
        {
            if (other.transform.parent.name == "NotEnemy")
                return;
            DamageTo(other.GetComponent<Soldier>());
            Destroy(gameObject);
        }
        if (other.GetComponent<PlayerController>() && isYours)
        {
            if (other.name == "NotEnemy")
                return;
            DamageTo(other.GetComponent<PlayerController>());
            Destroy(gameObject);
        }
        
    }

    private void DamageTo(Soldier sol)
    {
        sol.TakeDamage(damage);
    }
    private void DamageTo(PlayerController sol)
    {
        sol.TakeDamage(damage);
    }

    float GetPlatformOffset(Transform TargetObjectTF)
    {
        float platformOffset = 0.0f;
        platformOffset = TargetObjectTF.localPosition.y;
        return platformOffset;
    }

    public void Launch(Transform TargetObjectTF, string name, int damage, bool isYours)
    {
        this.damage = damage;
        this.isYours = isYours;
        enemyName = name;
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 targetXZPos = new Vector3(TargetObjectTF.position.x + Random.Range(-deltaAngle, deltaAngle), 0.0f, TargetObjectTF.position.z + Random.Range(-deltaAngle, deltaAngle));

        transform.LookAt(targetXZPos);

        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (TargetObjectTF.position.y + GetPlatformOffset(TargetObjectTF)) - transform.position.y;

        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;

        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);
        rigid.velocity = globalVelocity;
    }
}
