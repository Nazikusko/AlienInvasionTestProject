using UnityEngine;

public class JoystickInput : Singleton<JoystickInput>
{
    [SerializeField] private FixedJoystick _fixedJoistick;

    private Vector2 _joystickSate;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public Vector2 GetJoystickState()
    {
        _joystickSate.x = _fixedJoistick.Horizontal;
        _joystickSate.y = _fixedJoistick.Vertical;

        return _joystickSate;
    }

}
