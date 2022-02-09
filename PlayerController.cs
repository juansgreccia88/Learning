using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public CharacterController player;
    public GameObject colliderAtack;

    [Header("GENERAL VARS")]
    public int lifes;
    public bool playerDamage;
    public bool onLadder;
    public bool upStair;
    public bool downStair;

    [Header("PLAYER INPUT")]
    public float horizontalMove;
    public float verticalMove;
    private Vector3 playerInput;
    public Vector3 movePlayer;

    [Header("PLAYER PHYSICS")]
    public float fallVelocity;
    public float playerWalkSpeed;
    public float playerComboAtackSpeed;
    public float playerSpeed;
    public float jumpForce;
    public float gravity;
    public float fallVelocityDS; //Fallimg power for downwardStroke
    public float slideVelocity;
    public float slopeForceDown;
    public string hitTag;
    public Vector3 hitNormal;
    public int hitLayer;
    public Vector3 CylinderDistanceTraveled;

    [Header("CHECKS PHYSICS")]
    public bool playerIsGrounded;
    public bool isOnSlope;
    public int groundedLayerMask;

    [Header("PLAYER SKILLS")]
    public bool playerJump;
    public bool PlayerDoubleJump;
    public bool downwardStroke = false;
    public bool comboAttaking = false;
    public string hitAtackName;

    [Header("COMBO ATTACK VARS")]
    public float comboWaitReady;
    public float maxTimeForHit; 
    bool comboAtackReady = true;
    float comboHitState = 0;
    float comboTime = 0;

    [Header("CAMERA")]
    public Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;

    //Variables animacion
    public Animator playerAnimatorController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();
        mainCamera = Camera.main;
        playerSpeed = playerWalkSpeed;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {  
        playerIsGrounded = player.isGrounded;

        if (player.isGrounded)
        {
            playerJump = false;
            PlayerDoubleJump = false;
            downwardStroke = false;
            onLadder = false;
            upStair = false;
            downStair = false;
        }

        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        playerInput = Vector3.ClampMagnitude(playerInput, 1); //Usamos esta funcion para que al sumarce ambas direcciones de movimiento el mismo sea constante en su velosidad maxima, por ejemplo en 10, al no hacerlo cuando las velosidades se suman puede superarlo con lo cual rompe con el limite impuesto

        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerInput.magnitude * playerSpeed);

        CamDirection(); // Ontenemso la direccion de la camara

        movePlayer = playerInput.x * camRight + playerInput.z * camForward;
        movePlayer = movePlayer * playerSpeed;
        player.transform.LookAt(player.transform.position + movePlayer);

        SetGravity();
        PlayerSkills();

        //Si el personaje es golpeado no podra moverce por una fraccion de tiempo que se configura desde la clase "DetectDamage" en el metodo "DontMoveForDamage"
        if (playerDamage || upStair || downStair)
        {
            playerSpeed = 0;
        }

        player.Move(movePlayer * Time.deltaTime);
    }

    public void FixedUpdate()
    {
        
    }

    //Obtenemos la direccion a donde mira la camara
    public void CamDirection()
    {
        camForward = mainCamera.transform.forward; //Direccion en eje Z
        camRight = mainCamera.transform.right; //Direccion en eje X

        //Al no usar la direccion en el eje Y seteamos en 0 su valor
        camForward.y = 0;
        camRight.y = 0;

        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }

    //Distintas funciones para los movimientos del personaje
    public void PlayerSkills()
    {
        //------------------------ Saltar / Salto doble ---------------------------------//

        JumpAndDoubleJump();

        //--------------------------- Combo de ataque -----------------------------------//

        ComboAtack();

        //-------------------------- Golpe decendente -----------------------------------//

        DownwardStroke();

        //Variasion de velosidad de movimiento

        if (playerAnimatorController.GetCurrentAnimatorStateInfo(0).IsName("ComboAtack"))
        {
            playerSpeed = playerComboAtackSpeed;
        }
        else
        {
            playerSpeed = playerWalkSpeed;
        }

    }

    //Salto simple del personaje, a la fuerza de caida por gravedad le vamos a dar un valor positivo alto para que el personaje realize el salto, al existir gravedad ese salto tendra un limite
    //y a su vez ira desacelerando hasta que su fuerza hacia arriva sea 0 y comience a caer ya que actua la gravedad.
    public void JumpAndDoubleJump()
    {
        if (!playerJump && (player.isGrounded || onLadder) && !isOnSlope && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            movePlayer.y = fallVelocity;
            playerAnimatorController.SetTrigger("PlayerJump");

            playerJump = true;
            Debug.Log("Jump");

        }
        else if (playerJump && !PlayerDoubleJump && !isOnSlope && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            movePlayer.y = fallVelocity;

            PlayerDoubleJump = true;
        }
    }

    //Golpe basico que solo se puede usar estando en el piso
    public void ComboAtack()
    {
        comboAttaking = false;
        if (comboAtackReady && player.isGrounded && !isOnSlope && Input.GetButtonDown("Fire1"))
        {
            if (comboHitState == 0 || comboTime > maxTimeForHit)
            {
                comboHitState = 1;
                hitAtackName = "ComboHit" + comboHitState;
                colliderAtack.SetActive(false);
                colliderAtack.SetActive(true);
                comboAttaking = true;
                comboTime = 0;
                playerAnimatorController.SetTrigger("ComboAttaking");
                playerAnimatorController.SetFloat("comboHitState", comboHitState);
            }
            else if (comboHitState < 3 && comboTime < maxTimeForHit)
            {
                comboHitState++;
                hitAtackName = "ComboHit" + comboHitState;
                colliderAtack.SetActive(false);
                colliderAtack.SetActive(true);
                comboAttaking = true;
                comboTime = 0;
                playerAnimatorController.SetTrigger("ComboAttaking");
                playerAnimatorController.SetFloat("comboHitState", comboHitState);
            }

        }

        if (comboHitState == 3)
        {
            comboHitState = 0;
            comboTime = 0;
            comboAtackReady = false;
            StartCoroutine(WaitForComboAtackReady(comboWaitReady));
        }

        if (comboAtackReady == true)
        {
            StopCoroutine(WaitForComboAtackReady(comboWaitReady));
            comboTime += Time.deltaTime;
        }
        else
        {
            comboHitState = 0;
            comboTime = 0;
        }

        
                                                               
    }

    IEnumerator WaitForComboAtackReady(float time)
    {
        yield return new WaitForSeconds(time);
        playerAnimatorController.SetFloat("comboHitState", comboHitState);
        comboAtackReady = true;
    }

    IEnumerator StillAvtiveAtackCollider(float time)
    {
        colliderAtack.SetActive(true);
        yield return new WaitForSeconds(time);
        colliderAtack.SetActive(false);
    }

    //Golpe decendente: Golpe que se realiza cuando se esta en el aire, el mismo es decendente y termina cuando toca el suelo
    public void DownwardStroke()
    {
        //Verificamos si podemos hacer el golpe decendente
        if ((playerJump == true || PlayerDoubleJump == true) && Input.GetButtonDown("downwardStroke"))
        {
            hitAtackName = "downwardStroke";
            downwardStroke = true;
        }

        if (downwardStroke == true)
        {
            fallVelocity -= fallVelocityDS * Time.deltaTime;
            movePlayer.y = fallVelocity;
        }
    }

    //La gravedad del player
    public void SetGravity()
    {
        //Si estamos en el suelo la fuerza de salto sera igual a la gravedad, lo que mantendra al personaje pegado al suelo, En cambio si el personaje esta en el aire la gravedad ira incrementando
        //de forma relativa al tiempo transcurrido para dar sensacion de acceleracion.
        if (player.isGrounded)
        {
            fallVelocity = -gravity * 2 * Time.deltaTime;
            movePlayer.y = fallVelocity;
        }
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            movePlayer.y = fallVelocity;

            playerAnimatorController.SetFloat("PlayerVerticalVelocity", player.velocity.y);
        }

        playerAnimatorController.SetBool("IsGrounded", player.isGrounded);

        AddDistancePlatformRotating();
        SlideDown();
        IsStairs();
        
    }

    //Verificamos si estamos en una pendiente mayor a la que podemos caminar, de ser asi procedemos a aplicar un movimiento igual al vector del plano en el que estamos, dando el efecto de 
    //resbalarce o deslisarce hacia abajo de la pendiente.
    public void SlideDown()
    {
        if (hitTag != "CylinderPlatform")
        {
            isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit && Vector3.Angle(Vector3.up, hitNormal) < 90;

            if (isOnSlope && player.isGrounded && hitLayer != 9) //Si no comprobamos si estamos sobre el piso al chocar contra una pared lo identifica como una pendiente y nos da un impulso involuntario hacia atras
            {
                //Movemos al personaje en el eje X y Z en el vector de la normal del plano en donde estamos parados, la velosidad se consigue multiplicando el dato del eje y de la notmal por
                //el vector de la normal y la velosidad base que deseamos
                movePlayer.x += ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
                movePlayer.z += ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;
                movePlayer.y -= slopeForceDown;

            }
            else if (Vector3.Angle(Vector3.up, hitNormal) > 15 && player.isGrounded)
            {
                movePlayer.y -= slopeForceDown;
            }
        }
        else
        {
            if (player.isGrounded && hitLayer != 9) //Si no comprobamos si estamos sobre el piso al chocar contra una pared lo identifica como una pendiente y nos da un impulso involuntario hacia atras
            {
                //Movemos al personaje en el eje X y Z en el vector de la normal del plano en donde estamos parados, la velosidad se consigue multiplicando el dato del eje y de la notmal por
                //el vector de la normal y la velosidad base que deseamos
                movePlayer.x += ((1f - hitNormal.y) * hitNormal.x) * (slideVelocity * 2);
                movePlayer.z += ((1f - hitNormal.y) * hitNormal.z) * (slideVelocity * 2);
                movePlayer.y -= slopeForceDown;

            }
          
        }
       
       
    }

    public void AddDistancePlatformRotating()
    {
        movePlayer.z -= CylinderDistanceTraveled.z * 60;
       
    }

    public void IsStairs()
    {
        if (upStair && !downStair)
        {
            fallVelocity = 0;
            movePlayer.y = fallVelocity;
        }
        
    }

    public void PlayerDeath()
    {
        Debug.Log("Player death");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitTag = hit.gameObject.tag;
        hitNormal = hit.normal;
        hitLayer = hit.gameObject.layer;
    }

    private void OnAnimatorMove()
    {
        
    }

}
