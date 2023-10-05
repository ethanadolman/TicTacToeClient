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
        var msg = NewUserCheckbox.isOn ? $"{ClientToServerSignifiers.createAccount},{UsernameField.text},{PasswordField.text}" : $"{ClientToServerSignifiers.Login},{UsernameField.text},{PasswordField.text}";

        // Send the serialized credentials to the server
        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void SetFeedbackText(string msg)
    {
        switch (gameState)
        {
            case GameState.Login:
                LoginFeedback.text = msg;
                break;
            case GameState.Lobby:
                LobbyFeedback.text = msg;
                break;
            case GameState.WaitingRoom:
                WaitingRoomFeedback.text = msg;
                break;
            default:
                break;
        }
    }

    public void ProcessGameRoom()
    {
        var msg = $"{ClientToServerSignifiers.findGame},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessGameStart()
    {
        var msg = $"{ClientToServerSignifiers.gameStart},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
    }

    public void ProcessLeaveWaitingRoom()
    {
        var msg = $"{ClientToServerSignifiers.leaveWaitingRoom},{GameRoomField.text}";

        NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
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
                RoomName.text = WaitingRoomName.text;
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
}


