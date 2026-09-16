using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerAssignment : MonoBehaviour
{
    [SerializeField] private PlayerInput fishermanInput;
    [SerializeField] private PlayerInput fishInput;

    private int lastGamepadCount = -1;

    private void Start()
    {
        if (fishermanInput == null || fishInput == null)
        {
            Debug.LogError("Assign both PlayerInput references.");
            enabled = false;
            return;
        }

        fishermanInput.neverAutoSwitchControlSchemes = true;
        fishInput.neverAutoSwitchControlSchemes = true;

        AssignControllers();
    }

    private void Update()
    {
        if (Gamepad.all.Count != lastGamepadCount)
        {
            AssignControllers();
        }
    }

    private void AssignControllers()
    {
        lastGamepadCount = Gamepad.all.Count;

        fishermanInput.DeactivateInput();
        fishInput.DeactivateInput();

        fishermanInput.user.UnpairDevices();
        fishInput.user.UnpairDevices();

        if (lastGamepadCount < 2)
        {
            Debug.LogWarning("Waiting for two controllers.");
            return;
        }

        fishermanInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);

        fishInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);

        fishermanInput.ActivateInput();
        fishInput.ActivateInput();
    }

   
}