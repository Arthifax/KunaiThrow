using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Reticle : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float selectAnimTime;
    [SerializeField] private float deselectAnimTime;

    [Header("Drag Settings")] 
    [SerializeField] private float amplitude;
    
    [Header("Spring")]
    [SerializeField] private float stiffness;
    [SerializeField] float clamp;

    [Header("Reticle Points")] 
    [SerializeField] private List<GameObject> reticlePoints = new List<GameObject>();

    [SerializeField] private List<GameObject> launchPoints = new List<GameObject>();

    private List<Vector3> pointStartPos = new List<Vector3>();

    [SerializeField] public GameObject selectedObject;

    private void Awake()
    {
        foreach (GameObject reticlePoint in reticlePoints)
        {
            pointStartPos.Add(reticlePoint.transform.localPosition);
        }
    }

    private void Update()
    {
        if (selectedObject != null)
        {
            StopAllCoroutines();
            Selected(selectedObject);
        }
    }

    private void HandleRotation(ReticlePoint reticlePoint)
    {
        Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        reticlePoint.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void HandlePoint(Vector3 mousePosRelative, float distance, int i, ReticlePoint reticlePoint)
    {
        int dir = 0;
        if (reticlePoint.flip)
            dir = -1;
        else
            dir = 1;

        Vector3 startPos = pointStartPos[i];

        float magnitude = (amplitude * distance) / distance;
        float pointIdentity = (startPos.x * magnitude) * dir;

        Vector3 targetPos = (mousePosRelative * pointIdentity) * dir;

        float lerpTime = (selectAnimTime / pointIdentity) * Time.deltaTime;
        Vector3 lerpPos = Vector3.Lerp(reticlePoint.transform.localPosition, targetPos, lerpTime);
        
        lerpPos.z = 0;
        float pointDistance = Vector3.Distance(reticlePoint.transform.position, transform.position);
        Vector3 finalPos = Vector3.ClampMagnitude(lerpPos, (pointIdentity / magnitude) + (pointDistance / clamp));

        reticlePoint.transform.localPosition = finalPos;
    }
    
    public void Selected(GameObject selected)
    {
        gameObject.SetActive(true);
        transform.position = selected.transform.position;
        selectedObject = selected;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, transform.position.z - Camera.main.transform.position.z));
        Vector3 mousePosRelative = mouseWorldPos - transform.position;

        for (int i = 0; i < reticlePoints.Count; i++)
        {
            HandleRotation(reticlePoints[i].GetComponent<ReticlePoint>());
            float distance = Vector2.Distance(transform.position, mouseWorldPos);
            ReticlePoint reticlePoint = reticlePoints[i].GetComponent<ReticlePoint>();
            HandlePoint(mousePosRelative, distance, i, reticlePoint);
        }
    }

    public void Deselect()
    {
        selectedObject = null;
        for (int i = 0; i < reticlePoints.Count; i++)
        {
            StartCoroutine(LerpObject(reticlePoints[i], Vector3.zero, deselectAnimTime));
        }
    }

    private void FireProjectile()
    {
        for (int i = 0; i < launchPoints.Count; i++)
        {
            launchPoints[i].GetComponent<ShootProjectile>().FireProjectile();
        }   
        this.gameObject.SetActive(false);
    }

    private int count = 0;
    
    private IEnumerator LerpObject(GameObject item, Vector3 pos, float time)
    {
        Vector3 currentPos = item.transform.localPosition;
        float elapsed = 0f;
        float distance = Vector3.Distance(currentPos, pos);
        float ratio = 0;
        while (ratio < 1)
        {
            elapsed += Time.fixedDeltaTime;
            float offset = animCurve.Evaluate(ratio);
            float newOffset = offset - ratio;
            newOffset = newOffset / stiffness;
            offset = newOffset + ratio;
            float invertOffset = 1.0f - offset;
            item.transform.localPosition = Vector3.Lerp(currentPos, pos, ratio) * invertOffset;

            yield return null;
            ratio = (elapsed / time);
        }
        count++;
        if (count >= reticlePoints.Count)
        {
            FireProjectile();
        }
    }
}
