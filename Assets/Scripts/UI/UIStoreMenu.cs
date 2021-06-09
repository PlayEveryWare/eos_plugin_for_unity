//#define EOS_VERSION_1_12

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.Ecom;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class UIStoreMenu : MonoBehaviour
    {
        [Header("Store UI")]
        public GameObject StoreUIParent;

        public Button queryOffersButton;

        public Text catalogueItem0;
        public Button checkOutButton0;

        public Text catalogueItem1;
        public Button checkOutButton1;

        private EOSStoreManager StoreManager;

        public void Start()
        {
            StoreManager = EOSManager.Instance.GetOrCreateManager<EOSStoreManager>();

            HideMenu();
        }

        private void Update()
        {
            if (StoreManager.GetCatalogOffers(out List<CatalogOffer> CatalogOffers))
            {
                // Generate UI for offers
                // Hard-code for demo
                if (CatalogOffers.Count > 0)
                {
                    catalogueItem0.text = string.Format("{0}, ${1}", CatalogOffers[0].TitleText, StoreManager.GetCurrentPriceAsString(CatalogOffers[0]));
                }

                if (CatalogOffers.Count > 1)
                {
                    catalogueItem1.text = string.Format("{0}, ${1}", CatalogOffers[1].TitleText, StoreManager.GetCurrentPriceAsString(CatalogOffers[1]));
                }
            }
        }

        // E-Commerce
        public void OnQueryOffersClick()
        {
            print("OnQueryOffersClick: IsValid=" + EOSManager.Instance.GetLocalUserId().IsValid() + ", accountId" + EOSManager.Instance.GetLocalUserId().ToString());

            StoreManager.QueryOffers();
        }

        public void CheckOutButton(int index)
        {
            StoreManager.CheckOutOverlay(index);
        }

        public void ShowMenu()
        {
            EOSManager.Instance.GetOrCreateManager<EOSStoreManager>().OnLoggedIn();

            queryOffersButton.gameObject.SetActive(true);

            catalogueItem0.gameObject.SetActive(true);
            checkOutButton0.gameObject.SetActive(true);

            catalogueItem1.gameObject.SetActive(true);
            checkOutButton1.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            StoreManager?.OnLoggedOut();

            queryOffersButton.gameObject.SetActive(false);
            catalogueItem0.gameObject.SetActive(false);
            checkOutButton0.gameObject.SetActive(false);

            catalogueItem1.gameObject.SetActive(false);
            checkOutButton1.gameObject.SetActive(false);
        }
    }
}