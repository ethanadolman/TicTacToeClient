using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    [SerializeField] private GameObject Login;
    [SerializeField] private InputField UsernameField;
    [SerializeField] private InputField PasswordField;
    [SerializeField] private Toggle NewUserCheckbox;
    [SerializeField] private Text LoginFeedback;

    [SerializeField] private GameObject Lobby;
    [SerializeField] private InputField GameRoomField;
    [SerializeField] private Text LobbyFeedback;

    [SerializeField] private GameObject WaitingRoom;
    [SerializeField] private Text WaitingRoomName;
    [SerializeField] private Text WaitingRoomFeedback;
    [SerializeField] private Text WaitingRoomHostName;
    [SerializeField] private Text WaitingRoomHostMessage;
    [SerializeField] private Text WaitingRoomClientName;
    [SerializeField] private Text WaitingRoomClientMessage;
    [SerializeField] private InputField WaitingRoomMessageField;
    [SerializeField] private Button WaitingRoomSendMessageButton;
    [SerializeField] private Button StartGameButton;

    [SerializeField] private GameObject Game;
    [SerializeField] private Text RoomName;
    [SerializeField] private Text GameFeedback;
    [SerializeField] private Text GameHostName;
    [SerializeField] private Text GameHostMessage;
    [SerializeField] private Text GameClientName;
    [SerializeField] private Text GameClientMessage;
    [SerializeField] private InputField GameMessageField;
    [SerializeField] private Button GameSendMessageButton;
    [SerializeField] private Button LeaveGameButton;
    [SerializeField] private List<GameObject> Tiles;
    [SerializeField] private Sprite X;
    [SerializeField] private Sprite O;

    public enum GameState { Login, Lobby, WaitingRoom, InGame, Paused, GameOver }
    private GameState gameState;

    public bool isHost;
    public bool isObserver;
    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this);
        SetState(GameState.Login);
        
    }

    void Update()
    {

    }

    public void ProcessLogin()
    {
        var msg = NewUserCheckbox.isOn ? $"{ClientToServerSignifiers.CreateAccount},{UsernameField.text},{PasswordField.text}" : $"{ClientToServerSignifiers.Login},{UsernameField.text},{PasswordField.text}";

        // Send the serialized credentials to the server
        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }


    public void SetFeedbackText(string msg, Color color)
    {
        switch (gameState)
        {
            case GameState.Login:
                LoginFeedback.text = msg;
                LoginFeedback.color = color;
                break;
            case GameState.Lobby:
                LobbyFeedback.text = msg;
                LobbyFeedback.color = color;
                break;
            case GameState.WaitingRoom:
                WaitingRoomFeedback.text = msg;
                WaitingRoomFeedback.color = color;
                break;
            case GameState.InGame:
                GameFeedback.text = msg;
                GameFeedback.color = color;
                break;
            default:
                break;
        }
    }

    public void SetPlayerMessageText(string msg, bool isHostMessage)
    {
        switch (gameState)
        {
            case GameState.WaitingRoom:
                if (isHostMessage) WaitingRoomHostMessage.text = msg;
                else WaitingRoomClientMessage.text = msg;
                break;
            case GameState.InGame:
                if (isHostMessage) GameHostMessage.text = msg;
                else GameClientMessage.text = msg;
                break;
            default:
                break;
        }
    }

    public void SetPlayerNameTexts(string host = "", string client = "")
    {
        switch (gameState)
        {
            case GameState.WaitingRoom:
                WaitingRoomHostName.text = host;
                WaitingRoomClientName.text = client;
                break;
            case GameState.InGame:
                GameHostName.text = host;
                GameClientName.text = client;
                break;
            default:
                break;
        }
    }

    public void ProcessGameRoom()
    {
        var msg = $"{ClientToServerSignifiers.FindGame},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessGameStart()
    {
        var msg = $"{ClientToServerSignifiers.GameStart},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessLeaveWaitingRoom()
    {
        var msg = $"{ClientToServerSignifiers.LeaveWaitingRoom},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessGameMove(int tile)
    {
        var msg = $"{ClientToServerSignifiers.GameMove},{GameRoomField.text},{tile}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessLeaveGame()
    {
        var msg = $"{ClientToServerSignifiers.LeaveGame},{GameRoomField.text}";
        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessSendMessage()
    {
        var msg = "";
        switch (gameState)
        {
            case GameState.WaitingRoom:
                msg = $"{ClientToServerSignifiers.PlayerMessage},{GameRoomField.text}, {WaitingRoomMessageField.text}";
                WaitingRoomMessageField.text = "";
                break;
            case GameState.InGame:
                msg = $"{ClientToServerSignifiers.PlayerMessage},{GameRoomField.text}, {GameMessageField.text}";
                GameMessageField.text = "";
                break;
            default:
                print("ERROR: INVALID MESSAGE SEND PROTOCOL");
                return;
        }

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }


    public void ForceReturnToLobby()
    {
        StartCoroutine(ReturnToLobbyCoroutine());
    }
    public IEnumerator ReturnToLobbyCoroutine()
    {
        LeaveGameButton.interactable = false;
        SetFeedbackText("Opponent left the match. Returning to lobby in 3", Color.black);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match. Returning to lobby in 2", Color.black);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match. Returning to lobby in 1", Color.black);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match.  Returning to lobby in 0", Color.black);
        SetPlayerNameTexts();
        SetPlayerMessageText("", false);
        SetPlayerMessageText("", true);
        SetState(GameState.Lobby);
        
    }

    public void SetState(GameState state)
    {
        gameState = state;
        UpdateStateScreen();
    }

    private void UpdateStateScreen()
    {
        switch (gameState)
        {
            case GameState.Login:
                LoginFeedback.text = "";
                Game.SetActive(false);
                Login.SetActive(true);
                Lobby.SetActive(false);
                WaitingRoom.SetActive(false);
                break;
            case GameState.Lobby:
                LobbyFeedback.text = "";
                Game.SetActive(false);
                Login.SetActive(false);
                Lobby.SetActive(true);
                WaitingRoom.SetActive(false);
                ResetTiles();
                break;
            case GameState.WaitingRoom:
                StartGameButton.interactable = isHost;
                WaitingRoomSendMessageButton.interactable = !isObserver;
                WaitingRoomName.text = GameRoomField.text;
                WaitingRoom.SetActive(true);
                Game.SetActive(false);
                Login.SetActive(false);
                Lobby.SetActive(false);
                break;
            case GameState.InGame:
                LeaveGameButton.interactable = true;
                GameSendMessageButton.interactable = !isObserver;
                RoomName.text = WaitingRoomName.text;
                GameFeedback.text = "";
                Game.SetActive(true);
                Login.SetActive(false);
                Lobby.SetActive(false);
                WaitingRoom.SetActive(false);
                break;
            case GameState.Paused:
                break;
            case GameState.GameOver:
                break;

        }
    }

   public void SetTile(int tile, bool isX)
   {
       Tiles[tile].GetComponent<Image>().sprite = isX ? X : O;
   }

   public void ResetTiles()
   {
       foreach (var tile in Tiles)
       {
           tile.GetComponent<Image>().sprite = null;
       }
   }
}


