using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerAssignment : MonoBehaviour
{
    [SerializeField] private PlayerInput fishermanInput;
    [SerializeField] private PlayerInput fishInput;

    private void Start()
    {
        if (Gamepad.all.Count < 2)
        {
            Debug.LogWarning("Connect two controllers before starting the scene.");
            return;
        }

        fishermanInput.neverAutoSwitchControlSchemes = true;
        fishInput.neverAutoSwitchControlSchemes = true;
        fishermanInput.user.UnpairDevices();
        fishInput.user.UnpairDevices();
        fishermanInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
        fishInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
    }
}