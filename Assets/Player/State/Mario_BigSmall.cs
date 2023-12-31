using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario_BigSmall : Mario_state
{
    public Mario_BigSmall(Mario _mario, Mario_stateMachine _stateMachine, string _animBoolName) : base(_mario, _stateMachine, _animBoolName)
    {
    }

    Vector3 pos;

    public override void Enter()
    {
        AudioManager.instance.PlayerOneShot(MARIO_SOUND.POWER_DOWN, false, 2);
        Debug.Log("big small");
        base.Enter();
        stateTimer = 68 * Time.deltaTime;
        mario.marioMode = 0;
        mario.transform.position -= new Vector3(0, 0.5f, 0);
        pos = mario.gameObject.transform.position ;
        mario.collider.enabled = false;
        mario.collider_big.enabled = false;
        mario.PV.RPC("Photon_RigidBody_Off", RpcTarget.AllBuffered);

    }

    public override void Exit()
    {
        base.Exit();
        mario.PV.RPC("Photon_RigidBody_On", RpcTarget.AllBuffered);
        mario.PV.RPC("SetCollider", RpcTarget.AllBuffered, 0);
        mario.collider.enabled = true;
    }

    public override void Update()
    {
        base.Update();
        //mario.transform.position = pos;
        mario.rb.velocity = new Vector2(xInput * mario.moveSpeed, 0 );
        if (stateTimer <= 0) stateMachine.ChangeState(mario.idleState);
    }
}
