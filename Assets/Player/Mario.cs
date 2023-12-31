using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;
using System;
using Photon.Realtime;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Mario : MonoBehaviour
{

    // marioMode_0: smallMario, marioMode_1: bigMario, marioMode_2: fireMario
    [Header("Camera")]
    public GameObject virtual_camera;


    [Header("Move Info")]
    public float moveSpeed = 5;
    public float runSpeed = 6;
    public float jumpPower;
    public float lastXSpeed;

    [Header("Ground Check")]
    public Transform obj_isGround;
    public Transform obj_isPlayerA;
    public Transform obj_isPlayerB;
    public Transform obj_isWallA;
    public Transform obj_isWallB;
    public float groundCheckDist;
    public float playerCheckDist;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public Transform obj_bulletGeneratorA;
    public Transform obj_bulletGeneratorB;


    [Header("Audio source")]
    public AudioSource jump_audioSource;
    public AudioClip[] clips;

    public GameObject endingEffect;

    public int marioMode = 0;   // 0: 일반 마리오, 1 : 빅마리오, 2: 꽃 마리오
    public bool isStarMario = false;

    //public int maxBullet = 2;

	//Star Mode
	bool starCoroutineTrigger = true;
    float starTime = 8f;
    float starTimer = 0;
    Coroutine starModeColor;
    Color[] color = {
        Color.white,
        Color.red,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.cyan,
        Color.magenta
    };
    int colorIndex = 0;
    float colorChangeSec = 0.2f;

    // Component
    [HideInInspector] public Rigidbody2D rb;
    public CapsuleCollider2D collider;
    public CapsuleCollider2D collider_big;
    public Transform check_body;
    [HideInInspector] public PhysicsMaterial2D PM;
    [HideInInspector] public Animator anim;
    [HideInInspector] public SpriteRenderer spriteRenderer;


    //StateMachine
    public Mario_stateMachine stateMachine;

    public Mario_idle idleState;
    public Mario_run runState;
    public Mario_jump jumpState;
    public Mario_slide slideState;
    public Mario_walk walkState;
    public Mario_kicked kickedState;
    public Mario_sitDown sitDown;
    public Mario_die dieState;
    public Mario_stamp stampState;
    public Mario_BigSmall bigSmall;
    public Mario_SmallBig smallBig;
    public Mario_smallFire smallFire;
    public Mario_bigFire bigFire;
    public Mario_win winState;

    public Mario_Shell_idle mario_Shell_Idle;
    public Mario_Shell_jump mario_Shell_Jump;
    public Mario_Shell_run mario_Shell_Run;
    public Mario_Shell_stamp mario_Shell_Stamp;
    public Mario_Shell_walk mario_Shell_Walk;
    public Mario_Shell_Slide mario_Shell_Slide;

    public PhotonView PV;

    public GameObject pickedShell;
    

    private void Awake()
    {
        //PhotonNetwork.SendRate = 60;
        //PhotonNetwork.SerializationRate = 30;

        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        //collider = GetComponent<CapsuleCollider2D>();
        //PM = rb.GetComponent<PhysicsMaterial2D>();
        //collider.sharedMaterial = PM;
        PM = new PhysicsMaterial2D();
        collider.sharedMaterial = PM;
        collider_big.sharedMaterial = PM;
        //PM = collider.GetComponent<PhysicsMaterial2D>();

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        stateMachine = new Mario_stateMachine();

        idleState = new Mario_idle(this, stateMachine, "Idle");
        walkState = new Mario_walk(this, stateMachine, "Walk");
        runState = new Mario_run(this, stateMachine, "Run");
        jumpState = new Mario_jump(this, stateMachine, "Jump");
        slideState = new Mario_slide(this, stateMachine, "Slide");
        kickedState = new Mario_kicked(this, stateMachine, "Kicked");
        sitDown = new Mario_sitDown(this, stateMachine, "Sit");
        dieState = new Mario_die(this, stateMachine, "Die");
        stampState = new Mario_stamp(this, stateMachine, "Jump");
        bigSmall = new Mario_BigSmall(this, stateMachine, "BigToSmall");
        smallBig = new Mario_SmallBig(this, stateMachine, "SmallToBig");
        smallFire = new Mario_smallFire(this, stateMachine, "SmallToFire");
        bigFire = new Mario_bigFire(this, stateMachine, "BigToFire");
        winState = new Mario_win(this, stateMachine, "Win");

        mario_Shell_Idle = new Mario_Shell_idle(this, stateMachine, "Idle");
        mario_Shell_Jump = new Mario_Shell_jump(this, stateMachine, "Shell_Jump");
        mario_Shell_Run = new Mario_Shell_run(this, stateMachine, "Run");
        mario_Shell_Stamp = new Mario_Shell_stamp(this, stateMachine, "Shell_Jump");
        mario_Shell_Walk = new Mario_Shell_walk(this, stateMachine, "Walk");
        mario_Shell_Slide = new Mario_Shell_Slide(this, stateMachine, "Slide");
    }

    [PunRPC]
    public void Flip(bool a)
    {
        spriteRenderer.flipX = a;
    }

    private void Start()
    {
        //if(!GetComponent<PhotonView>().IsMine) return ;
        stateMachine.InitState(idleState);

        if (GameObject.Find("Virtual Camera") != null && GetComponent<PhotonView>().IsMine)
        {
            virtual_camera = GameObject.Find("Virtual Camera");
            virtual_camera.GetComponent<CinemachineVirtualCamera>().Follow = gameObject.transform;
        }
    }

    void Update()
    {
        //if (!GetComponent<PhotonView>().IsMine) return;
        //Debug.Log(GetComponent<PhotonView>().IsMine);
        stateMachine.currentState.Update();

        //star Mario
        starTimer -= Time.deltaTime;
        if (starTimer < 0 && isStarMario)
        {
            AudioManager.instance.PlayerOneShot(MARIO_SOUND.INGAME_BGM, true, 0);
            isStarMario = false;

        }
        if (isStarMario && starCoroutineTrigger)
        {
            starCoroutineTrigger = false;
            starModeColor = StartCoroutine(IStarModeColor());
        }
        else if (!isStarMario)
        {
            starCoroutineTrigger = true;
            if (starModeColor != null) StopCoroutine(starModeColor);
            spriteRenderer.color = Color.white;
        }

        if (Input.GetKeyDown(KeyCode.Delete) && PV.IsMine)
        {
            stateMachine.ChangeState(dieState);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && PV.IsMine)
        {
            if (transform.Find("Canvas_Chat") != null)
            {
                if (transform.Find("Canvas_Chat").gameObject.activeSelf)
                {
                    string ss = transform.Find("Canvas_Chat").Find("Chat_Input_Field").GetComponent<InputField>().text;
                    transform.Find("Canvas_Chat").Find("Chat_Input_Field").GetComponent<InputField>().text = "";
                    if(ss != "")
                        PV.RPC("RPC_Show_Chat", RpcTarget.All, ss);
                    Debug.Log(ss);
                    transform.Find("Canvas_Chat").gameObject.SetActive(false);


                }
                else
                {
                    transform.Find("Canvas_Chat").gameObject.SetActive(true);
                    Debug.Log(transform.Find("Canvas_Chat").Find("Chat_Input_Field"));
                    transform.Find("Canvas_Chat").Find("Chat_Input_Field").GetComponent<InputField>().ActivateInputField();
                }
                
                
            }
            
        }
    }

    [PunRPC]
    public void RPC_Show_Chat(string _chat)
    {
        StartCoroutine(Show_Chat(_chat));
        
    }

    IEnumerator Show_Chat(string _chat)
    {
        if (transform.Find("ChatBox") != null)
        {
            transform.Find("ChatBox").Find("ChatText").GetComponent<TMP_Text>().text = _chat;
            transform.Find("ChatBox").gameObject.SetActive(true);

            yield return new WaitForSeconds(3f);
            transform.Find("ChatBox").gameObject.SetActive(false);
        }
        yield break;
    }

    [PunRPC]
    public void SetCollider(int _val)
    {
        if (_val != 0)
        {
            collider.enabled = false;
            collider_big.enabled = true;
        }
        else
        {
            collider.enabled = true;
            collider_big.enabled = false;
        }
    }

    //// 부활 만들기
    //void Respawn()
    //{
    //	if (GetComponent<PhotonView>().IsMine)
    //	{
    //		// 로컬이면 체크 포인트에서 Respawn
    //		//PhotonNetwork.Instantiate()
    //	}
    //}

    IEnumerator IStarModeColor()
    {
        while (true)
        {
            if (!isStarMario) yield break;

            spriteRenderer.color = color[colorIndex++];

            if (colorIndex >= color.Length) colorIndex = 0;

            yield return new WaitForSeconds(colorChangeSec);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("enter");
        // starMode kill enemy script
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isStarMario)
            {
                if (collision.gameObject.GetComponent<Enemy>() != null)
                {
                    collision.gameObject.GetComponent<Enemy>().FilpOverDie();
                }
                else if (collision.gameObject.GetComponent<Enemy_shell>() != null
                    && collision.gameObject.GetComponent<Enemy_shell>().fsecMove == true)
                {
                    // kill moving enemy_shell
                    collision.gameObject.GetComponent<Enemy_shell>().FilpOverDie();
                }
                return;
            }
        }

        //밑에 적이 있음 == 죽으면 안 됨
        if (IsEnemyDetected() != null)
        {
            return;
        }
        else if (collision.gameObject.GetComponent<Enemy_shell>() != null && PV.IsMine)
        {
            //Debug.Log(collision.gameObject.GetComponent<Rigidbody2D>().velocity.x);
            // 멈춰있는 거북이 등딱지에 맞으면 삶
            if (collision.gameObject.GetComponent<Enemy_shell>().fsecMove == false)
            {
                //Debug.Log("등딱지 닿음");
                if (Input.GetKey(KeyCode.C))
                {
                    //Debug.Log("C 누르고 있음");
                    pickedShell = collision.gameObject;
                    pickedShell.GetComponent<Enemy_shell>().pickedState = true;
                    pickedShell.GetComponent<Enemy_shell>().pickedPlayer = gameObject;
                    pickedShell.GetComponent<Collider2D>().isTrigger = true;
                    pickedShell.GetComponent<Rigidbody2D>().gravityScale = 0;
                    pickedShell.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                    stateMachine.ChangeState(mario_Shell_Idle);
                }
                else
                    collision.gameObject.GetComponent<Enemy_shell>().fsecMove = true;
                return;
            }
            else
            {
                if (marioMode > 0)
                {
                    stateMachine.ChangeState(bigSmall); // 움직이는 거북이 등딱지에 맞으면 죽음
                }
                else
                {
                    stateMachine.ChangeState(dieState); // 움직이는 거북이 등딱지에 맞으면 죽음
                }
                return;
            }
        }
        else if (collision.gameObject.GetComponent<Goomba>() != null && collision.gameObject.GetComponent<Goomba>().isFlat)
        {
            // isFlat Goomba No die
        }
        else if (collision.gameObject.CompareTag("Enemy") && IsEnemyDetected() == null)
        {
            Debug.Log("flag2");
            if (marioMode > 0)
            {
                Debug.Log("flag3");
                stateMachine.ChangeState(bigSmall); // 움직이는 거북이 등딱지에 맞으면 small mario됨
            }
            else
            {
                Debug.Log("flag4");
                //small mario가 맞으면 죽음
                stateMachine.ChangeState(dieState); // 움직이는 거북이 등딱지에 맞으면 죽음
            }
        }


    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Item
        if (collision.CompareTag("Item") && PV.IsMine)
        {

            if (collision.GetComponent<Item_Star>() != null)
            {
                // star
                AudioManager.instance.PlayerOneShot(MARIO_SOUND.STAR, false, 0);
                isStarMario = true;
                starTimer = starTime; // starTime init
                                      //Debug.Log("star 먹음!!!!!!!!!!!!!: " + isStarMario);
            }
            else if (collision.GetComponent<Item_mushroom>() != null)
            {
                // mushroom
                if (marioMode == 0)
                {
                    marioMode = 1;
                    stateMachine.ChangeState(smallBig);
                    //collision.gameObject.GetComponent<Rigidbody2D>().Sleep();
                }

			}
			else if (collision.gameObject.GetComponent<Item_flower>() != null)
			{
				Debug.Log("flower !!" + marioMode);
				if (marioMode == 0)
				{
					marioMode = 2;
					stateMachine.ChangeState(smallFire);
				}
				else if (marioMode == 1)
				{
					marioMode = 2;
					stateMachine.ChangeState(bigFire);
				}
			}

            collision.gameObject.GetComponent<Item>().Destroy_item();
            //Destroy(collision.gameObject);

		}

		else if (collision.CompareTag("DeadZone"))
		{
            Debug.Log("죽어");
			stateMachine.ChangeState(dieState);
        }

        if (collision.GetComponent<Flag>() != null && PV.IsMine)
        {
            stateMachine.ChangeState(winState);
            //string winnerMario = PhotonNetwork.NickName;

            // 모든 마리오의 rb를 꺼서 움직이지 못하게 하자
            // 다음 스테이지 갈 거면 GameEnd()에 bool 인자 주면 됨
            int winnerPlayerId = PV.ViewID; // 1001, 2001, 3001, ...
                                            // Debug.Log("winnerPlayerId" + winnerPlayerId);
            PV.RPC("GameEnd", RpcTarget.All, winnerPlayerId);
        }
    }

    [PunRPC]
    public void Photon_RigidBody_Off()
    {
        GetComponent<PhotonRigidbody2DView>().enabled = false;
    }

    [PunRPC]
    public void Photon_RigidBody_On()
    {
        GetComponent<PhotonRigidbody2DView>().enabled = true;
    }

    [PunRPC]
    public void GameEnd(int _winnerId)
    {
        // 이렇게만 쓰면 모든 컴퓨터에 있는 1등 마리오만 sleep됨
        //rb.Sleep();
        //rb.constraints = RigidbodyConstraints2D.FreezeAll;

        //Cinemachine 카메라 이동
        if (GameObject.Find("Virtual Camera") != null)
        {
            virtual_camera = GameObject.Find("Virtual Camera");
            Destroy(virtual_camera);
        }

        // 모든 플레이어 가져와서 다 Sleep 시켜줘야 함
        Mario[] allMario = GameObject.FindObjectsOfType<Mario>();
        Debug.Log("allMario.Length" + allMario.Length); // 2나옴
        Vector3 winnerPos = new Vector3(200, 200, -1);

        for (int i = 0; i < allMario.Length; i++)
        {
            //Debug.Log("allMario[i]: " + allMario[i]);

            allMario[i].GetComponent<PhotonRigidbody2DView>().enabled = false;

            // Debug.Log(allMario[i] + ": " + allMario[i].GetComponent<PhotonView>().ViewID);
            if (_winnerId == allMario[i].GetComponent<PhotonView>().ViewID)
            {
                Vector2 marioPos = allMario[i].gameObject.transform.position;
                winnerPos = new Vector3(marioPos.x, marioPos.y, -1);
            }
        }

        // 1등의 위치로 카메라 이동시켜주기
        StartCoroutine(MoveCamera(winnerPos));
    }

    
	IEnumerator MoveCamera(Vector3 _winnerPos)
	{
		Debug.Log("MoveCamera 코루틴 실행!!!!!!!");
		yield return new WaitForSeconds(0.5f);
		while (Vector2.Distance(Camera.main.transform.position, _winnerPos) >= 1f)
		{
			Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, _winnerPos, 0.35f);
			yield return null;
		}
    }

    //public bool IsGroundDetected() => Physics2D.Raycast(obj_isGround.position, Vector2.down, groundCheckDist, whatIsGround);
    public bool IsGroundDetected()
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(obj_isPlayerA.position, obj_isPlayerB.position, LayerMask.GetMask("Ground"));
        //Debug.Log("그라운드 트루");

        if (cols != null && cols.Length > 0) return true;
        //for (int i = 0; i < cols.Length; i++)
        //{
        //	if (cols[i].gameObject.CompareTag("Ground"))
        //	{
        //		Debug.Log("그라운드 true");
        //		return true;
        //	}
        //}

        //Debug.Log("그라운드 false");

        return false;
    }
    public GameObject IsPlayerDetected()
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(obj_isPlayerA.position, obj_isPlayerB.position, LayerMask.GetMask("Player"));

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].gameObject != this.gameObject && cols[i].gameObject.CompareTag("Player"))
            {
                return cols[i].gameObject;
            }
        }
        return null;
    }

    public GameObject IsEnemyDetected()
    {
        if (isStarMario) return null;
        Collider2D[] cols = Physics2D.OverlapAreaAll(obj_isPlayerA.position, obj_isPlayerB.position);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].gameObject != this.gameObject && cols[i].gameObject.CompareTag("Enemy"))
            {
                if (cols[i].gameObject.GetComponent<Enemy>() != null && PV.IsMine)
                    cols[i].gameObject.GetComponent<Enemy>().Die();
                return cols[i].gameObject;
            }
        }
        return null;
    }

    public bool IsWallDetected()
    {
        Vector3 positionA;
        Vector3 positionB;

        if (!stateMachine.currentState.isFlip)
        {
            positionA = obj_isWallA.localPosition;
            positionB = obj_isWallB.localPosition;
        }
        else
        {
            positionA = new Vector3(-obj_isWallA.localPosition.x, obj_isWallB.localPosition.y, obj_isWallA.localPosition.z);
            positionB = new Vector3(-obj_isWallB.localPosition.x, obj_isWallA.localPosition.y, obj_isWallB.localPosition.z);
        }

        Debug.DrawLine(transform.position + positionA, transform.position + positionB, Color.cyan);
        Collider2D[] cols = Physics2D.OverlapAreaAll(transform.position + positionA, transform.position + positionB);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].gameObject != this.gameObject && cols[i].gameObject.CompareTag("Ground"))
            {
                //Debug.Log(cols[i].name);
                return true;
            }
        }
        return false;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(obj_isGround.position,
            new Vector3(obj_isGround.position.x, obj_isGround.position.y - groundCheckDist));
        //Gizmos.DrawLine(obj_isWallA.position, obj_isWallB.position);
    }
}
