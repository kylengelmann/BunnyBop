using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly:CheatSystem.CheatClass(typeof(CheatTest))]

public class CheatTest : MonoBehaviour
{
    [CheatSystem.Cheat("PrintSomeStuff")]
    void Testing(int num, string punc = "?")
    {
        Debug.Log("Printing a number" + num + punc);
    }

    [CheatSystem.Cheat("PrintLog")]
    public static void CheatLog(string log)
    {
        Debug.Log(log);
    }
}
