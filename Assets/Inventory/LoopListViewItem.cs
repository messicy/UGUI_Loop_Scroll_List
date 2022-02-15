using UnityEngine;


public class LoopListViewItem : MonoBehaviour
{
    int mItemIndex = -1;
    int mItemId = -1;

    LoopListView mParentListView = null;
    string mItemPrefabName;
    RectTransform mCachedRectTransform;
    float mPadding;
    int mItemCreatedCheckFrameCount = 0;


    public int ItemCreatedCheckFrameCount
    {
        get { return mItemCreatedCheckFrameCount; }
        set { mItemCreatedCheckFrameCount = value; }
    }

    public float Padding
    {
        get { return mPadding; }
        set { mPadding = value; }
    }

    public RectTransform CachedRectTransform
    {
        get
        {
            if (mCachedRectTransform == null)
            {
                mCachedRectTransform = gameObject.GetComponent<RectTransform>();
            }
            return mCachedRectTransform;
        }
    }

    public string ItemPrefabName
    {
        get
        {
            return mItemPrefabName;
        }
        set
        {
            mItemPrefabName = value;
        }
    }

    public int ItemIndex
    {
        get
        {
            return mItemIndex;
        }
        set
        {
            mItemIndex = value;
        }
    }
    public int ItemId
    {
        get
        {
            return mItemId;
        }
        set
        {
            mItemId = value;
        }
    }

    public LoopListView ParentListView
    {
        get
        {
            return mParentListView;
        }
        set
        {
            mParentListView = value;
        }
    }

    public float TopY
    {
        get
        {
            return CachedRectTransform.anchoredPosition3D.y;
        }
    }

    public float BottomY
    {
        get
        {
            return CachedRectTransform.anchoredPosition3D.y - CachedRectTransform.rect.height;
        }
    }

}
