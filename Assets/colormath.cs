using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using KMHelper;

public class colormath : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] btn;
    public MeshRenderer[] ledLeft, ledRight;
    public KMBombInfo Info;
    public TextMesh Text;
    public Color[] colors;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    private int[,] _leftcolor = new int[4, 10] {
    {5,1,2,8,3,7,0,9,6,4},
    {6,1,9,4,3,7,5,8,0,2},
    {4,1,5,7,0,6,9,3,8,2},
    {8,6,9,7,4,3,0,2,1,5}
    };

    private int[,] _rightcolor = new int[4, 10] {
    {0,8,9,4,3,2,1,5,7,6},
    {3,8,0,5,6,4,9,7,2,1},
    {1,9,4,7,3,0,2,5,8,6},
    {9,7,2,8,1,0,5,6,4,3}
    };

    private int[,] _anscolor = new int[4, 10] {
    {5,1,4,8,3,6,9,2,0,7},
    {0,1,3,7,9,4,5,8,6,2},
    {2,6,7,1,9,0,4,8,3,5},
    {1,8,2,4,9,5,3,7,0,6}
    };

    private int[,] _anscolordbg = new int[4, 10] {
    {8,1,7,4,2,0,5,9,3,6},
    {0,1,9,2,5,6,8,3,7,4},
    {5,3,0,8,6,9,1,2,7,4},
    {8,0,2,6,3,5,9,7,1,4}
    };

    private bool _click = false, _isSolved = false, _lightsOn = false;
    private int _mode, _act, _left, _right, _red = 0, _ans = 0, _sol = 0;
    private int[] _rightPos = { 0, 0, 0, 0 }, _solColor = { 0, 0, 0, 0 };
    private string[] _colorText = { "Blue", "Green", "Purple", "Yellow", "White", "Magenta", "Red", "Orange", "Gray", "Black" };

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Init;
    }

    private void Awake()
    {
        btn[0].OnInteract += delegate ()
        {
            handlePress(0);
            return false;
        };
        btn[1].OnInteract += delegate ()
        {
            handlePress(1);
            return false;
        };
        btn[2].OnInteract += delegate ()
        {
            handlePress(2);
            return false;
        };
        btn[3].OnInteract += delegate ()
        {
            handlePress(3);
            return false;
        };
        btn[4].OnInteract += delegate ()
        {
            ansChk();
            return false;
        };
    }

    void Init()
    {
        int temp, mult = 1000;
        _mode = Random.Range(0, 2); _act = Random.Range(0, 4);
        _left = Random.Range(0, 10000); _right = Random.Range(1, 10000);

        drawInitColor(0);

        if (_mode == 0)
        {
            Text.color = Color.green;
            if (_act == 0)
            {
                _sol = _left + _right;
                Debug.LogFormat("[Color Math #{0}] mode GREEN action ADD sol {1} + {2} = {3}", _moduleId, _left, _right, _sol);
                Text.text = "A";
            }
            else if (_act == 1)
            {
                _sol = _left - _right;
                if (_sol < 0) { _sol *= -1; Debug.LogFormat("[Color Math #{0}] Adjusted solution to positive", _moduleId); }
                Debug.LogFormat("[Color Math #{0}] mode GREEN action SUB sol {1} - {2} = {3}", _moduleId, _left, _right, _sol);
                Text.text = "S";
            }
            else if (_act == 2)
            {
                _sol = _left * _right;
                Debug.LogFormat("[Color Math #{0}] mode GREEN action MUL sol {1} * {2} = {3}", _moduleId, _left, _right, _sol);
                Text.text = "M";
            }
            else
            {
                _sol = _left / _right;
                Debug.LogFormat("[Color Math #{0}] mode GREEN action DIV sol {1} / {2} = {3}", _moduleId, _left, _right, _sol);
                Text.text = "D";
            }
        }
        else
        {
            Text.color = Color.red;
            generateRed();
            if (_act == 0)
            {
                _sol = _left + _red;
                Debug.LogFormat("[Color Math #{0}] mode RED action ADD sol {1} + {2} = {3}", _moduleId, _left, _red, _sol);
                Text.text = "A";
            }
            else if (_act == 1)
            {
                _sol = _left - _red;
                if (_sol < 0) { _sol *= -1; Debug.LogFormat("[Color Math #{0}] Adjusted solution to positive", _moduleId); }
                Debug.LogFormat("[Color Math #{0}] mode RED action SUB sol {1} - {2} = {3}", _moduleId, _left, _red, _sol);
                Text.text = "S";
            }
            else if (_act == 2)
            {
                _sol = _left * _red;
                Debug.LogFormat("[Color Math #{0}] mode RED action MUL sol {1} * {2} = {3}", _moduleId, _left, _red, _sol);
                Text.text = "M";
            }
            else
            {
                _sol = _left / _red;
                Debug.LogFormat("[Color Math #{0}] mode RED action DIV sol {1} / {2} = {3}", _moduleId, _left, _red, _sol);
                Text.text = "D";
            }
        }
        if(_sol >= 10000)
        {
            Debug.LogFormat("[Color Math #{0}] Result capped from {1} to {2}", _moduleId, _sol, _sol %= 10000);
        }

        temp = _sol;
        for (int i = 0; i < 4; i++)
        {
            _solColor[i] = temp / mult;
            temp %= mult;
            mult /= 10;
        }
        Debug.LogFormat("[Color Math #{0}] End solution = {1} (Sequence {2} {3} {4} {5})", _moduleId,
            _sol, _colorText[_anscolordbg[0, _solColor[0]]], _colorText[_anscolordbg[1, _solColor[1]]], _colorText[_anscolordbg[2, _solColor[2]]], _colorText[_anscolordbg[3, _solColor[3]]]);

        _lightsOn = true;
    }

    void generateRed()
    {
        int _batt = Info.GetBatteryCount();
        if(_batt<=1)
        {
            _red = (Info.GetSerialNumberNumbers().First() * 1000) + (Info.GetOffIndicators().Count() * 100) + 90 + Info.GetPortCount(KMBombInfoExtensions.KnownPortType.RJ45);
        }
        else if(_batt<=3)
        {
            _red = (Info.GetPortCount(KMBombInfoExtensions.KnownPortType.PS2) * 100) + (Info.GetSerialNumberLetters().Count() * 10) + Info.GetSerialNumberNumbers().Last();
        }
        else if(_batt<=5)
        {
            _red = (Info.GetSerialNumber().Count(c => "AEIOU".Contains(c)) * 1000) + (Info.GetBatteryHolderCount() * 100) + (Info.GetPortCount(KMBombInfoExtensions.KnownPortType.Serial) * 10) + 4;
        }
        else
        {
            _red = (Info.GetPortCount(KMBombInfoExtensions.KnownPortType.DVI) * 1000) + 500 + ((Info.GetSerialNumberLetters().Count() - Info.GetSerialNumber().Count(c => "AEIOU".Contains(c))) * 10) + (Info.GetOnIndicators().Count());
        }
    }

    void drawInitColor(int m)
    {
        int[] _tempPosLeft = { 0, 0, 0, 0 }, _tempPosRight = { 0, 0, 0, 0 };
        int mult = 1000, l = _left, r = _right, tl, tr;
        for (int i = 0; i < 4; i++)
        {
            tl = l / mult; tr = r / mult;
            l %= mult; r %= mult;
            _tempPosLeft[i] = _leftcolor[i, tl];
            _tempPosRight[i] = _rightcolor[i, tr];
            ledLeft[i].material.color = colors[_leftcolor[i,tl]];
            ledRight[i].material.color = colors[_rightcolor[i,tr]];
            mult /= 10;
        }
        if(m == 0)
        {
            Debug.LogFormat("[Color Math #{0}] Left LED converts to {1} (sequence {2} {3} {4} {5})", _moduleId, _left, _colorText[_tempPosLeft[0]], _colorText[_tempPosLeft[1]], _colorText[_tempPosLeft[2]], _colorText[_tempPosLeft[3]]);
            Debug.LogFormat("[Color Math #{0}] Right LED converts to {1} (sequence {2} {3} {4} {5})", _moduleId, _right, _colorText[_tempPosRight[0]], _colorText[_tempPosRight[1]], _colorText[_tempPosRight[2]], _colorText[_tempPosRight[3]]);
        }
    }

    void handlePress(int m)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[m].transform);
        btn[m].AddInteractionPunch();

        if(!_isSolved && _lightsOn)
        {
            if (!_click)
            {
                for (int i = 0; i < 4; i++)
                {
                    ledRight[i].material.color = Color.blue;
                    _rightPos[i] = 0;
                }
                _click = true;
            }
            else
            {
                _rightPos[m]++;
                if (_rightPos[m] > 9) _rightPos[m] = 0;
                ledRight[m].material.color = colors[_rightPos[m]];
            }
        }
    }

    void ansChk()
    {
        int mult = 1000, temp = _sol;
        _ans = 0;

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[4].transform);
        btn[4].AddInteractionPunch();

        if(!_isSolved && _lightsOn)
        {
            for (int i = 0; i < 4; i++)
            {
                _ans += _anscolor[i, _rightPos[i]] * mult;
                temp %= mult;
                mult /= 10;
            }
            Debug.LogFormat("[Color Math #{0}] Solution = {1} (Sequence {2} {3} {4} {5}) Answered = {6} (Sequence {7} {8} {9} {10})", _moduleId,
                _sol, _colorText[_anscolordbg[0, _solColor[0]]], _colorText[_anscolordbg[1, _solColor[1]]], _colorText[_anscolordbg[2, _solColor[2]]], _colorText[_anscolordbg[3, _solColor[3]]],
                _ans, _colorText[_rightPos[0]], _colorText[_rightPos[1]], _colorText[_rightPos[2]], _colorText[_rightPos[3]]);

            if (_sol == _ans)
            {
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Color Math #{0}] Answer correct! Module passed!", _moduleId);
                _isSolved = true;
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                drawInitColor(1);
                _click = false;
                Debug.LogFormat("[Color Math #{0}] Answer incorrect! Strike!", _moduleId);
            }
        }
    }
	
	KMSelectable[] ProcessTwitchCommand(string command)
    {
        int repA, repB, repC, repD;
        KMSelectable[] cmdA, cmdB, cmdC, cmdD, first = {};
        string colIndex = "bgpywmroak";

        command = command.ToLowerInvariant().Trim();

        if (command.Equals("submit")) return new[] { btn[4] };

        if (Regex.IsMatch(command, @"^set [bgpywmroak],[bgpywmroak],[bgpywmroak],[bgpywmroak]$"))
        {
            if (!_click)
            {
                first = new[] { btn[0] };
                for (int i = 0; i < 4; i++)
                {
                    _rightPos[i] = 0;
                }
            }

            command = command.Substring(4);

            repA = colIndex.IndexOf(command[0]) - _rightPos[0];
            repB = colIndex.IndexOf(command[2]) - _rightPos[1];
            repC = colIndex.IndexOf(command[4]) - _rightPos[2];
            repD = colIndex.IndexOf(command[6]) - _rightPos[3];
            if (repA < 0) repA += 10;
            if (repB < 0) repB += 10;
            if (repC < 0) repC += 10;
            if (repD < 0) repD += 10;

            cmdA = Enumerable.Repeat(btn[0], repA).ToArray();
            cmdB = Enumerable.Repeat(btn[1], repB).ToArray();
            cmdC = Enumerable.Repeat(btn[2], repC).ToArray();
            cmdD = Enumerable.Repeat(btn[3], repD).ToArray();
            return first.Concat(cmdA.Concat(cmdB.Concat(cmdC.Concat(cmdD)))).ToArray();
        }

        return null;
    }
}
