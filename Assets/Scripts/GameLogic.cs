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
    [SerializeField] private Button StartGameButton;

    [SerializeField] private GameObject Game;
    [SerializeField] private Text RoomName;
    [SerializeField] private Text GameFeedback;
    [SerializeField] private Button LeaveGameButton;
    [SerializeField] private List<GameObject> Tiles;
    [SerializeField] private Sprite X;
    [SerializeField] private Sprite O;

    public enum GameState { Login, Lobby, WaitingRoom, InGame, Paused, GameOver }
    private GameState gameState;

    public bool isHost;
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

    public void ForceReturnToLobby()
    {
        StartCoroutine(ReturnToLobbyCoroutine());
    }
    public IEnumerator ReturnToLobbyCoroutine()
    {
        LeaveGameButton.interactable = false;
        SetFeedbackText("Opponent left the match. You Win! Returning to lobby in 3", Color.green);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match. You Win! Returning to lobby in 2", Color.green);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match. You Win! Returning to lobby in 1", Color.green);
        yield return new WaitForSeconds(1);
        SetFeedbackText("Opponent left the match. You Win! Returning to lobby in 0", Color.green);
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
                WaitingRoomName.text = GameRoomField.text;
                WaitingRoom.SetActive(true);
                Game.SetActive(false);
                Login.SetActive(false);
                Lobby.SetActive(false);
                break;
            case GameState.InGame:
                LeaveGameButton.interactable = true;
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


