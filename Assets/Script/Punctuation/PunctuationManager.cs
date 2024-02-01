using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class PunctuationManager : MonoBehaviour
{
    public static PunctuationManager Instance;

    [SerializeField] TMP_Text punctuationText;
    [SerializeField] Transform pointsGroupTransform;
    [SerializeField] GameObject pointsPrefab;
    [SerializeField] int pointsQuantity;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float pitchRatio;

    public List<GameObject> pooledPoints;

    private int _punctuation = 0;
    private const float Duration = 0.5f;

    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        pooledPoints = new List<GameObject>();
        GameObject temporary;

        for(int i = 0; i < pointsQuantity; i++)
        {
            temporary = Instantiate(pointsPrefab, pointsGroupTransform);
            temporary.SetActive(false);
            pooledPoints.Add(temporary);
        }
    }

    public Tween ShowPointsAdded(int pointsToAdd)
    {
        Sequence showSequence = DOTween.Sequence();
        TMP_Text pointsText;
        for (int i = 0; i < pointsQuantity; i++)
        {
            if (!pooledPoints[i].activeSelf)
            {
                pooledPoints[i].SetActive(true);
                pointsText = pooledPoints[i].GetComponent<TMP_Text>();
                _punctuation += pointsToAdd;
                pointsText.text = pointsToAdd.ToString();
                punctuationText.text = _punctuation.ToString();
                showSequence.Append(pointsText.DOFade(1, Duration)).OnStart(PlayComboClip);
                showSequence.Append(pointsText.DOFade(0, Duration)).OnComplete(() => pooledPoints[i].SetActive(false));
                break;
            }
        }
        return showSequence;
    }

    public void PlayComboClip()
    {
        audioSource.pitch *= pitchRatio;
        audioSource.Play();
    }

    public void ResetClipPitch()
    {
        audioSource.pitch = 1;
    }
}
