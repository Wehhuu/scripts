using System.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _preDashDuration;//? İnput beklediğimiz yer.
    [SerializeField] private float _dashCooldown;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] bool _isGrounded;
    [SerializeField] private Vector2 _groundCheckPos;//? Transform maliyetli olabilir, ekstra gameobject.
    [SerializeField] private Vector2 _groundCheckSize;

    [SerializeField] private float2 _currentInput;

    private bool _isDashKeyPressed;
    private bool _isReadyToDash = true;
    private bool _isDashing = false;
    private bool _isJumpKeyPressed;
    private Rigidbody2D _rb2d;
    #endregion region Variables

    private void Awake()
    {
        _rb2d = this.gameObject.GetComponent<Rigidbody2D>();//? TryGetComponent da olabilir ama maliyetinin hakkını vermez, RequireComponent var.
    }

    // Update is called once per frame
    private void Update()
    {
        TakeInputFromSource();
    }

    private void FixedUpdate()
    {
        if (_isGrounded && _isJumpKeyPressed && !_isDashing)
        {
            _isGrounded = false;
            _rb2d.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        if (_isReadyToDash && _isDashKeyPressed && !_isDashing)
        {
            StartCoroutine(Dash());
        }

        if (!_isDashing)
        {
            _rb2d.linearVelocityX = _currentInput.x * _speed;
        }


        _isGrounded = GroundCheck();
    }

    private IEnumerator Dash()//? FixedUpdate içinde de yapılabilir fakat çok da gerek yok. Çünkü zaten anlık hız atama kullanacağız.
    {
        _isDashing = true;

        float temp_duration = 0;
        int dir = 0;
        while (temp_duration < _preDashDuration)
        {
            if (_currentInput.x >= 0)
            {
                dir = 1;
                temp_duration += _preDashDuration;
            }

            else if (_currentInput.x < 0)
            {
                dir = -1;
                temp_duration += _preDashDuration;
            }

            temp_duration += Time.deltaTime;
            yield return null;
        }
        temp_duration = 0;

        _rb2d.AddForce(Vector2.right * dir * _dashSpeed, ForceMode2D.Impulse);
        
        while (temp_duration < _dashDuration)
        {
            temp_duration += Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
        _isReadyToDash = false;
        
        temp_duration = 0;
        while (temp_duration < _dashCooldown)
        {
            temp_duration += Time.deltaTime;
            yield return null;
        }

        _isReadyToDash = true;
        yield return null;
    }

    /// <summary>
    /// Yere değip değmediğine göre değişken ataması yapar.
    /// </summary>
    /// <returns> bool = yere_değiyor ? evet : hayır </returns>
    private bool GroundCheck()
    {
        return Physics2D.OverlapBox(transform.TransformPoint(_groundCheckPos), _groundCheckSize, 0f, _groundLayer);
    }

    /// <summary>
    /// TakeInput sınıfından statik değişkenleri çek ve değerlerini yerel değişkenelere ata. (Kaynaktan sadece okuma yap)
    /// </summary>
    private void TakeInputFromSource()
    {
        _currentInput = TakeInput.input;
        _isJumpKeyPressed = TakeInput.isJumpKeyPressed;
        _isDashKeyPressed = TakeInput.isDashKeyPressed;
    }

    /// <summary>
    /// Hata giderme ve test için.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.TransformPoint(_groundCheckPos), _groundCheckSize);
    }
}
