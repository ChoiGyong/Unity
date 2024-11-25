using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 라이브러리
using UnityEngine.SceneManagement; // 씬 관리 관련 라이브러리

public class GyungGameManager : MonoBehaviourPunCallbacks {
    // 싱글턴 접근용 프로퍼티
    public static GyungGameManager instance
    {
        get
        {
            // 만약 싱글턴 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아서 할당
                m_instance = FindObjectOfType<GyungGameManager>();
            }
            //싱글턴 오브젝트 반환
            return m_instance;
        }
    }

    private static GyungGameManager m_instance; // 싱글턴이 할당될 static 변수
    public GameObject playerPrefeb; // 생성할 플레이어 캐릭터 프리펩
    public bool isGameover {get; private set;} // 게임오버 상태
    private GyungLobbyManager nickname;


    private void Awake() {
        // 씬에 싱글턴 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    // 게임 시작과 동시에 플레이어가 될 게임 오브젝트 생성
    private void Start() {
        // 생성할 랜덤 위치 지정
        Vector3 randomSpawnPos = new Vector3(13,0,18);//Random.insideUnitSphere * 5f;
        // 위치 y 값은 0으로 변경
        randomSpawnPos.y = 2f;
        // 네트워크상의 모든 클라이언트에서 생성 실행
        // 해당 게임 오브젝트의 주도권은 생성 메서드를 직접 실행한 클라이언트에 있음
        GameObject _playerPrefeb = PhotonNetwork.Instantiate(playerPrefeb.name, randomSpawnPos, Quaternion.identity);

        // Photon의 LocalPlayer 닉네임 전달
        string playerNickname = PhotonNetwork.NickName;
        Debug.Log("Player nickname: " + playerNickname);
        
        _playerPrefeb.GetComponent<GyungPlayerSetup>().IsLocalPlayer();
        _playerPrefeb.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, playerNickname);
    }
    // 현재 게임을 게임 오버 상태로 변경하는 메서드
    public void EndGame() {
        // 현재 상태를 게임 오버 상태로 전환
        isGameover = true;
        // 게임오버 UI 활성화
        //UIManager.instance.SetActiveGameoverUI(true);
    }
    // 키보드 입력을 감지하고 룸을 나가게 함
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    // 룸을 나갈 때 자동 실행되는 메서드
    public override void OnLeftLobby()
    {
        // 룸을 나가면 로비 씬으로 돌아감
        SceneManager.LoadScene("Lobby");
    }
}