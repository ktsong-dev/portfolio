using JSONTable;
using Spine.Unity;
using Spine;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Guest : FSM
{
    protected EntityGuest m_cEntityGuest = null;
    protected SkeletonAnimation m_cSpineAnim = null;
    protected Skeleton m_cSkeleton = null;
    protected NavAgent m_cAgent = null;
    protected eDIRECTION m_eCurDirection = eDIRECTION.NONE; // 길찾기 이동 중 다음 웨이포인트로의 진행 방향
    protected EntityAIManager m_cEntityAIMng = null;


    ////////////////////////////////////////////////////////////
    /// 재정의
    public virtual void Talk()
    {
        m_isMoving = false;
        m_cAgent.Pause();
    }

    public virtual void TalkComplete()
    {
        m_isMoving = true;
        m_cAgent.Resume();
        m_eCurDirection = eDIRECTION.NONE;
    }

    public override void OnEnter(params object[] arrParams)
    {
        base.OnEnter(arrParams);

        m_cEntityGuest = GetComponent<EntityGuest>();
        m_cSpineAnim = GetComponentInChildren<SkeletonAnimation>();
        m_cSkeleton = m_cSpineAnim.skeleton;
        m_cAgent = m_cEntityGuest.m_cAgent;
        m_cEntityAIMng = EntityAIManager.Inst;

        // 플레이어 이전 진행방향 저장
        m_eCurDirection = eDIRECTION.NONE;
    }


    ////////////////////////////////////////////////////////////
    /// 구현
    public void CheckDirection(Vector3 vt3NextPos, string strAnimName = "")
    {
        eDIRECTION eDir = EntityManager.Inst.GetDirection(m_cAgent.transform.position, vt3NextPos);

        if (m_eCurDirection != eDIRECTION.NONE && m_eCurDirection == eDir ||
            m_eCurDirection != eDIRECTION.NONE && m_cAgent.transform.position == vt3NextPos)
            return;

        switch (eDir)
        {
            case eDIRECTION.LEFT_DOWN:
                m_cSkeleton.SetLocalScale(new Vector2(1, 1));
                if (string.IsNullOrEmpty(strAnimName))
                    SetAnimation("walk");
                else
                    SetAnimation(strAnimName);
                break;

            case eDIRECTION.LEFT_UP:
                m_cSkeleton.SetLocalScale(new Vector2(1, 1));
                if (string.IsNullOrEmpty(strAnimName))
                    SetAnimation("walk_back");
                else
                    SetAnimation(strAnimName + "_back");
                break;

            case eDIRECTION.RIGHT_DOWN:
                m_cSkeleton.SetLocalScale(new Vector2(-1, 1));
                if (string.IsNullOrEmpty(strAnimName))
                    SetAnimation("walk");
                else
                    SetAnimation(strAnimName);
                break;

            case eDIRECTION.RIGHT_UP:
                m_cSkeleton.SetLocalScale(new Vector2(-1, 1));
                if (string.IsNullOrEmpty(strAnimName))
                    SetAnimation("walk_back");
                else
                    SetAnimation (strAnimName + "_back");
                break;
        }

        m_eCurDirection = eDir;
    }

    public void SetAnimation(string strAnimName, string strDefaultAnimName = "", float fAnimSpeed = 1)
    {
        if (m_cSpineAnim.AnimationName.Equals(strAnimName) && m_cSpineAnim.AnimationState.TimeScale == fAnimSpeed)
            return;

        if (!IsExistAnimation(strAnimName))
        {
            if (string.IsNullOrEmpty(strDefaultAnimName))
                strAnimName = "idle";
            else
                strAnimName = strDefaultAnimName;
        }

        m_cSpineAnim.AnimationName = strAnimName;
        m_cSpineAnim.AnimationState.TimeScale = fAnimSpeed;
    }

    public bool IsExistAnimation(string strAnimName)
    {
        if (m_cSkeleton.Data.FindAnimation(strAnimName) != null)
            return true;

        return false;
    }

    public Vector3 GetOutPos(eAREA_TYPE eInputAreaType, eAREA_TYPE eOutAreaType)
    {
        if (eInputAreaType == eAREA_TYPE.MAIN)
        {
            Map cCurMap = null;
            if (eOutAreaType == eAREA_TYPE.THEME_A)
                cCurMap = MapService.GetMap(1);
            else if (eOutAreaType == eAREA_TYPE.THEME_B)
                cCurMap = MapService.GetMap(2);
            else
                cCurMap = MapService.GetMap(0);

            List<MapPointExit> lstExits = new List<MapPointExit>();
            for (int i = 0; i < cCurMap.exits.Length; i++)
            {
                if (m_cEntityGuest.m_cAgent.GetNearestPosition(cCurMap.exits[i].transform.position, out var _))
                    lstExits.Add(cCurMap.exits[i]);
            }
            
            return lstExits[Random.Range(0, lstExits.Count - 1)].transform.position;
        }
        else
        {
            return MapService.GetMap(0).transform.position;
        }
    }

    public void CheckChairDirection(int nChairDir)
    {
        switch (nChairDir)
        {
            case 1:
                m_cSkeleton.SetLocalScale(new Vector2(1, 1));
                break;

            case 2:
                m_cSkeleton.SetLocalScale(new Vector2(-1, 1));
                break;

            case 3:
                m_cSkeleton.SetLocalScale(new Vector2(-1, 1));
                break;

            case 4:
                m_cSkeleton.SetLocalScale(new Vector2(1, 1));
                break;
        }
    }

    public void StandUp(bool isGoldenBell = false)
    {
        // 의자 옆으로 이동
        MapObjectTable cMapObjTable = MapObjectManager.Inst.GetTable(m_cEntityGuest.gameObject);
        if (cMapObjTable != null)
        {
            Vector3 vt3ChairNearestPos = Vector3.zero;
            if (m_cEntityGuest.m_cAgent.GetNearestPosition(cMapObjTable.GetAttachedSeat().transform.position, out vt3ChairNearestPos))
                transform.position = vt3ChairNearestPos;

            // 테이블에 할당된 손님 오브젝트 해제
            MapObjectManager.Inst.ReleaseTable(gameObject);
        }
        
        m_cEntityGuest.OutTable();

        if (isGoldenBell)
            return;

        CalcItemDrop();

        // 돌발 퀘스트 발동
        if (!m_cEntityGuest.IS_ON_SUDDEN_QUEST && m_cEntityGuest.m_cGuestInfo.TABLE_DATA.Type == (int)eGUEST_TYPE.NORMAL)
        {
            // 추가 보상 아이템이 존재하지 않을 경우에만 돌발 퀘스트가 발동한다
            if (m_cEntityGuest.m_cGuestInfo.GetFirstReward() == null)
                return;

            StartSuddenQuest();
        }
    }

    // 추가 획득 아이템 체크
    void CalcItemDrop()
    {
        bool isFixedShowItem = false;   // 손님 머리위에 보여질 아이템 아이콘이 결정났는지 여부 체크
        GuestCombo cGuestComboDb = null;

        // 특별 손님 콤보 횟수에 따른 추가 영혼석
        if (m_cEntityGuest.m_cGuestInfo.TABLE_DATA.Type == (int)eGUEST_TYPE.SPEC && m_cEntityGuest.TOTAL_ORDER_CNT > 0)
        {
            cGuestComboDb = dbGame.GuestCombo.Get(m_cEntityGuest.TOTAL_ORDER_CNT);

            if (cGuestComboDb.SoulStoneCnt > 0)
            {
                m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo((int)eITEM_TYPE_RANGE.SOULSTONE, m_cEntityGuest.m_cGuestInfo.TABLE_DATA.SoulStone, cGuestComboDb.SoulStoneCnt);

                if (!isFixedShowItem)
                {
                    var cSoulStoneDb = dbGame.SoulStone.Get(m_cEntityGuest.m_cGuestInfo.TABLE_DATA.SoulStone);
                    m_cEntityGuest.SetItemSoulstoneIcon(cSoulStoneDb.Type, cSoulStoneDb.Idx);
                    isFixedShowItem = true;
                }
            }
        }

        // 섭취한 음식에 따른 획득 아이템 계산
        foreach (FoodInfo cFood in m_cEntityGuest.m_cGuestInfo.m_lstEatFoods)
        {
            if (cFood.m_nCount <= 0)
                continue;

            Recipe cRecipeDb = dbGame.Recipe.Get(cFood.m_nId);

            for (int i = 1; i <= cFood.m_nCount; i++)
            {
                if (ValueHelper.ChanceRandom(cRecipeDb.DropSoulRatio + m_cEntityGuest.m_cGuestInfo.TABLE_DATA.DropSoulRatio))
                {
                    m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo((int)eITEM_TYPE_RANGE.SOULSTONE, m_cEntityGuest.m_cGuestInfo.TABLE_DATA.SoulStone, 1);

                    if (!isFixedShowItem)
                    {
                        var cSoulStoneDb = dbGame.SoulStone.Get(m_cEntityGuest.m_cGuestInfo.TABLE_DATA.SoulStone);
                        m_cEntityGuest.SetItemSoulstoneIcon(cSoulStoneDb.Type, cSoulStoneDb.Idx);
                        isFixedShowItem = true;
                    }
                }

                // 일반 아이템 획득 여부 체크
                if (ValueHelper.ChanceRandom(cRecipeDb.DropGroupRatio))
                {
                    foreach (var cReward in dbGame.RecipeRewardGroup.ByGroup.Get(cRecipeDb.DropItemGroup))
                    {
                        if (ValueHelper.ChanceRandom(cReward.Rate))
                        {
                            m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo(cReward.RewardType, cReward.RewardIdx, Random.Range(cReward.MinCount, cReward.MaxCount));

                            if (!isFixedShowItem)
                            {
                                dbGame.GetItem(cReward.RewardType, cReward.RewardIdx, out var cItemDb);
                                m_cEntityGuest.SetItemIcon(cItemDb.Icon);
                                isFixedShowItem = true;
                            }
                        }
                    }
                }
            }
        }

        // 특별 손님 콤보 횟수에 따른 추가 일반 아이템
        if (cGuestComboDb !=  null)
        {
            if (!isFixedShowItem)
            {
                if (cGuestComboDb.Reward1Type > 0)
                {
                    if (dbGame.GetItem(cGuestComboDb.Reward1Type, cGuestComboDb.Reward1ID, out var cItemDb))
                        m_cEntityGuest.SetItemIcon(cItemDb.Icon);
                }
            }

            m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo(cGuestComboDb.Reward1Type, cGuestComboDb.Reward1ID, cGuestComboDb.Reward1Cnt);
            m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo(cGuestComboDb.Reward2Type, cGuestComboDb.Reward2ID, cGuestComboDb.Reward2Cnt);
            m_cEntityGuest.m_cGuestInfo.AddRewardItemInfo(cGuestComboDb.Reward3Type, cGuestComboDb.Reward3ID, cGuestComboDb.Reward3Cnt);
        }
    }

    public void StartSuddenQuest()
    {
        if (Model.Object.QuestSuddenlyObject.Event(m_cEntityGuest.m_cGuestInfo.m_nTableId, out JSONTable.Quest result))
        {
            m_cEntityGuest.HUD.SetStateSuddenQuest(result);
            m_cEntityGuest.IS_ON_SUDDEN_QUEST = true;
        }
    }
}