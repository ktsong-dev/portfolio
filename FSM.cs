using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// 멤버 변수
    protected bool m_isMoving = false;  // 이동중인지 체크


    ////////////////////////////////////////////////////////////
    /// 유니티 기본 함수
    private void Update()
    {
        if (EntityAIManager.Inst.IS_FREEZING)
            return;

        OnUpdate();
    }

    ////////////////////////////////////////////////////////////
    /// 재정의
    public virtual void OnEnter(params object[] arrParams) { }
    public virtual void OnExit() { }
    public virtual void OnReEnter(params object[] arrParams) { }
    public virtual void OnUpdate(params object[] arrParams) { }


    ////////////////////////////////////////////////////////////
    /// 구현
    public void SetMoving(bool isEnable)
    {
        m_isMoving = isEnable;
    }
}