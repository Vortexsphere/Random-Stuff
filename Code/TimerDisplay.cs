using TMPro;
using UnityEngine;

// TOOD
// The implementation can be made a bit more efficient. For example, it's not
// necessary to calculate the hours and minutes digits so frequently. And it's
// definitely not necessary to store the entirety of the time amount. We could
// instead just keep a seconds variable, count it up every frame, roll it over
// once it reaches 60, increment the minutes by 1, and so on.

/// <summary>
/// Timer display in the format: HH:MM:SS:sss where sss is milliseconds.
/// Attach this to a Canvas and it will display the timer in the lower left.
/// Mostly just a proof-of-concept test. Plenty of room for improvement.
///
/// The reason for this test was to create a rapidly-updating timer that didn't
/// generate lots of garbage, which happens with the naive approach to updating
/// text, i.e., using strings.
/// In tests using strings, I observed anywhere from around 0.7-2.8k garbage
/// produced PER UPDATE CALL when using strings, compared to ZERO using this.
///
/// IMPORTANT: testing in the editor will show allocations every frame because
/// the editor is being updated with a string into the TextMeshProUGUI's text
/// field in the editor. In an actual build however this is not present and such
/// allocations are gone.
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    private TextMeshProUGUI _timer;
    private char[] _timerChars;
    private uint _currMS = 0;
    private double _elapsedSeconds = 0f;
    private const uint MAX_SECONDS = 60 * 60 * 1000; // Number of seconds in 1000 hours.
    private float _timeFactor = 1f;
    private uint _timeFactorInt = 1000; // 1000 = 1.0f timescale; maintains precision with changes.

    private void Start()
    {
        GameObject go = new GameObject();
        _timer = go.AddComponent<TextMeshProUGUI>();
        _timer.alignment = TextAlignmentOptions.Left;
        go.transform.SetParent(gameObject.transform); // The canvas is the parent.
        // IMPORTANT: Setting the rect transform parameters must be done AFTER setting the parent
        // otherwise things will get readjusted.
        RectTransform rt = _timer.rectTransform;
	// Anchor text in the bottom left corner, slightly shifted to the right.
        rt.anchorMin = new Vector2(0.005f, 0f);
        rt.anchorMax = new Vector2(0.005f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(300f, 40f); // Display size compared to anchor rectangle (0).

        _timerChars = new char[13] { // HHH:MM:SSS:sss format (sss = milliseconds).
            '0', '0', '0', ':', '0', '0', ':', '0', '0', ':', '0', '0', '0'
        };
        _timer.SetCharArray(_timerChars);
    }

    private void Update()
    {
        // Timescale range from 0.01 to 10000 times normal.
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            _timeFactorInt = (uint)Mathf.Min(10000000, _timeFactorInt + 10 * (Input.GetKey(KeyCode.LeftShift) ? 10 : 1));
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            _timeFactorInt = (uint)Mathf.Max(10, _timeFactorInt - 10 * (Input.GetKey(KeyCode.LeftShift) ? 10 : 1));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _timeFactorInt = 1000; // Reset to default 1.0x timescale.
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            _timeFactorInt = 10; // Set to minimum 0.01x timescale.
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            _timeFactorInt = 10000000; // Set to maximum 10000x timescale.
        }

        // Double precision required for lowest timescales to ensure the value
	// doesn't round to 0.
        _timeFactor = _timeFactorInt / 1000f;
        _elapsedSeconds += (double)Time.unscaledDeltaTime * _timeFactor;
        if (_elapsedSeconds >= MAX_SECONDS) _elapsedSeconds = 0f; // Reset (simple option).

        _currMS = (uint)(_elapsedSeconds * 1000);
        uint hours = _currMS / 3600000;
        uint minutes = (_currMS - (hours * 3600000)) / 60000;
        uint seconds = (_currMS - hours * 3600000 - minutes * 60000) / 1000;
        uint milliseconds = (uint)(_elapsedSeconds % 1 * 1000); // E.g. 1.234s -> 234ms.

        // Set the appropriate character digits in the text array.
        if (hours < 10)
        {
            _timerChars[0] = '0';
            _timerChars[1] = '0';
            _timerChars[2] = (char)(hours + '0');
        }
        else if (hours < 100)
        {
            _timerChars[0] = '0';
            _timerChars[1] = (char)(hours / 10 + '0');
            _timerChars[2] = (char)(hours % 10 + '0');
        }
        else
        {
            _timerChars[0] = (char)(hours / 100 + '0');
            _timerChars[1] = (char)((hours % 100) / 10 + '0');
            _timerChars[2] = (char)(hours % 10 + '0');
        }
        if (minutes < 10)
        {
            _timerChars[4] = '0';
            _timerChars[5] = (char)(minutes + '0');
        }
        else
        {
            _timerChars[4] = (char)(minutes / 10 + '0');
            _timerChars[5] = (char)(minutes % 10 + '0');
        }
        if (seconds < 10)
        {
            _timerChars[7] = '0';
            _timerChars[8] = (char)(seconds + '0');
        }
        else
        {
            _timerChars[7] = (char)(seconds / 10 + '0');
            _timerChars[8] = (char)(seconds % 10 + '0');
        }
        if (milliseconds < 10)
        {
            _timerChars[10] = '0';
            _timerChars[11] = '0';
            _timerChars[12] = (char)(milliseconds + '0');
        }
        else if (milliseconds < 100)
        {
            _timerChars[10] = '0';
            _timerChars[11] = (char)(milliseconds / 10 + '0');
            _timerChars[12] = (char)(milliseconds % 10 + '0');
        }
        else
        {
            _timerChars[10] = (char)(milliseconds / 100 + '0');
            _timerChars[11] = (char)((milliseconds % 100) / 10 + '0');
            _timerChars[12] = (char)(milliseconds % 10 + '0');
        }
        _timer.SetCharArray(_timerChars); // GC alloc-free method.
    }
}
