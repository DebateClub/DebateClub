﻿using System;
using System.Net.Mime;
using System.Resources;
using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    public int WinningDifference = 2;
    public Font Font;

    public GameObject Player1;
    public GameObject Player2;

    [Range(1,3)]
    public int MaxPointsPerRound = 1;
    public Animator Intro;

    [Header("GUI")] [Tooltip("use {0} for the player id")]
    public string TextTookLead = "Player {0} established a theory";

    [Tooltip("use {0} for the player id")]
    public string TextBalanced = "Player {0} invalidated the argument";

    [Tooltip("use {0} for the player id")]
    public string TextWins = "Player {0} won the debate!";
    
    public Color TextOutlineColor;
    public Color RoundColorStart;
    public Color RoundColorEnd;

    public Color[] PlayerWinColors = {Color.blue, Color.red};

    public Sprite Medal;
    public Sprite RestartSprite;
    public Sprite MainMenuSprite;
    public int ButtonMargin = 20;
    public int TopButtonOffset = 90;

    private static readonly int[] PlayerRoundPoints = { 0, 0 };
    private static readonly int[] PlayerRoundsWon = { 0, 0 };

    private static int _round;
    private float _roundTime;
    
    private Vector2 _player1StartPosition;
    private Vector2 _player2StartPosition;

    private int _player1StartDirection;
    private int _player2StartDirection;

    private bool _isWaitingForEnd;
    private bool _askForNewRound;
    private bool _startPressed;
    private bool _backPressed;

    private bool _introStopped = false;
    private GUIStyle _roundStyle;

    // playerId (1 or 2), 0 => no winner
    private int _roundWinner = 0;
    private int _gameWinner = 0;

    private int _playerDiff = 0;


    void Start()
    {
        _player1StartPosition = Player1.transform.position;
        _player2StartPosition = Player2.transform.position;

        _player1StartDirection = Player1.GetComponent<Character>().Direction;
        _player2StartDirection = Player2.GetComponent<Character>().Direction;

        _roundStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            font = Font,
            fontSize = 50,
            richText = true,
            normal = { textColor =  RoundColorStart}
        };

        _round = 1;
        GUI.color = Color.white;
    }

    void OnGUI()
    {
        if (_askForNewRound)
        {

            var startRect = RestartSprite.textureRect;
            var exitRect = MainMenuSprite.textureRect;

            var width = startRect.width + exitRect.width + ButtonMargin; ;
            var height = Math.Max(startRect.height,exitRect.height);

            var box = new Rect(Screen.width / 2F - width / 2f, Screen.height / 2f - height / 2f + TopButtonOffset, width, height);
            var blankStyle = new GUIStyle();

            // might run into: Getting control 0's position in a group with only 0 controls when doing Repaint Aborting
            // I could fix it, but would be more work and since it's aborting, it still works perfectly.
            GUILayout.BeginArea(box, blankStyle);
            GUILayout.BeginHorizontal(blankStyle);
            if (GUILayout.Button(RestartSprite.texture, blankStyle) || _startPressed)
            {
                Application.LoadLevel(1);
                EndRound();
                ResetPoints(true);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(MainMenuSprite.texture, blankStyle))
            {
                Application.LoadLevel(0); // main menu
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }

        
        if(_introStopped)
            _roundTime += Time.deltaTime;

        DrawWinner();
        if (!_askForNewRound) DrawRound();
    }

    void Update()
    {
        _backPressed = Input.GetButtonDown("Back") || Input.GetKeyDown(KeyCode.Escape);
        _startPressed = Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return);

        if (_backPressed){
            Application.LoadLevel(0); // main menu
        }
    }


    public void EndRound(bool endGame = false)
    {
       
        Player1.transform.position = _player1StartPosition;
        Player2.transform.position = _player2StartPosition;
        Player1.transform.localScale = new Vector3(_player1StartDirection,1,1);
        Player2.transform.localScale = new Vector3(_player2StartDirection,1,1);
       
        Player1.GetComponent<Character>().Direction = _player1StartDirection;
        Player2.GetComponent<Character>().Direction = _player2StartDirection;

        ResetPoints(endGame);
        _round++;
        _roundTime = 0;
        _isWaitingForEnd = false;
        Player1.GetComponent<Character>().BlockInput(false);
        Player2.GetComponent<Character>().BlockInput(false);
        
    }

    private IEnumerator WaitForEnd()
    {
        
        _isWaitingForEnd = true;
        var players = new []{Player1, Player2};
        
        foreach (var player in players)
        {
            player.GetComponent<Character>().BlockInput(true);
        }

        yield return new WaitForSeconds(2);
        EndRound();
    }

    public void AddPoint(int playerId)
    {
        PlayerRoundPoints[playerId-1]++;
        var points = PlayerRoundPoints[playerId - 1];


        Debug.Log("player" + playerId + " got a point");


        if (points >= MaxPointsPerRound)
        { // player has won the round
            _roundWinner = playerId;
            PlayerRoundsWon[playerId - 1]++;
        }
        _playerDiff = PlayerRoundsWon[0] - PlayerRoundsWon[1];
        if (_playerDiff <= -WinningDifference)
        {
            _gameWinner = 2;
        }
        else if (_playerDiff >= WinningDifference)
        {
            _gameWinner = 1;
        }
        else
        {
            Debug.Log("No winner yet!");
            return;
        }
        Debug.Log("player "+playerId+" won!");
    }

    public void ResetPoints(bool endGame)
    {
        PlayerRoundPoints[0] = 0;
        PlayerRoundPoints[1] = 0;
        _roundWinner = 0;
        _gameWinner = 0;
        if (!endGame) return;
        PlayerRoundsWon[0] = 0;
        PlayerRoundsWon[1] = 0;
    }


    private void DrawRound()
    {
        var box = new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 50f, 200f, 100f);
        if (_introStopped)
        {
            var lerpTime = Math.Max(0, _roundTime-1);
           
            box = new Rect(
                Mathf.Lerp(box.xMin, Screen.width - box.width - 15, lerpTime),
                Mathf.Lerp(box.yMin, 0, lerpTime),
                box.width, box.height);
            
            _roundStyle.fontSize = (int) Mathf.Lerp(50f, 20f, lerpTime);
            _roundStyle.normal.textColor = Color.Lerp(RoundColorStart, RoundColorEnd, lerpTime);

            GUI.Label(box, "Round " + _round, _roundStyle);
            Utils.DrawOutline(box, "Round " + _round, _roundStyle, TextOutlineColor, (int)Mathf.Lerp(3, 2, lerpTime));
        }
    }

    public void DeclareIntroEnd()
    {
        _introStopped = true;
    }

    private void DrawWinner()
    {
        _askForNewRound = false;
        if (_roundWinner == 0) return; // no need to do draw the winner/end the round.
        var endRoundOnly = _gameWinner == 0;

        if (!_isWaitingForEnd && endRoundOnly)
        {
           StartCoroutine(WaitForEnd());
        }
     
        var player = _roundWinner;
        Player1.GetComponent<Character>().BlockInput(true);
        Player2.GetComponent<Character>().BlockInput(true);


        if (!endRoundOnly)
        {
            player = _gameWinner;
            _askForNewRound = true;
        }
        var centeredStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            font = Font,
            fontSize = 40,
            normal = { textColor = PlayerWinColors[player-1] }
        };


        var text = TextBalanced;
        if (_playerDiff != 0)
        {
            text = TextTookLead;
        }

        if (!endRoundOnly) text = TextWins;
        text = String.Format(text, player);
        var size = centeredStyle.CalcSize(new GUIContent(text));

        var box = new Rect(Screen.width / 2f - size.x / 2f, Screen.height / 2f - size.y / 2f, size.x, size.y);
        Utils.DrawOutline(box, text, centeredStyle, TextOutlineColor, 2);
        
    }
}
