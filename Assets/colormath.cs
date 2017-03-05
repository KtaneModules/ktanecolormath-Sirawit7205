using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
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

    private bool _click = false;
    private int _mode, _act, _left, _right, _red = 0, _ans = 0, _sol = 0;
    private int[] _rightPos = { 0, 0, 0, 0 };

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
        _mode = Random.Range(0, 2); _act = Random.Range(0, 4);
        _left = Random.Range(0, 10000); _right = Random.Range(0, 10000);

        drawInitColor();

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

    void drawInitColor()
    {
        int mult = 1000, l = _left, r = _right, tl, tr;
        for (int i = 0; i < 4; i++)
        {
            tl = l / mult; tr = r / mult;
            l %= mult; r %= mult;
            ledLeft[i].material.color = colors[_leftcolor[i,tl]];
            ledRight[i].material.color = colors[_rightcolor[i,tr]];
            mult /= 10;
        }
    }

    void handlePress(int m)
    {
        if (_click == false)
        {
            for (int i = 0; i < 4; i++) ledRight[i].material.color = Color.blue;
            _click = true;
        }
        else
        {
            _rightPos[m]++;
            if (_rightPos[m] > 9) _rightPos[m] = 0;
            ledRight[m].material.color = colors[_rightPos[m]];
        }
    }

    void ansChk()
    {
        int mult = 1000;
        for (int i = 0; i < 4; i++)
        {
            _ans += _anscolor[i, _rightPos[i]] * mult;
            mult /= 10;
        }
        Debug.LogFormat("[Color Math #{0}] Solution = {1} Answered = {2}", _moduleId, _sol, _ans);

        if (_sol == _ans)
        {
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Color Math #{0}] Answer correct! Module passed!", _moduleId);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Color Math #{0}] Answer incorrect! Strike!", _moduleId);
        }
    }
}
