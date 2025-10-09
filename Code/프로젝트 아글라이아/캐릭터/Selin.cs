using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine;

// [RequireComponent(typeof(RangeWeaponController))]
public class Selin : CharacterBase
{
    // RangeWeaponController rangeWeaponController;

    public Canvas myCanvas;

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

    // public Transform weaponHold;

    // 셀린 블라스트 웨이브
    public Skill blastWave;

    // 셀린 블라스트 웨이브 발사체
    public BlastWaveProjectile blastWaveProjectile;
    public GameObject pvpBlastWaveProjectile;


    //애니메이션 류
    public Animator selin_Animator;
    public Animator suitcase_Animator;


    //움직임 보정용 Transform, 벡터들. 
    [SerializeField]
    private Transform SelinModelTransform;

    private Vector3 originalPos, originalRot;


    // 블라스트 웨이브 다음 발동 가능 시간
    public float blastWaveNextTime;
    
    protected override void Start()
    {
        base.Start();
        // rangeWeaponController = GetComponent<RangeWeaponController>();
        if (!GameManager.instance.isPvp || photonView.IsMine)
        {
            // 스킬이미지 캔버스 찾기
            rightMouseButtonSkillImage = GameObject.FindWithTag("RightMouseButtonSkillIcon").GetComponent<Image>();

            // 스킬하이드 이미지 찾기
            rightMouseBtnSkillHideImage = GameObject.FindWithTag("HideImage").GetComponent<Image>();

            // 스킬쿨타임 캔버스 찾기
            rightMouseBtnSkillCooldownText = GameObject.FindWithTag("RightMouseButtonCoolTimeText").GetComponent<TMP_Text>();
            rightMouseBtnSkillCooldownText.gameObject.SetActive(false);

            // 스킬아이콘 적용
            rightMouseButtonSkillImage.sprite = blastWave.skillIcon;


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
        }

        //UI Portrait.
        if ((GameManager.instance.isPvp && photonView.IsMine) || !GameManager.instance.isPvp)
        {

            leftPortrait = GameObject.FindWithTag("Portrait").GetComponent<Image>();
            leftPortrait.sprite = thisPortrait;

        }

        // 캐릭터 이속
        speed = 3;

        // 캐릭터 체력
        health = 100;
        maxHealth = 100;

        // 투사체 이름(포톤 네트워크 오브젝트 생성을 위해서)
        projectileName = "SelinProjectile";

        // 체력바 적용
        hpBar.SetFloat("_offset", health / maxHealth - 0.5f);
        hpBar.SetFloat("_segmentAmount", maxHealth / 20);

        //애니메이션 움직임 보정용 벡터 값.
        originalPos = new Vector3(0f, -1f, -0.2f);
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
                if (selin_Animator.GetInteger("AnimationState") != (int)motionState.Walk)//지금 애니메이션이 Walk가 아닐때만. 
                {
                    //샷 애니메이션으로 바꿔주기. 
                    if (!GameManager.instance.isPvp)
                    {
                        selin_Animator.SetInteger("AnimationState", (int)motionState.Walk);
                        suitcase_Animator.SetInteger("SuitCaseEnum", (int)motionState.Walk);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Walk);
                        photonView.RPC("RPCSuitCaseSetAnimator", RpcTarget.All, (int)motionState.Walk);
                    }

                    //애니메이션 좌표값 보정하기.
                    SelinModelTransform.localPosition = originalPos;
                    SelinModelTransform.localRotation = Quaternion.Euler(originalRot);
                    StartCoroutine(animationIdleTrans(0.903f, true));
                    audioManager.playSFX(11);
                }
            }
        }
        if (currentState != State.Fainted)
        {
            if (Input.GetMouseButtonDown(0) && currentState != State.Casting)
            {
                Throw(lookPoint);

                //샷 애니메이션으로 바꿔주기. 
                if (!GameManager.instance.isPvp)
                {
                    selin_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                    suitcase_Animator.SetInteger("SuitCaseEnum", (int)motionState.Shot);
                }
                else
                {
                    photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Shot);
                    photonView.RPC("RPCSuitCaseSetAnimator", RpcTarget.All, (int)motionState.Shot);
                }

                audioManager.playSFX(2001);

                //애니메이션 좌표값 보정하기.
                SelinModelTransform.localPosition = originalPos;
                SelinModelTransform.localRotation = Quaternion.Euler(originalRot);
                StartCoroutine(animationIdleTrans(0.4167f, false));
                HasCasting(0.4167f);
            }
            if (Input.GetMouseButtonDown(1) && currentState != State.Casting)
            {
                if (Time.time > blastWaveNextTime)
                {
                    BlastWave();
                    //샷 애니메이션으로 바꿔주기. 
                    if (!GameManager.instance.isPvp)
                    {
                        selin_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                        suitcase_Animator.SetInteger("SuitCaseEnum", (int)motionState.Shot);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Shot);
                        photonView.RPC("RPCSuitCaseSetAnimator", RpcTarget.All, (int)motionState.Shot);
                    }

                    audioManager.playSFX(2003);
                    //애니메이션 좌표값 보정하기.
                    SelinModelTransform.localPosition = originalPos;
                    SelinModelTransform.localRotation = Quaternion.Euler(originalRot);
                    StartCoroutine(animationIdleTrans(0.4167f, false));
                    HasCasting(0.4167f);
                    StartCoroutine(BlastWaveCoolDown());
                }
            }
        }
    }

    // 셀린의 우클릭 스킬.
    void BlastWave()
    {
        if (!GameManager.instance.isPvp)
        {
            blastWaveProjectile.targetPosition = lookPoint;
            Instantiate(blastWaveProjectile, transform.position + Vector3.forward, transform.rotation);
            blastWaveNextTime = Time.time + blastWave.coolTime;
        }
        else
        {
            pvpBlastWaveProjectile = PhotonNetwork.Instantiate("BlastWaveProjectile", transform.position + Vector3.forward, transform.rotation);
            // 블라스트웨이브 타겟포인트 지정해주기
            pvpBlastWaveProjectile.GetComponent<PhotonView>().RPC("SetTargetPosition", RpcTarget.All, lookPoint);
        }
    }

    IEnumerator BlastWaveCoolDown()
    {
        rightMouseBtnSkillCooldownText.gameObject.SetActive(true);

        rightMouseBtnSkillHideImage.fillAmount = 1;

        while (Mathf.Max(0, blastWaveNextTime - Time.time) > 0)
        {
            // 소수점으로 표현하고 싶을 때 사용하자
            // float remainedTime = Mathf.Max(0, blastWaveNextTime - Time.time);

            // 정수형으로만 표현하고 싶을 때
            int remainedTime = Mathf.CeilToInt(blastWaveNextTime - Time.time);

            // 쿨타임 텍스트 표시
            rightMouseBtnSkillCooldownText.text = remainedTime.ToString();

            float fillValue = (blastWaveNextTime - Time.time) / blastWave.coolTime;
            rightMouseButtonSkillImage.color = coolTimeColor;

            // 쿨타임 이미지 표시
            rightMouseBtnSkillHideImage.fillAmount = (blastWaveNextTime - Time.time) / blastWave.coolTime;

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
            selin_Animator.SetInteger("AnimationState", (int)motionState.Idle);
            suitcase_Animator.SetInteger("SuitCaseEnum", (int)motionState.Idle);
        }
        else
        {
            photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)motionState.Idle);
            photonView.RPC("RPCSuitCaseSetAnimator", RpcTarget.All, (int)motionState.Idle);
        }

        currentState = State.Idle;
    }

    [PunRPC]
    void RPCCharacterSetAnimator(motionState stateValue)
    {
        selin_Animator.SetInteger("AnimationState", (int)stateValue);
    }

    [PunRPC]
    void RPCSuitCaseSetAnimator(motionState stateValue)
    {
        suitcase_Animator.SetInteger("SuitCaseEnum", (int)stateValue);
    }
}
