using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using WindowsInput.Events;
using WindowsInput.Events.Sources;

namespace TabletFriend
{
    public static class ShortcutManager
    {
        private static IKeyboardEventSource _keyboard;
        private static readonly HashSet<KeyCode> _pressed = new HashSet<KeyCode>();
        private static KeyCode[] _shortcut;
        private static bool _triggered;

        public static void Init()
        {
            _keyboard = Capture.Global.KeyboardAsync();
            _keyboard.KeyEvent += KeyEvent;
            EventBeacon.Subscribe(Events.UpdateSettings, _ => LoadShortcut());
            LoadShortcut();
        }

        private static void LoadShortcut()
        {
            var text = AppState.Settings?.ToggleHotkey;
            if (string.IsNullOrWhiteSpace(text))
            {
                _shortcut = null;
                return;
            }
            _shortcut = ParseKeys(text);
            _triggered = false;
        }

        private static void KeyEvent(object sender, EventSourceEventArgs<KeyboardEvent> e)
        {
            var down = e.Data?.KeyDown?.Key;
            var up = e.Data?.KeyUp?.Key;

            if (down.HasValue)
            {
                _pressed.Add(down.Value);
                if (!_triggered && _shortcut != null && _shortcut.All(k => _pressed.Contains(k)))
                {
                    _triggered = true;
                    EventBeacon.SendEvent(Events.ToggleMinimize);
                }
            }

            if (up.HasValue)
            {
                _pressed.Remove(up.Value);
                if (_shortcut != null && !_shortcut.All(k => _pressed.Contains(k)))
                {
                    _triggered = false;
                }
            }
        }

        private static KeyCode[] ParseKeys(string keyString)
        {
            var args = keyString.Replace(" ", string.Empty).Replace("_", string.Empty).Split('+');
            var keysList = new List<KeyCode>();
            foreach (var arg in args)
            {
                try
                {
                    keysList.Add(Enum.Parse<KeyCode>(Translate(arg), true));
                }
                catch
                {
                    // Ignore invalid entries
                }
            }
            return keysList.ToArray();
        }

        private static string Translate(string inputKey)
        {
            if (_translationTable.TryGetValue(inputKey, out var outputKey))
            {
                return outputKey;
            }
            return inputKey;
        }

        private static readonly Dictionary<string, string> _translationTable = new Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase)
        {
            { "Windows", nameof(KeyCode.LWin) },
            { "Win", nameof(KeyCode.LWin) },
            { "Shift", nameof(KeyCode.LShift) },
            { "Ctrl", nameof(KeyCode.LControl) },
            { "Alt", nameof(KeyCode.LAlt) },
            { "0", nameof(KeyCode.D0) },
            { "1", nameof(KeyCode.D1) },
            { "2", nameof(KeyCode.D2) },
            { "3", nameof(KeyCode.D3) },
            { "4", nameof(KeyCode.D4) },
            { "5", nameof(KeyCode.D5) },
            { "6", nameof(KeyCode.D6) },
            { "7", nameof(KeyCode.D7) },
            { "8", nameof(KeyCode.D8) },
            { "9", nameof(KeyCode.D9) },
        };
    }
}
