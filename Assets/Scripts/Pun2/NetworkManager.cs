using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "Player";
    public string roomName = "TestRoom";
    public Vector3 spawnPosition = Vector3.zero;

    void Awake()
    {
        if (FindObjectsOfType<NetworkManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // ✅ FORCE WebSocketSecure cho itch.io (HTTPS)
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ConnectionProtocol.WebSocketSecure; // ← WSS cho HTTPS!
        PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0; // Auto-select secure port
        
        Debug.Log($"[Network] === CONNECTION INFO ===");
        Debug.Log($"[Network] Platform: {Application.platform}");
        Debug.Log($"[Network] Protocol: WebSocketSecure (HTTPS compatible)");
        Debug.Log($"[Network] Fixed Region: asia");
        Debug.Log($"[Network] Game Version: 1.0.0");
        Debug.Log($"[Network] Connecting to Photon...");
        
        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"✅ [Network] === CONNECTED TO MASTER ===");
        Debug.Log($"[Network] Region: {PhotonNetwork.CloudRegion}");
        Debug.Log($"[Network] Server Address: {PhotonNetwork.ServerAddress}");
        Debug.Log($"[Network] Player Count: {PhotonNetwork.CountOfPlayers}");
        Debug.Log($"[Network] Rooms Count: {PhotonNetwork.CountOfRooms}");
        
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        Debug.Log($"[Network] Nickname: {PhotonNetwork.NickName}");
        Debug.Log($"[Network] Attempting to join/create room: '{roomName}'");
        
        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = 10 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"✅ [Network] === JOINED ROOM ===");
        Debug.Log($"[Network] Room Name: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"[Network] Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        Debug.Log($"[Network] Is Master Client: {PhotonNetwork.IsMasterClient}");
        Debug.Log($"[Network] Room is open: {PhotonNetwork.CurrentRoom.IsOpen}");
        Debug.Log($"[Network] Room is visible: {PhotonNetwork.CurrentRoom.IsVisible}");
        
        // List all players
        Debug.Log($"[Network] Players in room:");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"  - {player.NickName} (Actor {player.ActorNumber})");
        }
        
        SpawnPlayer();
    }
    
    public override void OnCreatedRoom()
    {
        Debug.Log($"✅ [Network] Room created: {PhotonNetwork.CurrentRoom.Name}");
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ [Network] Join room FAILED!");
        Debug.LogError($"[Network] Return Code: {returnCode}");
        Debug.LogError($"[Network] Message: {message}");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ [Network] Create room FAILED!");
        Debug.LogError($"[Network] Return Code: {returnCode}");
        Debug.LogError($"[Network] Message: {message}");
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"❌ [Network] === DISCONNECTED ===");
        Debug.LogError($"[Network] Reason: {cause}");
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"👤 [Network] Player joined: {newPlayer.NickName}");
        Debug.Log($"[Network] Total players now: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"👋 [Network] Player left: {otherPlayer.NickName}");
        Debug.Log($"[Network] Total players now: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    void SpawnPlayer()
    {
        Debug.Log($"[Network] === SPAWNING PLAYER ===");
        Debug.Log($"[Network] Prefab name: {playerPrefabName}");
        Debug.Log($"[Network] Spawn position: {spawnPosition}");
        
        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        Vector3 finalPos = spawnPosition + offset;
        
        Debug.Log($"[Network] Final spawn position: {finalPos}");
        
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, finalPos, Quaternion.identity);
        
        if (player != null)
        {
            Debug.Log($"✅ [Network] Player spawned successfully: {player.name}");
            Debug.Log($"[Network] PhotonView ID: {player.GetComponent<PhotonView>()?.ViewID}");
        }
        else
        {
            Debug.LogError($"❌ [Network] Failed to spawn player!");
        }
    }
}