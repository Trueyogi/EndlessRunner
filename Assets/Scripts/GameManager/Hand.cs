using System;
using System.Collections;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isIdle { get; set; }
    public Vector3[] handPoints { get; set; } // 0 -> wrist, 1 -> pinky, 2-> index, 3-> thumb
    public float width;
    public float height;
    public GameObject shadowPrefab;
    public GameObject saveEffectPrefab;

    private bool isActive;
    private GameObject shadow;
    
    [HideInInspector] public bool targetTriggered;
    [HideInInspector] public Vector3 targetPos;

    private void Awake()
    {
        var handBounds = GetComponent<Collider>().bounds;
        width = handBounds.size.x;
        height = handBounds.size.y;
        
        shadow = Instantiate(shadowPrefab);
        isIdle = true;
        isActive = false;
        handPoints = new Vector3[4];
    }

    private void Update()
    {
        if (isIdle)
        {
            if (isActive)
            {
                isActive = false;
                shadow.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }

    public void SetActive()
    {
        if (!isActive)
        {
            isActive = true;
            shadow.SetActive(true);
            gameObject.SetActive(true);
        }
    }

    public void SetScale(Vector3 targetScale)
    {
        StartCoroutine(DoScale(targetScale));
    }

    private IEnumerator DoScale(Vector3 targetScale)
    {
        while (true)
        {
            var x = Mathf.Lerp(transform.localScale.x, targetScale.x, Time.deltaTime * 10);
            var y = Mathf.Lerp(transform.localScale.y, targetScale.y, Time.deltaTime * 10);
            var z = Mathf.Lerp(transform.localScale.z, targetScale.z, Time.deltaTime * 10);
            transform.localScale = new Vector3(x, y, z);
            if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            {
                transform.localScale = targetScale;
                yield break;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void SetHandPosition(Vector3 point)
    {
        transform.position = point;
        shadow.transform.position = point;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public void Helper(Vector3 point)
    {
        Vector3.Lerp(transform.position, point, 0.2f);
        Vector3.Lerp(shadow.transform.position, point, 0.2f);
    }

    public void SetRotation(Vector3 lookDir)
    {
        transform.up = lookDir;
    }

    private void OnCollisionEnter(Collision collision)
    {/*
        if (collision.gameObject.CompareTag("SoccerBall"))
        {
            if (collision.gameObject.GetComponent<BallController>().effectCreated)
            {
                return;
            }
            var collisionPoint = collision.GetContact(0).point;
            var effect = Instantiate(saveEffectPrefab);
            effect.transform.localRotation = Quaternion.identity;
            effect.transform.position = collisionPoint;
            effect.transform.localScale = Vector3.one;
            collision.gameObject.GetComponent<BallController>().effectCreated = true;
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TargetCollider"))
        {
            targetPos = other.gameObject.transform.position;
            targetTriggered = true;
        }
    }
}
