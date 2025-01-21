using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PopupUI : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// 멤버 변수
    protected Animator m_cOpenAnim = null;
    protected Action OnCompleteOpenAnimAction = null;   // 팝업이 열리고 닫힐 때 애니메이션 실행이 끝난 후 호출
    private Action<PopupUI> OnHidePopupAction = null;   // 팝업이 닫힐 때 실행될 액션이 있을경우 외부에서 등록

    ////////////////////////////////////////////////////////////
    /// 재정의
    protected virtual void AddEventListener() { }
    protected virtual void RemoveEventListener() { }


    ////////////////////////////////////////////////////////////
    /// 유니티 내장함수
    void Start()
    {

    }

    void Update()
    {

    }


    ////////////////////////////////////////////////////////////
    /// 멤버 함수
    public virtual void Show(params object[] arrParams)
    {
        gameObject.SetActive(true);
        GameService.SetPlayerMove(true);

        AddEventListener();
        StopAllCoroutines();

        m_cOpenAnim = GetComponent<Animator>();

        OnCompleteOpenAnimAction = () =>
        {
            
        };

        if (m_cOpenAnim != null && m_cOpenAnim.enabled)
        {
            m_cOpenAnim.SetTrigger("");
            m_cOpenAnim.Update(0);

            StartCoroutine(CompleteOpenAnim());
        }
        else
        {
            OnCompleteOpenAnimAction();
        }

    }

    public virtual void Hide()
    {
        if (!gameObject.activeSelf)
            return;

        StopAllCoroutines();

        OnCompleteOpenAnimAction = () =>
        {
            RemoveEventListener();

            OnHidePopupAction?.Invoke(this);

            RemoveHideAction();

            gameObject.SetActive(false);

            PopupManager.Inst.ResetOrderInLayer();
        };

        if (m_cOpenAnim != null && m_cOpenAnim.enabled)
        {
            m_cOpenAnim.SetTrigger("");
            m_cOpenAnim.Update(0);

            StartCoroutine(CompleteOpenAnim());
        }
        else
        {
            OnCompleteOpenAnimAction();
        }

        PopupManager.Inst.PopUIStack();
    }

    IEnumerator CompleteOpenAnim()
    {
        yield return new WaitUntil(() => !m_cOpenAnim.IsInTransition(0));

        while (m_cOpenAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            yield return null;

        OnCompleteOpenAnimAction?.Invoke();
    }

    public PopupUI AddHideAction(Action<PopupUI> onHidePopupAction)
    {
        OnHidePopupAction += onHidePopupAction;
        return this;
    }

    public void RemoveHideAction()
    {
        OnHidePopupAction = null;
    }

    public virtual void OnClickBtnClose()
    {
        Hide();
    }
}