using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine;

// [RequireComponent(typeof(RangeWeaponController))]
public class Heize : CharacterBase
{

    // RangeWeaponController rangeWeaponController;

    // public RangeWeapon heizeRocketLauncher;

    // public RangeWeapon heizeBasicWeapon;

    // 로켓런처 탄환
    public Projectile rocketLauncherProjectile;

    // 무기스왑 이펙트
    public GameObject rocketLauncherSwapWind;

    // 기본공격 이펙트
    public ParticleSystem ShotgunMuzzle;

    // 로켓런쳐 이펙트
    public ParticleSystem rocketLauncherMuzzle;

    // 기본공격 탄환
    public Projectile basicProjectile;

    public Skill rocketLauncherSkill;

    AttackMode currentAttackMode;
    HazeAnimationState currentAnimation;


    // UI 캔버스의 오른쪽 마우스 버튼 스킬의 아이콘
    public Image rightMouseButtonSkillImage;

    // UI 캔버스의 왼쪽 초상화 아이콘.
    public Image leftPortrait;
    [SerializeField] private Sprite thisPortrait;

    // 스킬 쿨타임 적용 이미지 
    public Image rightMouseBtnSkillHideImage;

    // 쿨타임 텍스트    
    public TMP_Text rightMouseBtnSkillCooldownText;



    // 이미지 컬러
    public Color color;

    // 쿨타임때의 컬러
    public Color coolTimeColor;


    // 다음 모드 전환 가능 시간
    float nextRocketLauncherTime;

    // 모드 수동 전환 가능 시간
    float nextModeChangeTime;

    // 로켓모드 끝나는 시각
    float offRocketLauncherTime;

    // 로켓모드 유지시간
    float maintainRocketModeTime = 10;


    // 남아있는 로켓 수
    public int remainRocket = 0;

    //0 : Haze, 1 : Haze Gun, 2 : Haze Rocket;
    public Animator[] hazeAnimator;

    // 0 : Gun, 1 : Rocket;
    public GameObject[] hazeWeapon;

    //움직임 보정용 Transform, 벡터들. 
    [SerializeField]
    private Transform HazeModelTransform;

    private Vector3 originalPos, originalRot;
    private float idleElapsedTime = 0f;



    public enum AttackMode { Classic, RocketLauncher };

    //FOR Animation.
    public enum HazeAnimationState
    {
        NormalIdle = 0,
        NormalWalk = 1,
        NormalAttack = 2,
        ETrans = 3,
        EIdle = 4,
        EWALK = 5,
        EATTACK = 6
    };
    protected override void Start()
    {
        base.Start();
        // rangeWeaponController = GetComponent<RangeWeaponController>();
        currentAnimation = HazeAnimationState.NormalIdle;
        currentAttackMode = AttackMode.Classic;
        basicProjectile = projectile;

        rocketLauncherSwapWind = transform.Find("WindVFXGraph").gameObject;

        ShotgunMuzzle = transform.Find("ShotgunEffect").gameObject.GetComponent<ParticleSystem>();


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
            rightMouseButtonSkillImage.sprite = rocketLauncherSkill.skillIcon;


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
        speed = 3.2f;

        // 캐릭터 체력
        health = 120;
        maxHealth = 120;

        // 투사체 이름(포톤 네트워크 오브젝트 생성을 위해서)
        projectileName = "HeizeProjectile";

        // 캐릭터 체력바 적용
        hpBar.SetFloat("_offset", health / maxHealth - 0.5f);
        hpBar.SetFloat("_segmentAmount", maxHealth / 20);

        //애니메이션 움직임 보정용 벡터 값.
        originalPos = Vector3.zero;
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

        if (currentAnimation == HazeAnimationState.NormalIdle || currentAnimation == HazeAnimationState.EIdle)
        {
            idleElapsedTime += Time.deltaTime;
            if (idleElapsedTime > 1f)
            {
                HazeModelTransform.localPosition = originalPos;
                idleElapsedTime = 0f;
            }
        }

        if ((currentState != State.Fainted || currentAnimation == HazeAnimationState.NormalIdle) && currentState != State.Casting)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                audioManager.playSFX(11);
                if (currentAttackMode == AttackMode.Classic)
                {
                    if (hazeAnimator[0].GetInteger("AnimationState") != (int)HazeAnimationState.NormalWalk)
                    {
                        if (!GameManager.instance.isPvp)
                        {
                            hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.NormalWalk);
                            hazeAnimator[1].SetInteger("AnimationState", (int)HazeAnimationState.NormalWalk);
                        }
                        else
                        {
                            photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, HazeAnimationState.NormalWalk);
                            photonView.RPC("RPCGunSetAnimator", RpcTarget.All, HazeAnimationState.NormalWalk);
                        }
                        currentAnimation = HazeAnimationState.NormalWalk;
                        StartCoroutine(animationIdleTrans(0.833f, true));
                    }
                }
                if (currentAttackMode == AttackMode.RocketLauncher)
                {
                    if (hazeAnimator[0].GetInteger("AnimationState") != (int)HazeAnimationState.EWALK)
                    {
                        if (!GameManager.instance.isPvp)
                        {
                            hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.EWALK);
                            hazeAnimator[2].SetInteger("AnimationState", (int)HazeAnimationState.EWALK);
                        }
                        else
                        {
                            photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.EWALK);
                            photonView.RPC("RPCRocketSetAnimator", RpcTarget.All, (int)HazeAnimationState.EWALK);
                        }
                        currentAnimation = HazeAnimationState.EWALK;
                        StartCoroutine(animationIdleTrans(0.2783f, true));
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            idleElapsedTime = 0f;
            if (currentAttackMode == AttackMode.Classic && currentState != State.Casting && currentState != State.Fainted)
            {
                if (Time.time > nextShotTime)
                {
                    BasicShoot();
                    if (!GameManager.instance.isPvp)
                    {
                        hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.NormalAttack);
                        hazeAnimator[1].SetInteger("AnimationState", (int)HazeAnimationState.NormalAttack);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, HazeAnimationState.NormalWalk);
                        photonView.RPC("RPCGunSetAnimator", RpcTarget.All, HazeAnimationState.NormalWalk);
                    }

                    audioManager.playSFX(3001);

                    StartCoroutine(animationIdleTrans(0.833f, true));
                    HasCasting(0.833f);
                }
            }
            else if (currentAttackMode == AttackMode.RocketLauncher && currentState != State.Casting && currentState != State.Fainted)
            {
                if (Time.time > nextShotTime)
                {
                    if (!rocketLauncherMuzzle.gameObject.activeSelf)
                        rocketLauncherMuzzle.gameObject.SetActive(true);
                    rocketLauncherMuzzle.Play();
                    Shoot();
                    nextShotTime = Time.time + 1 / attackPerSec;
                    remainRocket--;
                    if (!GameManager.instance.isPvp)
                    {
                        hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.EATTACK);
                        hazeAnimator[2].SetInteger("AnimationState", (int)HazeAnimationState.EATTACK);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.EATTACK);
                        photonView.RPC("RPCRocketSetAnimator", RpcTarget.All, (int)HazeAnimationState.EATTACK);
                    }

                    audioManager.playSFX(3003);

                    StartCoroutine(animationIdleTrans(0.2783f, true));
                    HasCasting(0.2783f);


                    if (remainRocket == 0)
                    {
                        OffRocketLauncher();
                        offRocketLauncherTime = 0;

                        StartCoroutine(RocketLauncherCoolDown());
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && currentState != State.Fainted && currentState != State.Casting)//Rocket Mode Trans
        {
            idleElapsedTime = 0f;
            if (currentAttackMode == AttackMode.Classic) // Classic -> Rocket Mode
            {
                rocketLauncherSwapWind.gameObject.SetActive(true);
                if (Time.time > nextRocketLauncherTime)
                {
                    OnRocketLauncher();
                    StartCoroutine(RocketLauncherMaintainDown());
                    if (!GameManager.instance.isPvp)
                    {
                        hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.ETrans);
                        hazeAnimator[2].SetInteger("AnimationState", (int)HazeAnimationState.ETrans);
                    }
                    else
                    {
                        photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.ETrans);
                        photonView.RPC("RPCRocketSetAnimator", RpcTarget.All, (int)HazeAnimationState.ETrans);
                    }


                    audioManager.playSFX(3002);

                    StartCoroutine(animationIdleTrans(1.0425f, true));
                    HasCasting(1.0425f);
                }
            }
            else if (currentAttackMode == AttackMode.RocketLauncher && Time.time > nextModeChangeTime) // RocketMode -> Classic
            {
                rocketLauncherSwapWind.gameObject.SetActive(false);
                OffRocketLauncher();
                offRocketLauncherTime = 0;
                StartCoroutine(RocketLauncherCoolDown());
                if (!GameManager.instance.isPvp)
                {
                    hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
                    hazeAnimator[1].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
                }
                else
                {
                    photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
                    photonView.RPC("RPCGunSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
                }

                StartCoroutine(animationIdleTrans(0.001f, true));
            }

        }
        // 로켓모드 유지시간 끝났을 때
        if (currentAttackMode == AttackMode.RocketLauncher && Time.time > offRocketLauncherTime)
        {
            OffRocketLauncher();
            StartCoroutine(RocketLauncherCoolDown());

            if (!GameManager.instance.isPvp)
            {
                hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
                hazeAnimator[1].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
            }
            else
            {
                photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
                photonView.RPC("RPCGunSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
            }

        }

    }

    // 헤이즈의 기본 공격(산탄)
    void BasicShoot()
    {
        ShotgunMuzzle.Play();

        HazeModelTransform.localPosition = originalPos;
        HazeModelTransform.localRotation = Quaternion.Euler(0f, -65f, 0f);
        Vector3 rightDirection = transform.TransformDirection(new Vector3(0.5f, 0, 1).normalized);
        Vector3 leftDirection = transform.TransformDirection(new Vector3(-0.5f, 0, 1).normalized);


        nextShotTime = Time.time + 1 / attackPerSec;
        projectile.projectileVector = Vector3.forward;

        if (!GameManager.instance.isPvp)
        {
            Instantiate(projectile, transform.position + Vector3.forward, transform.rotation);

            Instantiate(projectile, transform.position + Vector3.forward * 0.6f, Quaternion.LookRotation(rightDirection));

            Instantiate(projectile, transform.position + Vector3.forward * 0.6f, Quaternion.LookRotation(leftDirection));
        }
        else
        {
            PhotonNetwork.Instantiate(projectileName, transform.position + Vector3.forward, transform.rotation);
            PhotonNetwork.Instantiate(projectileName, transform.position + Vector3.forward, Quaternion.LookRotation(rightDirection));
            PhotonNetwork.Instantiate(projectileName, transform.position + Vector3.forward, Quaternion.LookRotation(leftDirection));
        }
    }


    // 로켓모드 온
    void OnRocketLauncher()
    {
        hazeWeapon[1].SetActive(true);
        hazeWeapon[0].SetActive(false);
        currentAnimation = HazeAnimationState.EIdle;
        currentAttackMode = AttackMode.RocketLauncher;
        projectile = rocketLauncherProjectile;
        projectileName = "RocketLauncherProjectile";
        offRocketLauncherTime = Time.time + maintainRocketModeTime;
        nextModeChangeTime = Time.time + rocketLauncherSkill.castingTime;
        nextShotTime = Time.time + rocketLauncherSkill.castingTime;
        speed = 1.5f;
        attackPerSec = 0.7f;
        remainRocket = 4;
        HazeModelTransform.rotation = Quaternion.Euler(0f, -65f, 0f);
    }

    // 로켓모드 오프
    void OffRocketLauncher()
    {
        hazeWeapon[0].SetActive(true);
        hazeWeapon[1].SetActive(false);
        currentAnimation = HazeAnimationState.NormalIdle;
        // 남은 탄환수에 비례해서 쿨타임 감소 적용
        nextRocketLauncherTime = Time.time + rocketLauncherSkill.coolTime - (rocketLauncherSkill.coolTime * remainRocket * 0.2f);
        currentAttackMode = AttackMode.Classic;
        projectile = basicProjectile;
        projectileName = "HeizeProjectile";
        speed = 3.2f;
        attackPerSec = 1.2f;
        remainRocket = 0;

        //Rotation 초기화
        HazeModelTransform.localPosition = originalPos;
        HazeModelTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    // 스킬 쿨타임
    IEnumerator RocketLauncherCoolDown()
    {
        rightMouseBtnSkillCooldownText.gameObject.SetActive(true);

        rightMouseBtnSkillHideImage.fillAmount = 1;

        while (Mathf.Max(0, nextRocketLauncherTime - Time.time) > 0)
        {
            if (rightMouseBtnSkillCooldownText.gameObject.activeSelf == false)
            {
                rightMouseBtnSkillCooldownText.gameObject.SetActive(true);
            }
            // 소수점으로 표현하고 싶을 때 사용하자
            // float remainedTime = Mathf.Max(0, nextRocketLauncherTime - Time.time);

            // 정수형으로만 표현하고 싶을 때
            int remainedTime = Mathf.CeilToInt(nextRocketLauncherTime - Time.time);

            // 쿨타임 텍스트 표시
            rightMouseBtnSkillCooldownText.text = remainedTime.ToString();

            rightMouseButtonSkillImage.color = coolTimeColor;

            // 쿨타임 이미지 표시
            rightMouseBtnSkillHideImage.fillAmount = (nextRocketLauncherTime - Time.time) / rocketLauncherSkill.coolTime;

            yield return null;
        }
        rightMouseBtnSkillCooldownText.gameObject.SetActive(false);

        rightMouseButtonSkillImage.color = color;

        rightMouseBtnSkillHideImage.fillAmount = 0;
    }

    // 로켓런처 유지시간 쿨다운
    IEnumerator RocketLauncherMaintainDown()
    {
        rightMouseBtnSkillCooldownText.gameObject.SetActive(true);

        rightMouseBtnSkillHideImage.fillAmount = 1;

        while (Mathf.Max(0, offRocketLauncherTime - Time.time) > 0)
        {
            // 소수점으로 표현하고 싶을 때 사용하자
            // float remainedTime = Mathf.Max(0, offRocketLauncherTime - Time.time);

            // 정수형으로만 표현하고 싶을 때
            int remainedTime = Mathf.CeilToInt(offRocketLauncherTime - Time.time);

            // 쿨타임 텍스트 표시
            rightMouseBtnSkillCooldownText.text = remainedTime.ToString();

            // 쿨타임 이미지 표시
            rightMouseBtnSkillHideImage.fillAmount = (offRocketLauncherTime - Time.time) / maintainRocketModeTime;

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
        if (currentAttackMode == AttackMode.Classic)
        {
            if (!GameManager.instance.isPvp)
            {
                hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
                hazeAnimator[1].SetInteger("AnimationState", (int)HazeAnimationState.NormalIdle);
            }
            else
            {
                photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
                photonView.RPC("RPCGunSetAnimator", RpcTarget.All, (int)HazeAnimationState.NormalIdle);
            }
            currentAnimation = HazeAnimationState.NormalIdle;
            HazeModelTransform.localPosition = originalPos;
            HazeModelTransform.localRotation = Quaternion.Euler(originalRot);
        }
        else if (currentAttackMode == AttackMode.RocketLauncher)
        {
            if (!GameManager.instance.isPvp)
            {
                hazeAnimator[0].SetInteger("AnimationState", (int)HazeAnimationState.EIdle);
                hazeAnimator[2].SetInteger("AnimationState", (int)HazeAnimationState.EIdle);
            }
            else
            {
                photonView.RPC("RPCCharacterSetAnimator", RpcTarget.All, (int)HazeAnimationState.EIdle);
                photonView.RPC("RPCRocketSetAnimator", RpcTarget.All, (int)HazeAnimationState.EIdle);
            }
            currentAnimation = HazeAnimationState.EIdle;
            HazeModelTransform.localPosition = originalPos;
            HazeModelTransform.localRotation = Quaternion.Euler(0f, -65f, 0f);
        }

        currentState = State.Idle;
    }

    [PunRPC]
    void RPCCharacterSetAnimator(motionState stateValue)
    {
        hazeAnimator[0].SetInteger("AnimationState", (int)stateValue);
    }

    [PunRPC]
    void RPCGunSetAnimator(motionState stateValue)
    {
        hazeAnimator[1].SetInteger("AnimationState", (int)stateValue);
    }

    [PunRPC]
    void RPCRocketSetAnimator(motionState stateValue)
    {
        hazeAnimator[2].SetInteger("AnimationState", (int)stateValue);
    }


}
