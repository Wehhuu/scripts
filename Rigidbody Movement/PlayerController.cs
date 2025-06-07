using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb2d;
    [SerializeField] private float2 _currentInput;

    [SerializeField] private float _speed;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] bool _isGrounded;
    [SerializeField] private Vector2 _groundCheckPos;//? Transform maliyetli olabilir, ekstra gameobject.
    [SerializeField] private Vector2 _groundCheckSize;
    [SerializeField] private float _jumpForce;
    private bool _isJumpKeyPressed;

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
        if (_isGrounded && _isJumpKeyPressed)
        {
            _isGrounded = false;
            _rb2d.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        _rb2d.linearVelocityX = _currentInput.x * _speed;


        _isGrounded = GroundCheck();
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
