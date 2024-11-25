using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;
    
public class GyungPlayerMovement : MonoBehaviourPun
{
    public static GyungPlayerMovement Instance;

    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float changeMoveSpeed = 1.5f;
    [SerializeField] GameObject ball;
    [SerializeField] float slidingPower = 1f;
    public GameObject mobilePad;
    public VirtualJoystick joystick;

    public Transform target;
    public Transform camTrans;

    Animator animator;
    float animatorSpeed = 0f;
    private bool hasAnimator;

    [SerializeField] private bool isGrounded = false;
    //[SerializeField] private bool isSlididng = false;

    private Rigidbody rigid;

    public float dirH, dirV;

    public float turnSpeed = 10f;

    //AudioSource audioSource;
    //public AudioClip walkSound;
    //public AudioClip jumpSound;

    Ball ballScript;

    //스피드업
    [SerializeField] bool isSpeedUpActive = false;


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        hasAnimator = TryGetComponent(out animator);
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        joystick = FindObjectOfType<VirtualJoystick>();
        //audioSource = GetComponent<AudioSource>();
        if (hasAnimator)
        {
            animator.applyRootMotion = false;
        }
        //gameObject.transform.position = new Vector3(-63f, 3.77f, -1f); 
        if (photonView.IsMine)
        {
            SetupMobilePad();
        }

    }


    private void Update()
    {
        // 로컬 플레이어가 아닌 경우 입력을 받지 않음
        if (!photonView.IsMine)
        {
            return;
        }
        else
        {
            dirH = joystick.Horizontal();
            dirV = joystick.Vertical();
        }
        

        //if (isGrounded == false)
        //{
        //    isSlididng = true;
        //}
    }

    private void FixedUpdate()
    {
        // 로컬 플레이어만 직접 위치와 회전 변경 가능
        if (!photonView.IsMine)
        {
            return;
        }

        if (isGrounded)
        {
            Vector3 velocity = Time.deltaTime * moveSpeed * InputDirection();
            if (isSpeedUpActive == false)
            {
                velocity = Time.deltaTime * moveSpeed * InputDirection();
            }
            else
            {
                velocity = Time.deltaTime * moveSpeed * changeMoveSpeed * InputDirection();
            }
            rigid.MovePosition(transform.position + velocity);
            
        }
        
    }

    private void LateUpdate()
    {
        Rotation();
    }

    void SetupMobilePad()
    {
        mobilePad = Instantiate(Resources.Load(mobilePad.name) as GameObject);
        mobilePad.transform.SetParent(transform);
    }

    public Vector3 InputDirection()
    {
        Vector3 dirMove = Vector3.zero;
        dirMove += camTrans.forward * dirV;
        dirMove += camTrans.right * dirH;

        
        if (dirMove.magnitude > 1f)
        {
            dirMove.Normalize();
        }
        //dirMove.y = 0f;
        animatorSpeed = dirMove.magnitude;

        if (hasAnimator)
        {
            animator.SetFloat("Speed", animatorSpeed);
            
        }
        
        return dirMove;
    }

    private void Rotation()
    {
        Vector3 MoveDirection = InputDirection();
        if (MoveDirection.magnitude >= 0.001f)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + dirH, transform.rotation.eulerAngles.z);
            //if (!audioSource.isPlaying)
            //{
            //    audioSource.PlayOneShot(walkSound, .5f);
            //}
            //else
            //
            //{
            //       audioSource.Stop();
            //}
        }
        
    }

    
    public void Jump(float jumpForce)
    {
        float jumpPower = jumpForce * Time.deltaTime;

        if (isGrounded)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            if(jumpForce > 10f /*&& !audioSource.isPlaying*/)
            {
                //audioSource.PlayOneShot(jumpSound, 1f);
                isGrounded = false;
            }
            //else
            //{
            //    audioSource.Stop();
            //}

            if (hasAnimator)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    //public void Sliding()
    //{
    //    float slidePower = slidingPower * Time.deltaTime;
    //
    //    
    //    if (isSlididng == true)
    //    {
    //        rigid.AddForce(Vector3.forward * slidePower, ForceMode.Impulse);
    //        isSlididng = false;
    //        //audioSource.PlayOneShot(slidingSound, 1f);
    //    }
    //    //else
    //    //{
    //    //    audioSource.Stop();
    //    //}
    //
    //    //if (hasAnimator)
    //    //{
    //    //    animator.SetTrigger("Sliding");
    //    //}
    //}
    [PunRPC]
    public void Catch()
    {
        if (!photonView.IsMine) return;
        ball.gameObject.SetActive(true);
    }

    [PunRPC]
    private void Restart()
    {
        GetComponent<GyungPlayerMovement>().enabled = true;
        if (hasAnimator)
        {
            animator.SetBool("Hit", false);
        }
    }

    [PunRPC]
    public void StartSpinSquence()
    {
        float restart = 2f;
        GetComponent<GyungPlayerMovement>().enabled = false;
        if (hasAnimator)
        {
            animator.SetBool("Hit", true);
        }
        Invoke("Restart", restart);
    }

    [PunRPC]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        switch (collision.collider.tag)
            {
                case "Ball":
                    Catch();
                    break;
                case "SpeedUp":
                    SpeedPowerActive();
                    break;
                case "Spin":
                    StartSpinSquence();
                    break;
                default:
                    break;
        }
    }

    public void Finish()
    {
        Debug.Log("Finish");
    }

    public void SpeedPowerActive()
    {
        isSpeedUpActive = true;
        StartCoroutine(SpeedPowerDown());
    }

    IEnumerator SpeedPowerDown()
    {
        yield return new WaitForSeconds(3.0f);
        isSpeedUpActive = false;
    }
}
