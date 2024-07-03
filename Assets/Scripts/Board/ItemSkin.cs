using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkin : MonoBehaviour
{
    public string id;

    private SpriteRenderer _spriteRenderer;
    


    private void OnEnable()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
        }
        _spriteRenderer.sprite = AddressableManager.instance.ItemData.GetItemSkin(id);

    }
}
