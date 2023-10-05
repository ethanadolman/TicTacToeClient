using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{
    //
    //
    // ON DESTROY IT NEEDS TO SEND INFO TO DESTROY ROOM IF NEEDED
    // ALSO NEED TO HAVE SERVER CONNECT ID WITH USER SO IT CAN ALSO TELL SERVER IT IS LOGGED OUT.
    //
    //
    #region Send and Receive Data Functions
    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);


        switch (signifier)
        {
            case ServerToClientSignifiers.FormatError:
            case ServerToClientSignifiers.LoginFail:
            case ServerToClientSignifiers.UserNotExist:
            case ServerToClientSignifiers.UsernameInUse:
                gameLogic.SetFeedbackText(csv[1]);
                break;
            case ServerToClientSignifiers.LoginSuccess:
            case ServerToClientSignifiers.ReturnToLobby:
                gameLogic.SetState(GameLogic.GameState.Lobby);
                break;
            case ServerToClientSignifiers.NewGameRoom:
                gameLogic.isHost = true;
                gameLogic.SetState(GameLogic.GameState.WaitingRoom);
                gameLogic.SetFeedbackText("(1/2) Awaiting Client");
                break;
            case ServerToClientSignifiers.GameRoomFound:
                gameLogic.isHost = false;
                gameLogic.SetState(GameLogic.GameState.WaitingRoom);
                gameLogic.SetFeedbackText("(2/2) Waiting for Host to start Game");
                break;
            case ServerToClientSignifiers.GameRoomFull:
                gameLogic.SetFeedbackText("Game room with that name is full!");
                break;
            case ServerToClientSignifiers.ClientJoined:
                gameLogic.SetFeedbackText("(2/2) Client has joined. Press Play to begin");
                break;
            case ServerToClientSignifiers.ClientLeft:
                gameLogic.SetFeedbackText("(1/2) Client has left. Awaiting Client");
                break;
            case ServerToClientSignifiers.GameStartSuccess:
                gameLogic.SetState(GameLogic.GameState.InGame);
                break;
            case ServerToClientSignifiers.GameStartFail:
                gameLogic.SetFeedbackText("(1/2) Game Cannot start without another user");
                break;
            default:
                break;
        }

    }

    static public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    #endregion

    #region Connection Related Functions and Events
    static public void ConnectionEvent()
    {
        Debug.Log("Network Connection Event!");
    }
    static public void DisconnectionEvent()
    {
        Debug.Log("Network Disconnection Event!");
    }
    static public bool IsConnectedToServer()
    {
        return networkClient.IsConnected();
    }
    static public void ConnectToServer()
    {
        networkClient.Connect();
    }
    static public void DisconnectFromServer()
    {
        networkClient.Disconnect();
    }

    #endregion

    #region Setup
    static NetworkClient networkClient;
    static GameLogic gameLogic;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }
    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    #endregion

}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int createAccount = 2;
    public const int findGame = 3;
    public const int gameStart = 4;
    public const int leaveWaitingRoom = 5;
    public const int leaveGame = 6;
}

static public class ServerToClientSignifiers
{
    public const int LoginFail = 0;
    public const int LoginSuccess = 1;
    public const int UserNotExist = 2;
    public const int UsernameInUse = 3;
    public const int FormatError = 4;
    public const int AccountCreated = 5;
    public const int NewGameRoom = 6;
    public const int ReturnToLobby = 7;
    public const int GameRoomFound = 8;
    public const int ClientJoined = 9;
    public const int ClientLeft = 10;
    public const int GameRoomFull = 11;
    public const int GameStartSuccess = 12;
    public const int GameStartFail = 13;
    public const int GameRoomGameInProgress = 14;
}

#endregion

