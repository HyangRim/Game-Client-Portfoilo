using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Net;

// [RequireComponent(typeof(RangeWeaponController))]
public class Katia : CharacterBase
{
    // RangeWeaponController rangeWeaponController;

    // 마우스 우클릭 스킬
    public Skill rightMouseButtonSkill;

    // 다음 마우스 우클릭 스킬 사용 가능 시각
    float next_rightMouseButton_SkillTime;

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

    // 카티야 스킬 범위 만큼의 오브젝트
    public GameObject rightMouseButton_Skill_RangeObject;

    // 카티야 E스킬 충돌 마스크
    public LayerMask eSkillCollsionMask;


    //애니메이션 류
    public Animator katiya_Animator;
    public Animator gun_Animator;


    //움직임 보정용 Transform, 벡터들. 
    [SerializeField]
    private Transform katiyaModelTransform;

    private Vector3 originalPos, originalRot;


    //Katiya_Muzzle
    [SerializeField]
    private Transform katiyaMuzzleEffect;
    private Vector3 katiyaMuzzleTar, katiyaMuzzleZero;

    // bool managerExist = true;
    float idleElapsedTime = 0f;


    protected override void Start()
    {
        base.Start();
        // rangeWeaponController = GetComponent<RangeWeaponController>();
        rightMouseButton_Skill_RangeObject = CreateInstanceManager.instance.CreateOrderedObject(rightMouseButton_Skill_RangeObject);
        

        //UI Portrait.
        if ((GameManager.instance.isPvp && photonView.IsMine) || !GameManager.instance.isPvp)
        {

            leftPortrait = GameObject.FindWithTag("Portrait").GetComponent<Image>();
            leftPortrait.sprite = thisPortrait;

        }


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
            rightMouseButtonSkillImage.sprite = rightMouseButtonSkill.skillIcon;


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

        // 캐릭터 이속
        speed = 3;

        // 캐릭터 체력
        health = 80;
        maxHealth = 80;

        // 투사체 이름(포톤 네트워크 오브젝트 생성을 위해서)
        projectileName = "KatiaProjectile";

        // 체력바 적용
        hpBar.SetFloat("_offset", health / maxHealth - 0.5f);
        hpBar.SetFloat("_segmentAmount", maxHealth / 20);

        //애니메이션 움직임 보정용 벡터 값.
        originalPos = Vector3.zero;
        if (GameManager.instance.isPvp)
        {
            originalPos = new Vector3(-0.2f, 0f, 0f);
        }
        originalRot = this.transform.localRotation.eulerAngles;

        katiyaMuzzleTar = katiyaMuzzleEffect.localScale;
        katiyaMuzzleZero = Vector3.zero;

    }

    protected override void Update()
    {
        if (GameManager.instance.isPvp)
        {
            //Photon
            if (!photonView.IsMine)
                return;
        }
        if (currentState == State.Fainted || currentState == State.Casting) return;
        if (!Input.anyKey)
        {
            idleElapsedTime += Time.deltaTime;
            if (idleElapsedTime > 1f)
            {
                katiyaModelTransform.localPosition = originalPos;
                idleElapsedTime = 0f;
            }
        }
        else
        {
            idleElapsedTime = 0f;
        }

        base.Update();
        if (currentState != State.Fainted && currentState != State.Casting)
        {
            //Debug.Log("hihi");
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                if (katiya_Animator.GetInteger("AnimationState") != (int)motionState.Walk)//지금 애니메이션이 Walk가 아닐때만. 
                {
                    //샷 애니메이션으로 바꿔주기. 
                    if(!GameManager.instance.isPvp)
                    {
                        katiya_Animator.SetInteger("AnimationState", (int)motionState.Walk);
                        gun_Animator.SetInteger("GunEnum", (int)motionState.Walk);
                    } else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, motionState.Walk);
                        photonView.RPC("RPCGunSetAnimator", RpcTarget.All, motionState.Walk);
                    }

                    //애니메이션 좌표값 보정하기.
                    katiyaModelTransform.localPosition = originalPos;
                    katiyaModelTransform.localRotation = Quaternion.Euler(originalRot);
                    StartCoroutine(animationIdleTrans(0.833f, true));
                    audioManager.playSFX(11);
                }
            }
        }
        if (Input.GetMouseButtonDown(0) && currentState != State.Casting)
        {
            //샷 애니메이션으로 바꿔주기. 
            if(!GameManager.instance.isPvp)
            {
                katiya_Animator.SetInteger("AnimationState", (int)motionState.Shot);
                gun_Animator.SetInteger("GunEnum", (int)motionState.Shot);
            } else
            {
                photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, motionState.Shot);
                photonView.RPC("RPCGunSetAnimator", RpcTarget.All, motionState.Shot);
            }

            //샷 이펙트 효과 활성화.
            StartCoroutine(katiaMuzzle(katiyaMuzzleTar, 0.12f));

            bool whatSFX = (Random.value > 0.5f);
            if (whatSFX) audioManager.playSFX(1001);
            else audioManager.playSFX(1002);
            //애니메이션 좌표값 보정하기.
            katiyaModelTransform.localPosition = originalPos;
            katiyaModelTransform.localRotation = Quaternion.Euler(originalRot);
            StopCoroutine(animationIdleTrans(0.4867f, false));
            StartCoroutine(animationIdleTrans(0.4867f, false));
            HasCasting(0.4867f);


            //샷 쏘기. 
            Shoot();
        }

        if (Input.GetMouseButtonDown(1) && currentState != State.Casting)
        {
            if (Time.time > next_rightMouseButton_SkillTime)
            {
                StartCoroutine(BackStepShot());
                next_rightMouseButton_SkillTime = Time.time + rightMouseButtonSkill.coolTime;

                //카티야 Eskill motion 실행. 
                if(!GameManager.instance.isPvp)
                {
                    katiya_Animator.SetInteger("AnimationState", (int)motionState.ESkill);
                    gun_Animator.SetInteger("GunEnum", (int)motionState.ESkill);
                } else
                {
                    photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, motionState.ESkill);
                    photonView.RPC("RPCGunSetAnimator", RpcTarget.All, motionState.ESkill);
                }
                

                //애니메이션 좌표값 보정하기.
                katiyaModelTransform.localPosition = originalPos;
                katiyaModelTransform.localRotation = Quaternion.Euler(originalRot);

                audioManager.playSFX(1003);

                StartCoroutine(BackStepShotCoolDown());
                StartCoroutine(animationIdleTrans(0.5725f, false));
                HasCasting(0.5725f);
            }
        }
    }

    // 카티아 오른쪽 마우스 버튼 클릭 스킬
    IEnumerator BackStepShot()
    {
        float backStepDistance = 5;
        // 범위 공격 큐브 생성
        rightMouseButton_Skill_RangeObject.transform.position = transform.position;
        rightMouseButton_Skill_RangeObject.transform.rotation = transform.rotation;
        rightMouseButton_Skill_RangeObject.SetActive(true);


        // 벡 스탭 로직
        Vector3 originalPosition = transform.position;
        Vector3 targetDirection = -(lookPoint - originalPosition).normalized;
        targetDirection.y = 0;
        Vector3 targetPosition = originalPosition + backStepDistance * targetDirection;

        // targetPosition 수정 (장애물 고려해서)
        Ray ray = new Ray(originalPosition, targetDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, backStepDistance, eSkillCollsionMask, QueryTriggerInteraction.Collide))
        {
            Vector3 startHit = hit.point;
            Vector3 endHit = targetPosition;
            bool isEndHit = false;

            ray = new Ray(targetPosition, -targetDirection);
            if (Physics.Raycast(ray, out hit, backStepDistance, eSkillCollsionMask, QueryTriggerInteraction.Collide))
            {
                isEndHit = true;
                endHit = hit.point;
            }
            else
            {
                targetPosition = startHit;
            }

            float calDistance = Vector3.Distance(originalPosition, endHit);

            if (isEndHit && calDistance < backStepDistance)
            {
                ray = new Ray(endHit, targetDirection);
                if (Physics.Raycast(ray, out hit, backStepDistance - calDistance, eSkillCollsionMask, QueryTriggerInteraction.Collide))
                {
                    targetPosition = hit.point;
                }
            }
        }

        // 뒤로 점프하는 이동 속도
        float stepSpeed = 2;

        float percent = 0;

        while (percent <= 1)
        {
            percent += Time.deltaTime * stepSpeed;
            transform.position = Vector3.Lerp(originalPosition, targetPosition, percent);
            yield return null;
        }

        // 큐브 비활성화
        rightMouseButton_Skill_RangeObject.SetActive(false);
    }

    IEnumerator BackStepShotCoolDown()
    {
        if (!GameManager.instance.isPvp || photonView.IsMine)
        {
            rightMouseBtnSkillCooldownText.gameObject.SetActive(true);

            rightMouseBtnSkillHideImage.fillAmount = 1;

            while (Mathf.Max(0, next_rightMouseButton_SkillTime - Time.time) > 0)
            {
                // 소수점으로 표현하고 싶을 때 사용하자
                // float remainedTime = Mathf.Max(0, next_rightMouseButton_SkillTime - Time.time);

                // 정수형으로만 표현하고 싶을 때
                int remainedTime = Mathf.CeilToInt(next_rightMouseButton_SkillTime - Time.time);

                // 쿨타임 텍스트 표시
                rightMouseBtnSkillCooldownText.text = remainedTime.ToString();

                float fillValue = (next_rightMouseButton_SkillTime - Time.time) / rightMouseButtonSkill.coolTime;
                rightMouseButtonSkillImage.color = coolTimeColor;

                // 쿨타임 이미지 표시
                rightMouseBtnSkillHideImage.fillAmount = (next_rightMouseButton_SkillTime - Time.time) / rightMouseButtonSkill.coolTime;

                yield return null;
            }
            rightMouseBtnSkillCooldownText.gameObject.SetActive(false);

            rightMouseButtonSkillImage.color = color;

            rightMouseBtnSkillHideImage.fillAmount = 0;
        }
    }
    private IEnumerator animationIdleTrans(float animeTime, bool isWalkable)
    {

        if (!isWalkable) currentState = State.Casting;
        yield return new WaitForSeconds(animeTime);

        //Idle 애니메이션으로 바꿔주기. 
        if(!GameManager.instance.isPvp)
        {
            katiya_Animator.SetInteger("AnimationState", (int)motionState.Idle);
            gun_Animator.SetInteger("GunEnum", (int)motionState.Idle);
        } else
        {
            photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, motionState.Idle);
            photonView.RPC("RPCGunSetAnimator", RpcTarget.All, motionState.Idle);
        }
        
        currentState = State.Idle;
    }

    private IEnumerator katiaMuzzle(Vector3 target, float time)
    {

        float elapsedTime = 0f;
        katiyaMuzzleEffect.gameObject.SetActive(true);
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            katiyaMuzzleEffect.localScale = Vector3.Lerp(katiyaMuzzleZero, target, elapsedTime / time);
            yield return null;
        }
        katiyaMuzzleEffect.gameObject.SetActive(false);
        yield return null;
    }

    [PunRPC]
    void RPCCharacterSetAnimator(motionState stateValue)
    {
        katiya_Animator.SetInteger("AnimationState", (int)stateValue);
    }

    [PunRPC]
    void RPCGunSetAnimator(motionState stateValue)
    {
        gun_Animator.SetInteger("GunEnum", (int)stateValue);
    }

}
