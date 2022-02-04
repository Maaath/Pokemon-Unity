using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterMenu : MonoBehaviour
{
    [SerializeField] private Text nickName;

    public void CreateRoom()
    {
        NetworkManager.Instance.ChangeNick(nickName.text);
        NetworkManager.Instance.CreateRoom("testRoom");
    }

    public void EnterRoom()
    {
        NetworkManager.Instance.ChangeNick(nickName.text);
        NetworkManager.Instance.EnterRoom("testRoom");
    }
}
