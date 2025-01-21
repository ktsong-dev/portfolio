using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 화난 애니 출력 후 맵 이탈 시킴
/// </summary>
public class StateGuestAngry : FSM_Guest
{
    private Action m_cHideAction = null;
    private float m_fWaitingDur = 0;

    private readonly float FLOAT_WAITING_DUR = 5;


    ////////////////////////////////////////////////////////////
    /// 재정의    
    public override void OnEnter(params object[] arrParams)
    {
        base.OnEnter();

        m_fWaitingDur = FLOAT_WAITING_DUR;

        if (m_cEntityGuest.m_cSpineAnim.AnimationName.Contains("back"))
            SetAnimation("angry_back");
        else
            SetAnimation("angry");

        // 맵 안으로 입장 처리
        m_cEntityGuest.SetEnterInRestaurant();
    }

    public override void OnExit()
    {
        base.OnExit();

        if (m_cHideAction != null)
        {
            m_cHideAction();
            m_cHideAction = null;
        }
    }

    public override void OnUpdate(params object[] arrParams)
    {
        base.OnUpdate(arrParams);

        if (m_fWaitingDur > 0)
        {
            m_fWaitingDur -= m_cEntityAIMng.GetDeltaTime();
            if (m_fWaitingDur <= 0)
                AngryMoveToOut();
        }
    }


    ////////////////////////////////////////////////////////////
    /// 구현
    void AngryMoveToOut()
    {
        // 의자에 앉아 있는 상태였으면 의자 옆으로 강제 이동
        MapObjectTable cMapObjTable = MapObjectManager.Inst.GetTable(gameObject);
        if (cMapObjTable)
        {
            Vector3 vt3ChairNearestPos = Vector3.zero;
            if (m_cEntityGuest.m_cAgent.GetNearestPosition(cMapObjTable.GetAttachedSeat().transform.position, out vt3ChairNearestPos))
                transform.position = vt3ChairNearestPos;
        }

        m_cEntityGuest.SetState(EntityGuest.eSTATE.MOVE_TO_OUT);
    }

    public void AddHideAction(Action cHideAction)
    {
        m_cHideAction = null;
        m_cHideAction = cHideAction;
    }
}