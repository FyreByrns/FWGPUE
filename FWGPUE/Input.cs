using Silk.NET.Input;
using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE;

static class Input {
    public static IInputContext? InputContext { get; set; }

    /// <summary> Whether keys are down (true) or up (false). </summary>
    public static bool[]? KeyStates { get; set; }
    /// <summary> Number of frames keys have been down, or 0 if they are not currently down. </summary>
    public static int[]? KeyFrames { get; set; }
    /// <summary> Time in seconds keys have been down, or 0 if they are not currently down. </summary>
    public static float[]? KeyTimers { get; set; }

    /// <summary> Whether mouse buttons are down (true) or up (false). </summary>
    public static bool[]? MouseStates { get; set; }
    /// <summary> Number of frames mouse buttons have been down, or 0 if they are not currently down. </summary>
    public static int[]? MouseFrames { get; set; }
    /// <summary> Time in seconds mouse buttons have been down, or 0 if they are not currently down. </summary>
    public static float[]? MouseTimers { get; set; }

    public static void UpdateKeyFrames() {
        for (int key = 0; key < (int)Enum.GetValues<Key>().Max(); key++) {
            if (KeyStates![key]) {
                KeyFrames![key]++;
            }
            else {
                KeyFrames![key] = 0;
            }
        }
    }
    public static void UpdateKeyTimers(float elapsed) {
        for (int key = 0; key < (int)Enum.GetValues<Key>().Max(); key++) {
            if (KeyStates![key]) {
                KeyTimers![key] += elapsed;
            }
            else {
                KeyTimers![key] = 0;
            }
        }
    }

    public static void UpdateMouseFrames() {
        for (int mouse = 0; mouse < (int)Enum.GetValues<MouseButton>().Max(); mouse++) {
            if (MouseStates![mouse]) {
                MouseFrames![mouse]++;
            }
            else {
                MouseFrames![mouse] = 0;
            }
        }
    }
    public static void UpdateMouseTimers(float elapsed) {
        for (int mouse = 0; mouse < (int)Enum.GetValues<MouseButton>().Max(); mouse++) {
            if (MouseStates![mouse]) {
                MouseTimers![mouse] += elapsed;
            }
            else {
                MouseTimers![mouse] = 0;
            }
        }
    }

    public static bool KeyPressed(Key key, int framesSincePress = 1) {
        return KeyFrames![(int)key] == framesSincePress;
    }
    public static bool KeyDown(Key key) {
        return KeyStates![(int)key];
    }
    public static bool KeyUp(Key key) => !KeyDown(key);

    public static bool MouseButtonPressed(MouseButton button, int framesSincePress = 1) {
        return MouseFrames![(int)button] == framesSincePress;
    }
    public static bool MouseButtonDown(MouseButton button) {
        return MouseStates![(int)button];
    }
    public static bool MouseButtonUp(MouseButton button) => !MouseButtonDown(button);

    public static Vector2 MousePosition() {
        return InputContext!.Mice.First().Position;
    }

    public static bool KeyPressedWithinTime(Key key, float secondsSincePress) {
        return KeyTimers![(int)key] <= secondsSincePress;
    }

    private static void OnKeyUp(IKeyboard keyboard, Key key, int args) {
        Log.Inane($"{key} | {args} up");
        KeyStates![(int)key] = false;
    }
    private static void OnKeyDown(IKeyboard keyboard, Key key, int args) {
        Log.Inane($"{key} | {args} down");
        KeyStates![(int)key] = true;
    }

    private static void OnMouseUp(IMouse mouse, MouseButton button) {
        Log.Inane($"{button} up");
        MouseStates![(int)button] = false;
    }
    private static void OnMouseDown(IMouse mouse, MouseButton button) {
        Log.Inane($"{button} down");
        MouseStates![(int)button] = true;
    }
    
    public static void Update(float elapsed) {
        UpdateMouseFrames();
        UpdateMouseTimers((float)elapsed);
        UpdateKeyFrames();
        UpdateKeyTimers((float)elapsed);
    }

    public static void Init(Engine context) {
        InputContext = context.Window!.CreateInput();
        for (int i = 0; i < InputContext.Keyboards.Count; i++) {
            InputContext.Keyboards[i].KeyDown += OnKeyDown;
            InputContext.Keyboards[i].KeyUp += OnKeyUp;
        }
        for (int i = 0; i < InputContext.Mice.Count; i++) {
            InputContext.Mice[i].MouseDown += OnMouseDown; ;
            InputContext.Mice[i].MouseUp += OnMouseUp;
        }

        int keyCount = (int)Enum.GetValues<Key>().Max();
        KeyStates = new bool[keyCount];
        KeyFrames = new int[keyCount];
        KeyTimers = new float[keyCount];

        int mouseCount = (int)Enum.GetValues<MouseButton>().Max();
        MouseStates = new bool[mouseCount];
        MouseFrames = new int[mouseCount];
        MouseTimers = new float[mouseCount];
    }
}
