using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class TakeInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActionAsset;
    public static float2 input;
    public static bool isJumpKeyPressed;


    // Update is called once per frame
    void Update()
    {
        input = _inputActionAsset.FindAction("Move").ReadValue<Vector2>();
        isJumpKeyPressed = _inputActionAsset.FindAction("Jump").ReadValue<float>() > 0 ? true : false;
    }

    void OnEnable()
    {
        _inputActionAsset.Enable();
    }

    void OnDisable()
    {
        _inputActionAsset.Disable();
    }
}
