using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSkinData", menuName = "SkinData/ItemSkinData", order = 1)]
public class ItemData : ScriptableObject
{
    public ItemSkinData itemSkinData;

    public ItemSkinData oldSkin;

    public bool useNewSkin;

    public Sprite GetItemSkin(string id)
    {
        Sprite skinSprite = null;

        var skinDict = useNewSkin ? itemSkinData : oldSkin;
        if (skinDict.TryGetValue(id, out var skin))
        {
            skinSprite = skin;
        }

        return skinSprite;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}