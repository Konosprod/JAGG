using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuReplayButton : MonoBehaviour
{
    public void ButtonPress()
    {
        ReplayManager._instance.ShowReplayList();
    }
}
