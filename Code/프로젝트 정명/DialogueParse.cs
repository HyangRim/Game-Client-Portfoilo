using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using UnityEngine;

public class DialogueParse : MonoBehaviour
{
    private static Dictionary<string, TalkData[]> DialogueDictionary = new Dictionary<string, TalkData[]>();

    //����׷� �ν����� â���� ���� ���� ��. 
    [SerializeField]
    List<DebugTalkData> talkDataList = new List<DebugTalkData>();

    [SerializeField]
    private TextAsset csvFile = null;


    // 0 : Morosa, 1 : Kinato

    // 0 : Morosa_KR, 1 : Kinato_KR, 2 : Morasa_EN, 3 : Kinato_EN, 4 : Morosa_JP, 5 : Kinato_JP
    [SerializeField]
    private TextAsset[] eachCharacter = null;

    [SerializeField]
    private PlayerStatus playerStatus;

    [SerializeField]
    private PlayerSettings playerSettings;


    // 0 : Morosa, 1 : Kinato
    int characterNum = 0;

    // 0 : KR , 1 : EN, 2 : JP
    int countryNum = 0;

    private void Start()
    {

        //SetDebugTalkData();
        //
        if(GameObject.Find("BattleManager") != null) {
            if (GameObject.Find("BattleManager").GetComponent<BattleManager>().isBossScene == 0) return;
        }
        if (GameObject.Find("PlayerInfo") != null)
        {
            playerSettings = GameObject.Find("PlayerInfo").GetComponent<PlayerSettings>();
        }
        if (playerStatus != null)
        {
            //characterNum
            characterNum = playerStatus.playerdata.character;
            csvFile = eachCharacter[characterNum];

        }

        if (playerSettings != null)
        {
            characterNum = playerSettings.PlayDataInstance.character;
            countryNum = playerSettings.PlayDataInstance.countryCode;


            //characterNum -> 0 : Morosa , 1 : Kinato 
            //countryCode -> 0 : KR, 1 : EN , 2 : JP

            //ex ) JP_Kinato -> 2(JP CN CODE) * 1 + 1 = 5 
            csvFile = eachCharacter[countryNum * 2 + characterNum];
        }


        SetTalkDictionary();
    }


    public void SetTalkDictionary()
    {
        string csvText = csvFile.text.Substring(0, csvFile.text.Length - 1);
        string[] datas = csvText.Split(new char[] { '\n' });


        for (int i = 1; i < datas.Length; i++)
        {
            string[] rowValues = datas[i].Split(new char[] { ',' });

            if (rowValues[0].Trim() == "" || rowValues[0].Trim() == "End")
                continue;

            List<TalkData> talkDataList = new List<TalkData>();
            string eventName = rowValues[0];

            while (rowValues[0].Trim() != "End")
            {
                List<string> contextList = new List<string>();
                TalkData talkData = new TalkData();
                talkData.eventName = eventName;
                talkData.eventCode = int.Parse(rowValues[1]);
                talkData.name = rowValues[2];
                do
                {
                    contextList.Add(rowValues[3].ToString());
                    if (++i < datas.Length)
                        rowValues = datas[i].Split(new char[] { ',' });
                    else
                        break;
                } while (rowValues[1] == "" && rowValues[0] != "End");

                talkData.contexts = contextList.ToArray();
                //UnityEngine.Debug.Log(i + "��° : " + rowValues[3]);
                talkDataList.Add(talkData);
            }
            //UnityEngine.Debug.Log("�̺�Ʈ ���� : " + eventName);
            //if(DialogueDictionary.f)
            if (!DialogueDictionary.ContainsKey(eventName))
            {
                //UnityEngine.Debug.Log(eventName + "���!!");
                DialogueDictionary.Add(eventName, talkDataList.ToArray());
            }
        }
    }

    public static TalkData[] GetDialogue(string eventName)
    {
        return DialogueDictionary[eventName];
    }

    void SetDebugTalkData()
    {
        List<string> eventNames = new List<string>(DialogueDictionary.Keys);

        List<TalkData[]> talkDatasList = new List<TalkData[]>(DialogueDictionary.Values);

        for (int i = 0; i < eventNames.Count; i++)
        {
            DebugTalkData debugTalk = new DebugTalkData(eventNames[i], talkDatasList[i]);
            //DebugTalkData.Add(debugTalk);
        }
    }

}
