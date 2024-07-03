using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance;
    private ItemData itemData;
      
    public Dictionary<string, AsyncOperationHandle<GameObject>> operationDictionary;
    public List<string> keys;
    public UnityEvent Ready;
    public List<string> readyKey;

    public ItemData ItemData => itemData;
    void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        Addressables.InitializeAsync();
        StartCoroutine(LoadAndAssociateResultWithKey(keys));

        AdressableLoadData((data) =>
        {

            itemData = data;

        });

    }

    private void OnDestroy()
    {
        instance = null;
    }
    public void AdressableLoadObjectByKey(string objectToLoad, UnityAction<GameObject> callback)
    {


        // var gameObj = operationDictionary.TryGetValue(objectToLoad,var out result);
        if (operationDictionary.TryGetValue(objectToLoad, out var result))
        {
            callback?.Invoke(result.Result.gameObject);
        }

    }
    public void AdressableLoadData( UnityAction<ItemData> callback)
    {
    
        Addressables.LoadAssetAsync<ItemData>("data").Completed += (handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded) return;
            var loadedObject = handle.Result;
            callback?.Invoke(loadedObject);
            
        });
    }
    
  

    IEnumerator LoadAndAssociateResultWithKey(IList<string> keys)
    {
        if (operationDictionary == null)
            operationDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations
            = Addressables.LoadResourceLocationsAsync(keys,
                Addressables.MergeMode.Union, typeof(GameObject));

        yield return locations;

        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result)
        {
            AsyncOperationHandle<GameObject> handle =
                Addressables.LoadAssetAsync<GameObject>(location);
            handle.Completed += obj =>
            {
                // Debug.Log(location.PrimaryKey);
                operationDictionary.Add(obj.Result.name, obj);
                readyKey.Add(obj.Result.name);
            };
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        Ready.Invoke();
    }
}
