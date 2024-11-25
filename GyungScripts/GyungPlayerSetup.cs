using Photon.Pun; // 유니티용 포톤 컴포넌트들
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GyungPlayerSetup : MonoBehaviour
{
    public GyungPlayerMovement movement;

    public GameObject camera;
    
    public TextMeshPro nicknameText;

    public void IsLocalPlayer()
    {
        movement.enabled = true;
        camera.SetActive(true);
        // Photon에서 닉네임 가져오기
        if (PhotonNetwork.LocalPlayer != null)
        {
            SetNickname(PhotonNetwork.LocalPlayer.NickName);
        }
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        Debug.Log("Setting nickname: " + _name);
        nicknameText.text = _name;
    }
}
