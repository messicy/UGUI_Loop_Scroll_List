using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[System.Serializable]
public class ItemPrefabConfData
{
    public GameObject mItemPrefab = null;
    public float mPadding = 0;
}


public class LoopListView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    ItemPool mPool = new ItemPool();

    [SerializeField]
    ItemPrefabConfData mItemPrefabData = new ItemPrefabConfData();

    List<LoopListViewItem> mItemList = new List<LoopListViewItem>();
    RectTransform mContainerTrans;
    ScrollRect mScrollRect = null;
    RectTransform mViewPortRectTransform = null;
    int mItemTotalCount = 0;
    System.Func<LoopListView, int, LoopListViewItem> mOnGetItemByIndex;
    Vector3[] mItemWorldCorners = new Vector3[4];
    Vector3[] mViewPortRectLocalCorners = new Vector3[4];
    int mCurReadyMinItemIndex = 0;
    int mCurReadyMaxItemIndex = 0;
    bool mNeedCheckNextMinItem = true;
    bool mNeedCheckNextMaxItem = true;
    ItemPosMgr mItemPosMgr = null;
    float mDistanceForRecycle0 = 40;
    float mDistanceForNew0 = 35;
    float mDistanceForRecycle1 = 40;
    float mDistanceForNew1 = 35;

    bool mIsDraging = false;

    Vector3 mLastFrameContainerPos = Vector3.zero;
    Vector2 mAdjustedVec;
    bool mNeedAdjustVec = false;
    bool mListViewInited = false;
    int mListUpdateCheckFrameCount = 0;

    int mCurrentSelectIndex = -1; 

    public List<LoopListViewItem> ItemList
    {
        get
        {
            return mItemList;
        }
    }

    public void InitListView(int itemTotalCount,
        System.Func<LoopListView, int, LoopListViewItem> onGetItemByIndex)
    {
        mScrollRect = gameObject.GetComponent<ScrollRect>();
        if (mScrollRect == null)
        {
            Debug.LogError("ListView Init Failed! ScrollRect component not found!");
            return;
        }

        mContainerTrans = mScrollRect.content;
        mViewPortRectTransform = mScrollRect.viewport;

        mScrollRect.horizontal = false;
        mScrollRect.vertical = true;
        InitItemPool();
        mOnGetItemByIndex = onGetItemByIndex;
        if(mListViewInited == true)
        {
            Debug.LogError("LoopListView.InitListView method can be called only once.");
        }
        mListViewInited = true;
        ResetListView();
        mItemTotalCount = itemTotalCount;

        LoopListViewItem item = mItemPrefabData.mItemPrefab.GetComponent<LoopListViewItem>();
        if(item == null)
        {
            Debug.LogError("item template should has LoopListViewItem component.");
        }
        mItemPosMgr = new ItemPosMgr(itemTotalCount, item.CachedRectTransform.rect.height + mItemPrefabData.mPadding);
        mCurReadyMaxItemIndex = 0;
        mCurReadyMinItemIndex = 0;
        mNeedCheckNextMaxItem = true;
        mNeedCheckNextMinItem = true;
        UpdateContentSize();
    }

    public void ResetListView(bool resetPos = true)
    {
        mViewPortRectTransform.GetLocalCorners(mViewPortRectLocalCorners);
        if(resetPos)
        {
            mContainerTrans.anchoredPosition3D = Vector3.zero;
        }
    }

    public LoopListViewItem NewListViewItem()
    {
        LoopListViewItem item = mPool.GetItem();
        RectTransform rf = item.GetComponent<RectTransform>();
        rf.SetParent(mContainerTrans);
        rf.localScale = Vector3.one;
        rf.anchoredPosition3D = Vector3.zero;
        rf.localEulerAngles = Vector3.zero;
        item.ParentListView = this;
        return item;
    }


    void RecycleItemTmp(LoopListViewItem item)
    {
        if (item == null)
        {
            return;
        }

        mPool.RecycleItem(item);
    }


    void ClearAllTmpRecycledItem()
    {
        mPool.ClearTmpRecycledItem();
    }

    void InitItemPool()
    {
        if (mItemPrefabData.mItemPrefab == null)
        {
            Debug.LogError("Item prefab is null ");
        }
        string prefabName = mItemPrefabData.mItemPrefab.name;

        RectTransform rtf = mItemPrefabData.mItemPrefab.GetComponent<RectTransform>();
        if (rtf == null)
        {
            Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
        }
        LoopListViewItem tItem = mItemPrefabData.mItemPrefab.GetComponent<LoopListViewItem>();
        if (tItem == null)
        {
            mItemPrefabData.mItemPrefab.AddComponent<LoopListViewItem>();
        }
        mPool.Init(mItemPrefabData.mItemPrefab, mItemPrefabData.mPadding, mContainerTrans);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        mIsDraging = true;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        mIsDraging = false;
    }

    LoopListViewItem GetNewItemByIndex(int index)
    {
        if(index < 0)
        {
            return null;
        }
        if(mItemTotalCount > 0 && index >= mItemTotalCount)
        {
            return null;
        }
        LoopListViewItem newItem = mOnGetItemByIndex(this, index);
        if (newItem == null)
        {
            return null;
        }
        newItem.ItemIndex = index;
        newItem.ItemCreatedCheckFrameCount = mListUpdateCheckFrameCount;
        return newItem;
    }


    bool GetPlusItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
    {
        return mItemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
    }


    float GetItemPos(int itemIndex)
    {
        return mItemPosMgr.GetItemPos(itemIndex);
    }

    void Update()
    {
        if(mListViewInited == false)
        {
            return;
        }
        if(mNeedAdjustVec)
        {
            mNeedAdjustVec = false;
            if (mScrollRect.velocity.y * mAdjustedVec.y > 0)
            {
                mScrollRect.velocity = mAdjustedVec;
            }

        }
        UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
        ClearAllTmpRecycledItem();
        mLastFrameContainerPos = mContainerTrans.anchoredPosition3D;
    }

    public int CurrentSelectIndex { get => mCurrentSelectIndex; set => mCurrentSelectIndex = value; }

    public void UpdateListView(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
    {
        mListUpdateCheckFrameCount++;
        bool needContinueCheck = true;
        int checkCount = 0;
        int maxCount = 9999;
        while (needContinueCheck)
        {
            checkCount++;
            if (checkCount >= maxCount)
            {
                Debug.LogError("UpdateListView Vertical while loop " + checkCount + " times! something is wrong!");
                break;
            }
            needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1);
        }

    }



    bool UpdateForVertList(float distanceForRecycle0,float distanceForRecycle1,float distanceForNew0, float distanceForNew1)
    {
        int itemListCount = mItemList.Count;
        if (itemListCount == 0)
        {
            float curY = mContainerTrans.anchoredPosition3D.y;
            if (curY < 0)
            {
                curY = 0;
            }
            int index = 0;
            float pos = -curY;
            if (GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos) == false)
            {
                return false;
            }
            pos = -pos;
            LoopListViewItem newItem = GetNewItemByIndex(index);
            if (newItem == null)
            {
                return false;
            }
            mItemList.Add(newItem);
            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(0, pos, 0);
            UpdateContentSize();
            return true;
        }
        LoopListViewItem tViewItem0 = mItemList[0];
        tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
        Vector3 topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
        Vector3 downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

        if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
            && downPos0.y - mViewPortRectLocalCorners[1].y > distanceForRecycle0)
        {
            mItemList.RemoveAt(0);
            RecycleItemTmp(tViewItem0);

            return true;
        }

        LoopListViewItem tViewItem1 = mItemList[mItemList.Count - 1];
        tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
        Vector3 topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
        Vector3 downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
        if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
            && mViewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
        {
            mItemList.RemoveAt(mItemList.Count - 1);
            RecycleItemTmp(tViewItem1);

            return true;
        }

        if (mViewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
        {
            if(tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
            {
                mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                mNeedCheckNextMaxItem = true;
            }
            int nIndex = tViewItem1.ItemIndex + 1;
            if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
            {
                LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                if (newItem == null)
                {
                    mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                    mNeedCheckNextMaxItem = false;
                    CheckIfNeedUpdataItemPos();
                }
                else
                {
                    mItemList.Add(newItem);
                    float y = tViewItem1.CachedRectTransform.anchoredPosition3D.y - tViewItem1.CachedRectTransform.rect.height - tViewItem1. Padding;
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(0, y, 0);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();

                    if (nIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = nIndex;
                    }
                    return true;
                }
                        
            }

        }

        if (topPos0.y - mViewPortRectLocalCorners[1].y < distanceForNew0)
        {
            if(tViewItem0.ItemIndex < mCurReadyMinItemIndex)
            {
                mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                mNeedCheckNextMinItem = true;
            }
            int nIndex = tViewItem0.ItemIndex - 1;
            if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
            {
                LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                if (newItem == null)
                {
                    mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                    mNeedCheckNextMinItem = false;
                }
                else
                {
                    mItemList.Insert(0, newItem);
                    float y = tViewItem0.CachedRectTransform.anchoredPosition3D.y + newItem.CachedRectTransform.rect.height + newItem.Padding;
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(0, y, 0);
                    UpdateContentSize();
                    CheckIfNeedUpdataItemPos();
                    if (nIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = nIndex;
                    }
                    return true;
                }
                        
            }

        }

        return false;

    }

    float GetContentPanelSize()
    {
        float tTotalSize = mItemPosMgr.mTotalSize > 0 ? (mItemPosMgr.mTotalSize - mItemPrefabData.mPadding) : 0;
        if (tTotalSize < 0)
        {
            tTotalSize = 0;
        }
        return tTotalSize;
    }


    void CheckIfNeedUpdataItemPos()
    {
        int count = mItemList.Count;
        if (count == 0)
        {
            return;
        }

        LoopListViewItem firstItem = mItemList[0];
        LoopListViewItem lastItem = mItemList[mItemList.Count - 1];
        float viewMaxY = GetContentPanelSize();
        if (firstItem.TopY > 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.TopY != 0))
        {
            UpdateAllShownItemsPos();
            return;
        }
        if ((-lastItem.BottomY) > viewMaxY || (lastItem.ItemIndex == mCurReadyMaxItemIndex && (-lastItem.BottomY) != viewMaxY))
        {
            UpdateAllShownItemsPos();
            return;
        }
    }


    void UpdateAllShownItemsPos()
    {
        int count = mItemList.Count;
        if (count == 0)
        {
            return;
        }

        mAdjustedVec = (mContainerTrans.anchoredPosition3D - mLastFrameContainerPos) / Time.deltaTime;

        float pos = 0;
        pos = -GetItemPos(mItemList[0].ItemIndex);
        float pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.y;
        float d = pos - pos1;
        float curY = pos;
        for (int i = 0; i < count; ++i)
        {
            LoopListViewItem item = mItemList[i];
            item.CachedRectTransform.anchoredPosition3D = new Vector3(0, curY, 0);
            curY = curY - item.CachedRectTransform.rect.height - item.Padding;
        }
        if (d != 0)
        {
            Vector2 p = mContainerTrans.anchoredPosition3D;
            p.y = p.y - d;
            mContainerTrans.anchoredPosition3D = p;
        }

        if (mIsDraging)
        {
            mScrollRect.Rebuild(CanvasUpdate.PostLayout);
            mScrollRect.velocity = mAdjustedVec;
            mNeedAdjustVec = true;
        }
    }
    void UpdateContentSize()
    {
        float size = GetContentPanelSize();
        if (mContainerTrans.rect.height != size)
        {
            mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }
    }
}

