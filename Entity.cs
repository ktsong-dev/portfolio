using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum eDIRECTION
{
    NONE,
    LEFT_UP,
    RIGHT_UP,
    LEFT_DOWN,
    RIGHT_DOWN,
}

public class Entity : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// 멤버 변수
    public NavAgent m_cAgent = null;
    public SkeletonAnimation m_cSpineAnim = null;
    protected Map m_cMap = null;


    ////////////////////////////////////////////////////////////
    /// 재정의
    public virtual void Initialize()
    {
        if (m_cAgent == null)
            m_cAgent = GetComponentInChildren<NavAgent>();

        if (m_cSpineAnim == null)
            m_cSpineAnim = GetComponentInChildren<SkeletonAnimation>();

        foreach (Map m in FindObjectsOfType<Map>())
        {
            if (m.name == "MAP Main") SetMap(m);
        }
    }

    public virtual void OnTap()
    {
    }


    ////////////////////////////////////////////////////////////
    /// 구현
    public T AddFSMComponent<T>() where T : FSM
    {
        T retVal = GetComponent<T>();
        if (retVal == null)
            retVal = gameObject.AddComponent<T>();

        return retVal;
    }

    public T GetState<T>() where T : Behaviour
    {
        return GetComponent<T>();
    }

    public Map GetMap()
    {
        return m_cMap;
    }

    public void SetMap(Map cMap)
    {
        m_cMap = cMap;
    }

    public void SetMoveSpeed(float fRatio)
    {
        m_cAgent.speed = m_cAgent.GetDefaultSpeed() + m_cAgent.GetDefaultSpeed() * fRatio;
        m_cSpineAnim.timeScale = 1 + (1 * fRatio);
    }

    public void SetDefaultMoveSpeed()
    {
        m_cAgent.SetSpeed(m_cAgent.GetDefaultSpeed());
        m_cSpineAnim.timeScale = 1;
    }

    public void SetActiveSpineModel(bool isActive)
    {
        GetComponentInChildren<MeshRenderer>().enabled = isActive;
        m_cSpineAnim.enabled = isActive;
    }
}