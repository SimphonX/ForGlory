using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    private float TargetRadius = 5.0f;
    private float LaunchAngle = 45.0f;
    private float deltaAngle = 0.5f;
    
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
        if (other.transform.parent.name.Equals("Enemy"))
        {
            Destroy(gameObject);
            //Destroy(other.gameObject);
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Equals("Terrain"))
            Destroy(gameObject);
    }
    float GetPlatformOffset(Transform TargetObjectTF)
    {
        float platformOffset = 0.0f;
        platformOffset = TargetObjectTF.localPosition.y;
        return platformOffset;
    }

    public void Launch(Transform TargetObjectTF)
    {
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
