using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab (Resources/Player.prefab)")]
    public string playerPrefabName = "Player";

    [Header("Room")]
    public string roomName = "TestRoom";

    [Header("Spawn")]
    public Vector3 spawnPosition = Vector3.zero;

    void Start()
    {
        Debug.Log("🔌 Connecting to Photon...");
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Master");

        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
        }

        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = 10 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"🏠 Joined room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"👥 Player count: {PhotonNetwork.CurrentRoom.PlayerCount}");

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Debug.Log("🚀 TRY SPAWN");

        Vector3 offset = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            0
        );

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPosition + offset,
            Quaternion.identity
        );

        Debug.Log("✅ SPAWN OK: " + player.GetPhotonView().ViewID);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ JoinRoom FAILED | {returnCode} | {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ CreateRoom FAILED | {returnCode} | {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("❌ Disconnected: " + cause);
    }
}
