﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/*
   Este script se va encargar de todo lo que LA CARTA debe hacer.
   Eso incluye, su activación.
   El player ni el deck van a tener conocimiento de algun método interno.
   Va a tener un Evento que se va a disparar cuando la carta es consumida.
   El deck va a estar suscrito a dicho evento, de esta forma se entera que la carta fue útilizada.
*/

public class Card : MonoBehaviour
{
    #region Eventos
    public event Action<int> OnUseCard = delegate { };
    public Action<Actor, Actor, CardData, int> CardEffect = delegate { };
    public Func<int, bool> CanBeActivated;
    //public event Action OnCardIsSeleced = delegate { }; 
    #endregion

    [Header("Data Fundamental")]
    public CardData Stats;
    public Actor Owner;
    public Actor Rival;
    public int DeckID;

    [Header("Posicionamiento de la carta")]
    public bool back;
    public bool touchScreen = false;
    public bool stopAll = false;
    public bool isInteractuable;
    public bool comingBack = false;
    public bool inHand = false;
    public bool canBeShowed;
    public bool targetHitted = false;
    public bool toSlot = false;
    public LayerMask posLayer;
    public GameObject slotSelected;

    public Vector3 starPos;
    private Vector3 mOffset;

    public GameObject fusion;
    public GameObject Textfusion;

    [Header("Shaders")]
    public Renderer mats;
    public float shaderLerp;
    public bool shaderStart = false;
    public GameObject canvas;
    public GameObject[] objetos;

    public AudioSource ni;
    public AudioClip clickCard;
    public AudioClip noEnergy;

    //public GameObject attack;
    private float mZCoord;

    [Header("HUD")]
    public TextMeshProUGUI nameCard;
    public TextMeshProUGUI description;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI damage;
    public Image image;

    public Transform discardPosition;

    public Animator anim;
    Rigidbody rb;
    BoxCollider col;

    private void Awake()
    {
        ni = GetComponent<AudioSource>();
        discardPosition = GameObject.Find("DeckDiscard").GetComponent<Transform>();
        Owner = GetComponentInParent<Actor>();
        Rival = FindObjectOfType<Enem>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
        inHand = false;
        //isInteractuable = false;
        //starPos = transform.position;
        comingBack = true;
        back = true;
        shaderLerp = 100;
    }

    public void LoadCardDisplayInfo()
    {
        this.name = Stats.CardName;
        nameCard.text = Stats.CardName;
        description.text = Stats.description;
        cost.text = Stats.Cost.ToString();
        damage.text = Stats.GetDebuff(DeBuffType.healthReduction).Ammount.ToString();
        image.sprite = Stats.image;
        if (Stats.canFusion)
        {
            fusion.SetActive(true);
            Textfusion.SetActive(true);
        }
        else
        {
            fusion.SetActive(false);
            Textfusion.SetActive(false);
        }
    }
    

    private void Update()
    {
        if (Owner.ActorName == "Gordon Ramsay")
        {
            canBeShowed = true;
            if (inHand)
            {
                if (!back)
                    anim.SetBool(stopAll ? "ToTable" : "Flip", true);
                
                if (comingBack)
                    anim.SetBool("Flip", false);

                ToDiscard();

                GoBackToHand();

                GoToSlot();
            }
        }
        ShaderAnimation();
    }

    private void ShaderAnimation()
    {
        if (shaderStart)
        {
            shaderLerp -= 2f;
            ShadersOP(shaderLerp);
            if (shaderLerp <= 0)
            {
                comingBack = false;
                stopAll = true;
                transform.SetParent(discardPosition.transform);
                Owner.hand.AlingCards();
                shaderStart = false;
            }
        }
    }

    private void GoBackToHand()
    {
        if (comingBack)
        {
            var dist = Vector3.Distance(transform.position, starPos);
            if (dist >= 0)
                transform.position = Vector3.Lerp(transform.position, starPos, Time.deltaTime * 6f);
            else
                comingBack = false;
        }
    }

    private void ToDiscard()
    {
        if (stopAll)
        {
            var dist = Vector3.Distance(transform.position, discardPosition.position);
            if (dist >= 0)
                transform.position = Vector3.Lerp(transform.position, discardPosition.position, Time.deltaTime * 3f);

            else
            {
                inHand = false;
                stopAll = false;
            }
        }
    }


    public void ShadersOP(float sec)
    {
        foreach (var item in mats.materials)
        {
            item.SetFloat("_Progress", sec / 100);
        }
    }

    public void GoToSlot()
    {
        if (toSlot)
        {
            if (Stats.canFusion)
            {
                if (CanBeActivated(Stats.Cost))
                {
                    if (Vector3.Distance(transform.position, slotSelected.transform.position) > 1f)
                        transform.position = Vector3.Lerp(transform.position, slotSelected.transform.position, Time.deltaTime);
                    else
                    {
                        int cost = -Stats.Cost;
                        Owner.ModifyEnergy(cost);
                        transform.SetParent(slotSelected.transform);
                        transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                        slotSelected.GetComponent<Slot>().AddCard(this);
                        Owner.hand.hand.Remove(DeckID);
                        toSlot = false;
                    }

                }
                else
                {
                    touchScreen = false;
                    back = false;
                    comingBack = true;
                    toSlot = false;
                    CombatManager.match.HUDAnimations.SetTrigger("PlayerNoENergy");
                    ni.clip = noEnergy;
                    ni.Play();
                }
            }
            else
            {
                touchScreen = false;
                back = false;
                comingBack = true;
                toSlot = false;

            }
        }
    }

    public void ActivateCard()
    {
        if (CanBeActivated(Stats.Cost))
        {
            CardEffect(Owner, Rival, Stats, DeckID);
            //Acá va todos los efectos.
            //shaderStart = true;
            OnUseCard(Stats.ID);
        }
        else
        {
            touchScreen = false;
            back = false;
            comingBack = true;
            CombatManager.match.HUDAnimations.SetTrigger("PlayerNoENergy");
            ni.clip = noEnergy;
            ni.Play();
        }

    }

    public void OnMouseDown()
    {
        if (Owner.ActorName == "Gordon Ramsay")
        {
            if (isInteractuable)
            {
                if (!stopAll)
                {
                   // starPos = transform.position;
                    comingBack = false;
                    touchScreen = false;
                    back = true;
                    mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
                    mOffset = transform.position - GetMouseAsWorldPoint();
                    ni.clip = clickCard;
                    ni.Play();
                }
            }
        }
    }
    public void OnMouseDrag()
    {
        if (Owner.ActorName == "Gordon Ramsay")
        {
            if (isInteractuable)
            {
                if (!stopAll)
                {
                    transform.position = GetMouseAsWorldPoint() + mOffset;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, posLayer))
                    {
                        //Esto es un juego de booleans >:D
                        if (hit.collider.gameObject.layer == 10)
                        {
                            slotSelected = null;
                            targetHitted = true;
                            back = !targetHitted;
                            touchScreen = targetHitted;
                        }
                        else if (hit.collider.gameObject.layer == 11)
                        {
                            slotSelected = hit.collider.gameObject;
                            targetHitted = true;
                            back = !targetHitted;
                            touchScreen = targetHitted;
                        }
                        else
                        {
                            slotSelected = null;
                            targetHitted = false;
                            back = !targetHitted;
                            touchScreen = targetHitted;
                        }
                    }
                    
                }
            }
        }
    }
    private void OnMouseUp()
    {
        if (Owner.ActorName == "Gordon Ramsay")
        {
            if (isInteractuable)
            {
                if (!stopAll)
                {
                    if (back)
                        comingBack = true;
                    else if (touchScreen && slotSelected != null)
                        toSlot = true;
                    else if (touchScreen && !toSlot)
                        ActivateCard();

                }
            }
        }
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}