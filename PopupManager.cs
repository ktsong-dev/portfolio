using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public enum ePOPUP_TYPE { NONE, SYSTEM, NORMAL }

    Stack<Tuple<PopupUI,Element>> uiStack = new Stack<Tuple<PopupUI, Element>>();

    ////////////////////////////////////////////////////////////
    /// 멤버 변수
    private static PopupManager m_Inst = null;

    public static PopupManager Inst
    {
        get
        {
            if (m_Inst == null)
            {
                var obj = new GameObject("PopupManager");
                obj.transform.SetParent(InstanceService.BaseCanvasTransform, false);
                m_Inst = obj.AddComponent<PopupManager>();

                var cRectTransform = obj.AddComponent<RectTransform>();
                cRectTransform.anchoredPosition = Vector3.zero;
                cRectTransform.anchorMin = new Vector2(0, 0);
                cRectTransform.anchorMax = new Vector2(1, 1);
                cRectTransform.offsetMin = Vector2.zero;
                cRectTransform.offsetMax = Vector2.zero;
                cRectTransform.localPosition = Vector3.zero;
                cRectTransform.localScale = Vector3.one;
            }

            return m_Inst;
        }
    }

    private Dictionary<string, PopupUI> m_dicPopups = new Dictionary<string, PopupUI>();
    private List<PopupUI> m_lstOpenedPopups = new List<PopupUI>();
    private int m_nOrderInLayer = 0;    // 캔바스의 소팅 오더 지정용

    public int CUR_SORTING_ORDER { get { return m_nOrderInLayer; } }

    ////////////////////////////////////////////////////////////
    /// 유니티 내장함수
    private void Awake()
    {
        if (m_Inst == null)
            m_Inst = this;

    }

    private void OnDestroy()
    {
        m_dicPopups.Clear();
        m_lstOpenedPopups.Clear();

        m_dicPopups = null;
        m_lstOpenedPopups = null;
    }


    ////////////////////////////////////////////////////////////
    /// 멤버 함수
    public UISystemPopup ShowSystemPopup(UISystemPopup.eTYPE eType, string strTitle, string strDesc, string strImage = "", Action cActionOK = null, Action cActionCancel = null, int nOrderInLayer = 0, params object[] arrParam)
    {
        var cUISystemPopup = MakePopup<UISystemPopup>();
        //string strUISystemPopup = typeof(UISystemPopup).Name;
        //if (m_dicPopups.ContainsKey(strUISystemPopup))
        //    cUISystemPopup = m_dicPopups[strUISystemPopup] as UISystemPopup;
        //else
        //{
        //    GameObject goSystemPopup = Instantiate(Resources.Load(strUISystemPopup), Vector3.zero, Quaternion.identity) as GameObject;
        //    goSystemPopup.name = strUISystemPopup;
        //    goSystemPopup.transform.SetParent(transform);
        //    cUISystemPopup = goSystemPopup.GetComponent<UISystemPopup>();
        //    m_dicPopups.Add(strUISystemPopup, cUISystemPopup);
        //}

        return cUISystemPopup.Show(eType, strTitle, strDesc, strImage, cActionOK, cActionCancel);
    }

    public T Show<T>(params object[] arrParams) where T : PopupUI => Show(typeof(T).Name, arrParams) as T;
    public PopupUI Show(string strPopupPrefab, params object[] arrParams)
    {
        var cPopupUI = MakePopup(strPopupPrefab);

        Show(cPopupUI);
        cPopupUI.Show(arrParams);
        
        return cPopupUI;
    }

    public UIToastMessage ShowToastMessage(string strMessage)
    {
        var cPopupUI = MakePopup<UIToastMessage>();

        Show(cPopupUI);
        cPopupUI.Show(strMessage);
        return cPopupUI;
    }

    void Show(PopupUI cPopupUI)
    {
        var cChildCanvas = cPopupUI.GetComponentInChildren<Canvas>(true);
        if (cChildCanvas != null)
        {
            m_nOrderInLayer += 10;  // UI에 스파인 등이 올라가 있어 팝업 내에서 여러개의 소팅오더가 적용되어야 하는 경우가 발생하여 팝업 간 소팅오더 10씩 증가시킴

            cChildCanvas.sortingOrder = m_nOrderInLayer;
        }

        PushUIStack(cPopupUI);

        if (!m_lstOpenedPopups.Contains(cPopupUI))
            m_lstOpenedPopups.Add(cPopupUI);
    }

    public void AllPopupClose()
    {
        foreach (var cPopupUI in m_lstOpenedPopups)
            cPopupUI.Hide();

        m_lstOpenedPopups.Clear();
    }

    // 팝업이 닫힐 때마다 체크하며 활성화 된 UI가 없을 경우만 소팅오더 조절값을 초기화 시킴
    public void ResetOrderInLayer()
    {
        int nActiveCnt = 0;
        foreach (var cPopupUI in m_lstOpenedPopups)
        {
            if (cPopupUI.gameObject.activeSelf)
                nActiveCnt++;
        }

        if (nActiveCnt == 0)
        {
            // TODO: 다람쥐 이동 복구
            GameService.SetPlayerMove(false);
            m_nOrderInLayer = 0;
        }
    }

    T MakePopup<T>() where T : PopupUI => MakePopup(typeof(T).Name) as T;
    PopupUI MakePopup(string strPopupPrefab)
    {
        if (m_dicPopups.TryGetValue(strPopupPrefab, out var popup))
            return popup;

        var goPopup = Instantiate(Resources.Load(strPopupPrefab), Vector3.zero, Quaternion.identity, gameObject.transform) as GameObject;
        goPopup.name = strPopupPrefab;

        var cPopupUI = goPopup.GetComponent<PopupUI>();
        var rect = cPopupUI.GetComponent<RectTransform>();
        cPopupUI.transform.localPosition = Vector3.zero;
        cPopupUI.transform.localScale = Vector3.one;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        m_dicPopups[strPopupPrefab] = cPopupUI;
        return cPopupUI;
    }


    public T Get<T>() where T : PopupUI => Get(typeof(T).Name) as T;
    public PopupUI Get(string strPopupName) => m_dicPopups.TryGetValue(strPopupName, out var popup) ? popup : null;

    public static bool IsOpenedAnyUI()
    {
        foreach (var entry in Inst.m_lstOpenedPopups)
        {
            if (entry.gameObject.activeSelf)
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopupManager.Inst.OnBackKey();
        }
    }

public void OnBackKey()
    {
        if(uiStack.Count > 0)
        {
            var item = uiStack.Peek();
            if(item.Item1 != null)
            {
                item.Item1.Hide();
                PopupManager.Inst.PopUIStack();
            }                
            else if(item.Item2 != null)
            {
                if(item.Item2 is QuestView)
                {
                    (item.Item2 as QuestView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
                else if (item.Item2 is QuestInfoView)
                {
                    (item.Item2 as QuestInfoView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
                else if (item.Item2 is MapEditView)
                {
                    (item.Item2 as MapEditView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
                else if (item.Item2 is FurnitureShopView)
                {
                    (item.Item2 as FurnitureShopView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
                else if (item.Item2 is CashShopView)
                {
                    (item.Item2 as CashShopView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
                else if (item.Item2 is EventView)
                {
                    (item.Item2 as EventView).OnBackButton();
                    PopupManager.Inst.PopUIStack();
                }
            }
        }
        else
        {
            ShowSystemPopup(UISystemPopup.eTYPE.OK_CANCEL, string.Empty, Util_UI.LocalizedText("Game_Exit"), "", () => Main.Quit());
        }
    }

    public void PopUIStack()
    {
        if (uiStack.Count > 0)
        {
            uiStack.Pop();
        }
    }

    public void PushUIStack(Element elem)
    {
        uiStack.Push(new Tuple<PopupUI, Element>(null, elem));
    }

    public void PushUIStack(PopupUI popup)
    {
        uiStack.Push(new Tuple<PopupUI, Element>(popup, null));
    }


}
