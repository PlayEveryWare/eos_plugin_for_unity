using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOSNetworkManager : MonoBehaviour
{
#if COM_UNITY_MODULE_NETCODE
    public GameObject NetworkManagerPrefab;
    private void Start()
    {
        Instantiate(NetworkManagerPrefab);
    }
#endif
}
