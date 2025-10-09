using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI talkText;
    public TextMeshProUGUI talkName;
    public GameObject talkPanel;
    public Dialogue talkDialogue;




    TalkData[] talkDatas = null;

    //0 : None, 1 : True, 2 : Last
    public int isTalk;
    public int talkIndex = 0;
    public Image[] talkRenderer;


    // 0 : Morosa, 1 : Kinato
    // Main character Image.

    //캐릭터 코드 0 : Morosa , 1 : Kinato
    public int charCode;
    // 0 : 기본 표정 , 1 : 웃는 표정 , 2 : 불만 표정
    public Sprite[] morosaImage;
    public Sprite[] kinatoImage;

    //enemy Image
    public Sprite[] enemyImage;


    //Talk Start Animation.
    public bool isTalkAnimation = false;
    [SerializeField] private Image talkPanelImage;
    [SerializeField] public bool battleTag = false;

    //0 : Morosa, 1 : Kinato
    public int characterCode;
    public int beforeDialogueCode = -1;

    public GameObject scanObject;

    [SerializeField]
    public string nextScene;


    //캐릭터 별 표정 다르게 하기위한 코드. 
    [SerializeField]
    private int talkLength;
    [SerializeField]
    public int dialogueIDX;

    [SerializeField]
    private int[] morosaChar_DialogueCode;
    [SerializeField]
    private int[] m_enemyChar_DialogueCode;



    [SerializeField]
    private int[] kinatoChar_DialogueCode;
    [SerializeField]
    private int[] k_enemyChar_DialogueCode;



    //3면 유령같이 말만 하는 NPC를 위한 dialogueIDX;
    [SerializeField]
    private int justNPCIDX;

    [SerializeField]
    private int[] m_justNPCCode;
    [SerializeField]
    private int[] k_justNPCCode;


    //현재 캐릭터 데이터 가지고 있는 것. 
    public PlayerData playerdata;

    
    //For Audio.
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private bool isEndingScene = false;

    //캐릭터 엔딩씬에서 사용될 때만 true인 변수.
    [SerializeField] private bool isBeforeEndingCredit = false;



    private void Awake()
    {
        talkPanel.SetActive(false);
        if (GameObject.Find("PlayerInfo") != null)
        {
            playerdata = GameObject.Find("PlayerInfo").GetComponent<PlayerSettings>().PlayDataInstance;

            //0 : Morosa, 1 : Kinato
            charCode = playerdata.character;
        }

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

    }

    void Start()
    {
        dialogueIDX = 0;
        for (int spriteIndex = 0; spriteIndex < talkRenderer.Length; spriteIndex++)
        {
            talkRenderer[spriteIndex].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (battleTag == true)
        {
            if (Input.GetKeyDown(KeyCode.X)) Action();
        }
    }


    public void Action(GameObject scanObj)
    {
        audioManager.playSFX(201);

        if (isTalk == 2)
        {
            isTalk = 0;
            talkIndex = 0;
            talkPanel.SetActive(false);
            isTalkAnimation = false;
            //Talk End.

            //Sprite꺼서 대화 끝. 
            for (int spriteIndex = 0; spriteIndex < talkRenderer.Length; spriteIndex++)
            {
                talkRenderer[spriteIndex].gameObject.SetActive(false);
            }
            //전투로 넘어가기. 

            //일반 NPC면 그냥 전투로 안넘어감.
            if (scanObj.CompareTag("JUSTNPC"))
            {
                justNPCIDX = 0;
                return;
            }

            audioManager.pauseBGM();
            audioManager.playSFX(2101);

            FieldGameManager.sceneChange = 2;
            //StartCoroutine(LoadAsyncBossScene());
        }
        else if (isTalk == 1)
        {
            DialogueUpdate(talkIndex);
        }
        else if (isTalk == 0)
        {
            StartCoroutine(TalkStartAnimation());
            isTalkAnimation = true;
            if (isTalkAnimation == false) return;
            //대화 시작. 캐릭터 일러스트 가시화.  
            for (int spriteIndex = 0; spriteIndex < talkRenderer.Length; spriteIndex++)
            {
                talkRenderer[spriteIndex].gameObject.SetActive(true);
            }

            isTalk = 1;
            talkPanel.SetActive(true);
            scanObject = scanObj;

            //대화 불러오기. 
            talkDatas = talkDialogue.GetObjectDialogue();

            if (talkDatas != null)
            {
                //디버그 용도. 
                //DebugDialogue(talkDatas);
            }

            DialogueUpdate(talkIndex);
        }


        if (scanObj.tag == "BossCharacter")
        {
            //Debug.Log(talkDatas.Length + " : " + dialogueIDX);
            changeDialogueImage();
            dialogueIDX++;
        }

        if (scanObj.tag == "JUSTNPC")
        {
            changejustNPCImage();
            justNPCIDX++;
        }
    }

    //Function Overloading...
    //For Boss Script. (Ending...)
    public void Action()
    {
        battleTag = true;
        if (isTalk == 2)
        {
            isTalk = 0;
            talkIndex = 0;
            talkPanel.SetActive(false);
            isTalkAnimation = false;
            //Talk End.

            audioManager.pauseBGM();
            audioManager.playSFX(2102);

            //Debug.Log("BattleEnd = 1");
            BattleManager.battleEnd = 1;

            //Sprite꺼서 대화 끝. 
            for (int spriteIndex = 0; spriteIndex < talkRenderer.Length; spriteIndex++)
            {
                talkRenderer[spriteIndex].gameObject.SetActive(false);
            }
            //전투로 넘어가기. 


            if (!isBeforeEndingCredit)
            {
                StartCoroutine(LoadAsyncBossScene());
            }
        }
        else if (isTalk == 1)
        {
            DialogueUpdate(talkIndex);
        }
        else if (isTalk == 0)
        {
            //대화 시작. 캐릭터 일러스트 가시화.  
            for (int spriteIndex = 0; spriteIndex < talkRenderer.Length; spriteIndex++)
            {
                talkRenderer[spriteIndex].gameObject.SetActive(true);
            }

            isTalk = 1;
            talkPanel.SetActive(true);

            StartCoroutine(TalkStartAnimation());
            //if (isTalkAnimation == false) return;
            //대화 불러오기. 
            talkDatas = talkDialogue.GetObjectDialogue();

            if (talkDatas != null)
            {
                //디버그 용도. 
                //DebugDialogue(talkDatas);
            }

            DialogueUpdate(talkIndex);
        }

        if(isEndingScene == false)
        {
            changeDialogueImage();
        }
        dialogueIDX++;
    }


    void DebugDialogue(TalkData[] talkDatas)
    {
        for (int i = 0; i < talkDatas.Length; i++)
        {
            Debug.Log(talkDatas[i].name);
            foreach (string context in talkDatas[i].contexts)
                Debug.Log(context);
        }
    }

    void DialogueUpdate(int indexcode)
    {
        audioManager.playSFX(201);
        //대사집 이어 붙이기.  
        talkText.text = "";
        foreach (string context in talkDatas[indexcode].contexts)
        {
            talkText.text += context;
        }

        //개행 \\n을 \n으로 오류 수정작업. 
        talkText.text = talkText.text.Replace("\\n", "\n");
        talkText.text = talkText.text.Replace("\"", "");

        //
        string dialogueName = talkDatas[indexcode].eventName;
        int dialogueCode = talkDatas[indexcode].eventCode;
        talkName.text = talkDatas[indexcode++].name;

        //대화의 끝 발견 시에. 
        if (dialogueName == "Ending") isTalk = 2;
        if (indexcode == talkDatas.Length) isTalk = 2;

        if (dialogueCode != beforeDialogueCode)
        {
            DialogueImageControl(dialogueCode);
        }
        beforeDialogueCode = dialogueCode;

        //indexcode로 넣어주기. 
        talkIndex = indexcode;
    }

    void DialogueImageControl(int dialogueCode)
    {
        //주인공(왼쪽이 말하는 중.)
        if (dialogueCode == 1)
        {
            StartCoroutine(FadeInOut(0.3f, 1));
        }
        else if (dialogueCode == 2)//오른쪽이 말하는 중. 
        {
            StartCoroutine(FadeInOut(0.3f, 2));
        }
        else if (dialogueCode == 3)//동시에 말해서 두 이미지 모두 페이드 인. 
        {
            StartCoroutine(FadeInOut(0.3f, 3));
        }
    }


    //캐릭터 대화시에 페이드 인, 페이드 아웃 효과. 
    IEnumerator FadeInOut(float duration, int code)
    {
        float startTime = Time.time;

        //1: 주인공, 2: 적, 3: 동시에. 
        if (code == 1)
        {
            while (Time.time - startTime <= duration)
            {
                float elapsed = Time.time - startTime;
                float normalizedTime = 0.5f + (Mathf.Clamp01(elapsed / duration) / 2);
                float normalizedTime2 = 1f - (Mathf.Clamp01(elapsed / duration) / 2);
                float normalizedTime3 = Mathf.Clamp01(elapsed / duration);

                Color color1 = talkRenderer[0].color;
                Color color2 = talkRenderer[1].color;

                color1.r = normalizedTime;
                color1.g = normalizedTime;
                color1.b = normalizedTime;

                color2.r = normalizedTime2;
                color2.g = normalizedTime2;
                color2.b = normalizedTime2;

                talkRenderer[0].color = color1;
                talkRenderer[1].color = color2;

                //Finish talk Color
                //Start Moving.
                yield return null;
            }
        }
        else if (code == 2)
        {
            while (Time.time - startTime <= duration)
            {
                float elapsed = Time.time - startTime;
                float normalizedTime = 0.5f + (Mathf.Clamp01(elapsed / duration) / 2);
                float normalizedTime2 = 1f - (Mathf.Clamp01(elapsed / duration) / 2);
                float normalizedTime3 = Mathf.Clamp01(elapsed / duration);

                Color color1 = talkRenderer[0].color;
                Color color2 = talkRenderer[1].color;

                color1.r = normalizedTime2;
                color1.g = normalizedTime2;
                color1.b = normalizedTime2;

                color2.r = normalizedTime;
                color2.g = normalizedTime;
                color2.b = normalizedTime;

                talkRenderer[0].color = color1;
                talkRenderer[1].color = color2;

                //Finish talk Color
                //Start Moving.

                yield return null;
            }
        }
        else
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = 0.5f + (Mathf.Clamp01(elapsed / duration) / 2);

            Color color1 = talkRenderer[0].color;
            Color color2 = talkRenderer[1].color;

            color1.r = normalizedTime;
            color1.g = normalizedTime;
            color1.b = normalizedTime;

            color2.r = normalizedTime;
            color2.g = normalizedTime;
            color2.b = normalizedTime;

            talkRenderer[0].color = color1;
            talkRenderer[1].color = color2;

            yield return null;
        }
    }

    IEnumerator LoadAsyncBossScene()
    {
        //Debug.Log("로드 싱크 씬!!");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = false;
        while (true)
        {
            if (asyncLoad.isDone) break;
            if (asyncLoad.progress >= 0.9f) break;
            //Debug.Log("Progress : " + asyncScene.progress);
            yield return null;
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;
        //Debug.Log("대화 끝!!");
    }

    IEnumerator TalkStartAnimation()
    {

        float duration = 0.3f;
        float initHeight = 0f;
        float targetHeight = 1f;

        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float newHeight = Mathf.Lerp(initHeight, targetHeight, elapsedTime / duration);
            talkPanelImage.fillAmount = newHeight;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        talkPanelImage.fillAmount = targetHeight;
        isTalkAnimation = true;
        yield return null;
    }


    void changeDialogueImage()
    {
        if (dialogueIDX >= talkDatas.Length) return;
        if (charCode == 0)//Morosa
        {
            //일단 보스 부터.
            if (enemyImage[m_enemyChar_DialogueCode[dialogueIDX]] != null)
            {
                talkRenderer[1].sprite = enemyImage[m_enemyChar_DialogueCode[dialogueIDX]];
            }
            //그다음 메인 캐릭터. 
            if (morosaImage[morosaChar_DialogueCode[dialogueIDX]] != null)
            {
                talkRenderer[0].sprite = morosaImage[morosaChar_DialogueCode[dialogueIDX]];
            }
        }
        else if (charCode == 1)//Kinato
        {
            //일단 보스 부터.
            if (enemyImage[k_enemyChar_DialogueCode[dialogueIDX]] != null)
            {
                talkRenderer[1].sprite = enemyImage[k_enemyChar_DialogueCode[dialogueIDX]];
            }
            if (kinatoImage[kinatoChar_DialogueCode[dialogueIDX]] != null)
            {
                talkRenderer[0].sprite = kinatoImage[kinatoChar_DialogueCode[dialogueIDX]];
            }
        }
    }


    void changejustNPCImage()
    {
        if (justNPCIDX >= talkDatas.Length) return;

        /*
         * 
         *     [SerializeField]
    private int justNPCIDX;

    [SerializeField]
    private int[] m_justNPCCode;
    [SerializeField]
    private int[] k_justNPCCode;
         * */
        if (charCode == 0)//Morosa.
        {
            if (morosaImage[m_justNPCCode[justNPCIDX]] != null)
            {
                talkRenderer[0].sprite = morosaImage[m_justNPCCode[justNPCIDX]];
            }
        }
        else if (charCode == 1)//Kinasi.
        {
            if (kinatoImage[k_justNPCCode[justNPCIDX]] != null)
            {
                talkRenderer[0].sprite = kinatoImage[k_justNPCCode[justNPCIDX]];
            }
        }
    }
}
