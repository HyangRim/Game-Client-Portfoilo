using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TalkData
{
    public string eventName;
    public int eventCode;
    public string name;
    public string[] contexts;
}
public class Dialogue : MonoBehaviour
{
    //Dialogue Manager Variable
    [SerializeField]
    public string eventname = null;
    [SerializeField]
    TalkData[] talkDatas = null;


    public TalkData[] GetObjectDialogue()
    {
        return DialogueParse.GetDialogue(eventname);
    }
}
