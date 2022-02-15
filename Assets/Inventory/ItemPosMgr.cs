
public class ItemSizeGroup
{

    public float[] mItemSizeArray = null;
    public float[] mItemStartPosArray = null;
    public int mItemCount = 0;
    public float mGroupSize = 0;
    public float mGroupStartPos = 0;
    public float mGroupEndPos = 0;
    float mItemDefaultSize = 0;
    public ItemSizeGroup(float itemDefaultSize, int itemCount)
    {
        mItemDefaultSize = itemDefaultSize;
        mItemCount = itemCount;
        Init();
    }

    public void Init()
    {
        mItemSizeArray = new float[mItemCount];
        if (mItemDefaultSize != 0)
        {
            for (int i = 0; i < mItemSizeArray.Length; ++i)
            {
                mItemSizeArray[i] = mItemDefaultSize;
            }
        }
        mGroupSize = mItemDefaultSize * mItemSizeArray.Length;

        mItemStartPosArray = new float[mItemCount];
        mItemStartPosArray[0] = 0;
        for (int i = 1; i < mItemCount; ++i)
        {
            mItemStartPosArray[i] = mItemStartPosArray[i - 1] + mItemSizeArray[i - 1];
        }

        mGroupStartPos = 0;
        mGroupEndPos = mGroupSize;
    }

    public float GetItemStartPos(int index)
    {
        return mGroupStartPos + mItemStartPosArray[index];
    }

    public int GetItemIndexByPos(float pos)
    {
        if (mItemCount == 0)
        {
            return -1;
        }
            
        int low = 0;
        int high = mItemCount - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            float startPos = mItemStartPosArray[mid];
            float endPos = startPos + mItemSizeArray[mid];
            if (startPos <= pos && endPos >= pos)
            {
                return mid;
            }
            else if (pos > endPos)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return -1;
    }
}

public class ItemPosMgr
{
    ItemSizeGroup mItemSizeGroup;
    public float mTotalSize = 0;

    public ItemPosMgr(int maxCount, float itemDefaultSize)
    {
        mItemSizeGroup = new ItemSizeGroup(itemDefaultSize, maxCount);
        mTotalSize = mItemSizeGroup.mGroupSize;
    }

    public float GetItemPos(int itemIndex)
    {
        return mItemSizeGroup.GetItemStartPos(itemIndex);
    }

    public bool GetItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
    {
        index = 0;
        itemPos = 0f;

        int hitIndex = -1;
        hitIndex = mItemSizeGroup.GetItemIndexByPos(pos - mItemSizeGroup.mGroupStartPos);
        if (hitIndex < 0)
        {
            return false;
        }
        index = hitIndex;
        itemPos = mItemSizeGroup.GetItemStartPos(hitIndex);

        return true;
    }
}
