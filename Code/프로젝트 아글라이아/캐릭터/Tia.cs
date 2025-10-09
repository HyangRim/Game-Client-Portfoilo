using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine.VFX;

public class Tia : CharacterBase
{

    public Skill tiaRightMouseButtonSkill;


    // UI 캔버스의 오른쪽 마우스 버튼 스킬의 아이콘
    public Image rightMouseButtonSkillImage;

    public Image rightMouseBtnSkillHideImage;

    // UI 캔버스의 왼쪽 초상화 아이콘.
    public Image leftPortrait;
    [SerializeField] private Sprite thisPortrait;

    // 쿨타임 텍스트    
    public TMP_Text rightMouseBtnSkillCooldownText;

    // 이미지 컬러
    public Color color;

    // 쿨타임때의 컬러
    public Color coolTimeColor;

    // 스킬 사용을 위한 큐브
    public GameObject rainbowDrawingCube;

    // 레인보우 드로잉 이펙트
    public GameObject rainbowDrawingEffect;
    public Rigidbody myRigidbody;

    // 우클릭 스킬 다음 스킬 사용 가능 시각
    float nextRightMouseButton_SkillTime;


    //애니메이션 류
    public Animator tia_Animator;
    public Animator tiapen_Animator;


    //움직임 보정용 Transform, 벡터들. 
    [SerializeField]
    private Transform tiaModelTransform;

    private Vector3 originalPos, originalRot;


    [SerializeField] private GameObject tiaShotHitObject;
    [SerializeField] private string tiaShotHitObjectName;

    protected override void Start()
    {
        base.Start();
        // rangeWeaponController = GetComponent<RangeWeaponController>();
        myRigidbody = GetComponent<Rigidbody>();

        // 스킬 사용을 위한 큐브 소환
        rainbowDrawingCube = Instantiate(rainbowDrawingCube, Vector3.zero, rainbowDrawingCube.transform.rotation) as GameObject;
        rainbowDrawingCube.SetActive(false);

        // 스킬이미지 캔버스 찾기
        rightMouseButtonSkillImage = GameObject.Find("Canvas/RightMouseButtonSkillIcon").GetComponent<Image>();

        // 스킬하이드 이미지 찾기
        rightMouseBtnSkillHideImage = GameObject.Find("Canvas/RightMouseButtonSkillIcon/HideImage").GetComponent<Image>();

        // 스킬쿨타임 캔버스 찾기
        rightMouseBtnSkillCooldownText = GameObject.Find("Canvas/RightMouseButtonSkillIcon/RightMouseButtonCoolTimeText").GetComponent<TMP_Text>();
        rightMouseBtnSkillCooldownText.gameObject.SetActive(false);

        // 스킬아이콘 적용
        rightMouseButtonSkillImage.sprite = tiaRightMouseButtonSkill.skillIcon;


        // 쿨타임 표시 이미지를 0으로 채워서 만듬
        rightMouseBtnSkillHideImage.fillAmount = 0;

        // fillAmount 방향을 시계방향으로 설정
        rightMouseBtnSkillHideImage.fillClockwise = false;

        // 스킬아이콘 투명도 변경 (디폴트가 투명이라 변경 해줘야함.)
        color = rightMouseButtonSkillImage.color;
        color.a = 1f;
        rightMouseButtonSkillImage.color = color;

        // 쿨타임 중일때 이미지 투명도
        coolTimeColor = rightMouseButtonSkillImage.color;
        coolTimeColor.a = 0.8f;

        // 캐릭터 이속
        speed = 2.9f;

        // 캐릭터 체력
        health = 95;
        maxHealth = 95;

        // 체력바에 적용
        hpBar.SetFloat("_offset", health / maxHealth - 0.5f);
        hpBar.SetFloat("_segmentAmount", maxHealth / 20);

        //UI Portrait.
        if ((GameManager.instance.isPvp && photonView.IsMine) || !GameManager.instance.isPvp)
        {

            leftPortrait = GameObject.FindWithTag("Portrait").GetComponent<Image>();
            leftPortrait.sprite = thisPortrait;

        }


        //애니메이션 움직임 보정용 벡터 값.
        originalPos = new Vector3(0f, 0f, 0f);
        originalRot = this.transform.localRotation.eulerAngles;
    }

    protected override void Update()
    {
        if (GameManager.instance.isPvp)
        {
            //Photon
            if (!photonView.IsMine)
                return;
        }
        base.Update();
        if (currentState != State.Fainted && currentState != State.Casting)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                if (tia_Animator.GetInteger("AnimationState") != (int)motionState.Walk)//지금 애니메이션이 Walk가 아닐때만. 
                {
                    //샷 애니메이션으로 바꿔주기. 
                    if (!GameManager.instance.isPvp)
                    {
                        tia_Animator.SetInteger("AnimationState", (int)motionState.Walk);
                        tiapen_Animator.SetInteger("AnimationState", (int)motionState.Walk);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Walk);
                        photonView.RPC("RPCTiapenSetAnimator", RpcTarget.All, (int)motionState.Walk);
                    }

                    //애니메이션 좌표값 보정하기.
                    tiaModelTransform.localPosition = originalPos;
                    tiaModelTransform.localRotation = Quaternion.Euler(originalRot);

                    StartCoroutine(animationIdleTrans(0.693f, true));
                    audioManager.playSFX(11);
                }
            }
        }



        if (Input.GetMouseButtonDown(0) && currentState != State.Casting)
        {
            //일반적인 Projectile 쏘고. 
            GameObject tiaProjectile = Shoot();
            GameObject tiaShotHittedObject;
            tiaProjectile.GetComponent<dyeStuffsProejctile>().InitExplosionRotation(this.transform.rotation.eulerAngles);

            //여기서 HitEffect 생성
            if (!GameManager.instance.isPvp)
            {
                tiaShotHittedObject = Instantiate(tiaShotHitObject, transform.position + Vector3.forward, transform.rotation);
            }
            else
            {
                tiaShotHittedObject = PhotonNetwork.Instantiate(tiaShotHitObjectName, transform.position + Vector3.forward, transform.rotation);
            }
            tiaProjectile.GetComponent<dyeStuffsProejctile>().SetHitObject(tiaShotHittedObject);
            tiaShotHitObject.SetActive(false);


            //샷 애니메이션으로 바꿔주기. 
            if (!GameManager.instance.isPvp)
            {
                tia_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                tiapen_Animator.SetInteger("AnimationState", (int)motionState.Shot);
            }
            else
            {
                photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Shot);
                photonView.RPC("RPCTiapenSetAnimator", RpcTarget.All, (int)motionState.Shot);
            }

            //애니메이션 좌표값 보정하기.
            tiaModelTransform.localPosition = originalPos;
            tiaModelTransform.localRotation = Quaternion.Euler(originalRot);
            StartCoroutine(animationIdleTrans(0.4167f, false));

            HasCasting(0.3f);
            audioManager.playSFX(4001);
        }
        if (Input.GetMouseButtonDown(1) && currentState != State.Casting)
        {
            if (Time.time > nextRightMouseButton_SkillTime)
            {
                StartCoroutine(RainbowDrawing());
                if (!GameManager.instance.isPvp)
                {
                    Instantiate(rainbowDrawingEffect, lookPoint, rainbowDrawingCube.transform.rotation);
                }
                else
                {
                    PhotonNetwork.Instantiate("RainbowDrawing", lookPoint, transform.rotation);
                }
                nextRightMouseButton_SkillTime = Time.time + tiaRightMouseButtonSkill.coolTime;
                StartCoroutine(RainbowDrawingCoolDown());

                //샷 애니메이션으로 바꿔주기. 
                if (!GameManager.instance.isPvp)
                {
                    tia_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                    tiapen_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                }
                else
                {
                    photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Shot);
                    photonView.RPC("RPCTiapenSetAnimator", RpcTarget.All, (int)motionState.Shot);
                }

                audioManager.playSFX(4003);
                //애니메이션 좌표값 보정하기.

                tiaModelTransform.localPosition = originalPos;
                tiaModelTransform.localRotation = Quaternion.Euler(originalRot);
                StartCoroutine(animationIdleTrans(1.0f, false));
                HasCasting(1.5f);
            }
        }


    }

    IEnumerator RainbowDrawing()
    {
        rainbowDrawingCube.transform.position = lookPoint;
        float timeCnt = 0;
        while (timeCnt < tiaRightMouseButtonSkill.castingTime)
        {
            timeCnt += Time.deltaTime;
            yield return null;
        }
        rainbowDrawingCube.SetActive(true);
        yield return new WaitForFixedUpdate();
        rainbowDrawingCube.SetActive(false);
    }
    IEnumerator RainbowDrawingCoolDown()
    {
        rightMouseBtnSkillCooldownText.gameObject.SetActive(true);

        rightMouseBtnSkillHideImage.fillAmount = 1;

        while (Mathf.Max(0, nextRightMouseButton_SkillTime - Time.time) > 0)
        {
            // 소수점으로 표현하고 싶을 때 사용하자
            // float remainedTime = Mathf.Max(0, nextRightMouseButton_SkillTime - Time.time);

            // 정수형으로만 표현하고 싶을 때
            int remainedTime = Mathf.CeilToInt(nextRightMouseButton_SkillTime - Time.time);

            // 쿨타임 텍스트 표시
            rightMouseBtnSkillCooldownText.text = remainedTime.ToString();

            float fillValue = (nextRightMouseButton_SkillTime - Time.time) / tiaRightMouseButtonSkill.coolTime;
            rightMouseButtonSkillImage.color = coolTimeColor;

            // 쿨타임 이미지 표시
            rightMouseBtnSkillHideImage.fillAmount = (nextRightMouseButton_SkillTime - Time.time) / tiaRightMouseButtonSkill.coolTime;

            yield return null;
        }
        rightMouseBtnSkillCooldownText.gameObject.SetActive(false);

        rightMouseButtonSkillImage.color = color;

        rightMouseBtnSkillHideImage.fillAmount = 0;
    }



    private IEnumerator animationIdleTrans(float animeTime, bool isWalkable)
    {

        if (!isWalkable) currentState = State.Casting;
        yield return new WaitForSeconds(animeTime);

        //Idle 애니메이션으로 바꿔주기. 
        if (!GameManager.instance.isPvp)
        {
            tia_Animator.SetInteger("AnimationState", (int)motionState.Idle);
            tia_Animator.SetInteger("AnimationState", (int)motionState.Idle);
        }
        else
        {
            photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Idle);
            photonView.RPC("RPCTiapenSetAnimator", RpcTarget.All, (int)motionState.Idle);
        }

        currentState = State.Idle;
    }

    [PunRPC]
    void RPCCharacterSetAnimator(motionState stateValue)
    {
        tia_Animator.SetInteger("AnimationState", (int)stateValue);
    }

    [PunRPC]
    void RPCTiapenSetAnimator(motionState stateValue)
    {
        tiapen_Animator.SetInteger("AnimationState", (int)stateValue);
    }
}
