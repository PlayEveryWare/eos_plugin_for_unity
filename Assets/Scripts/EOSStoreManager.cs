﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Auth;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class EOSStoreManager : IEOSSubManager, IEOSOnAuthLogin
    {
        private List<CatalogOffer> CatalogOffers;
        private bool CatalogOffersDirty;

        public EOSStoreManager()
        {
            CatalogOffers = new List<CatalogOffer>();
            CatalogOffersDirty = false;
        }

        public void OnLoggedIn()
        {
            QueryOffers();
        }

        void IEOSOnAuthLogin.OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            OnLoggedIn();
        }

        public void OnLoggedOut()
        {
            CatalogOffers.Clear();
            CatalogOffersDirty = true;
        }

        public bool GetCatalogOffers(out List<CatalogOffer> CatalogOffers)
        {
            CatalogOffers = this.CatalogOffers;
            return CatalogOffersDirty;
        }

        public void QueryOffers()
        {
            var queryOfferOptions = new QueryOffersOptions();
            queryOfferOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
            queryOfferOptions.OverrideCatalogNamespace = null;

            EOSManager.Instance.GetEOSPlatformInterface().GetEcomInterface().QueryOffers(queryOfferOptions, null, OnQueryOffers);
        }

        private void OnQueryOffers(QueryOffersCallbackInfo queryOffersCallbackInfo)
        {
            CatalogOffers.Clear();

            Debug.Log("QueryOffers callback. ResultCode=" + queryOffersCallbackInfo.ResultCode);

            if (queryOffersCallbackInfo.ResultCode == Result.Success)
            {
                var getOfferCountOptions = new GetOfferCountOptions();
                getOfferCountOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

                var offerCount = EOSManager.Instance.GetEOSPlatformInterface().GetEcomInterface().GetOfferCount(getOfferCountOptions);

                Debug.Log(string.Format("QueryOffers found {0} offers.", offerCount));

                for (int offerIndex = 0; offerIndex < offerCount; ++offerIndex)
                {
                    var copyOfferByIndexOptions = new CopyOfferByIndexOptions();
                    copyOfferByIndexOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
                    copyOfferByIndexOptions.OfferIndex = (uint)offerIndex;

                    var copyOfferByIndexResult = EOSManager.Instance.GetEOSPlatformInterface().GetEcomInterface().CopyOfferByIndex(copyOfferByIndexOptions, out var catalogOffer);
                    switch (copyOfferByIndexResult)
                    {
                        case Result.Success:
                        case Result.EcomCatalogOfferPriceInvalid:
                        case Result.EcomCatalogOfferStale:
                            Debug.Log($"Offer {offerIndex}: {copyOfferByIndexResult}, {catalogOffer.Id} {catalogOffer.TitleText} {catalogOffer.PriceResult} {GetCurrentPriceAsString(catalogOffer)} {GetOriginalPriceAsString(catalogOffer)}");
                            CatalogOffers.Add(catalogOffer);
                            break;

                        default:
                            Debug.Log($"Offer {offerIndex} invalid: {copyOfferByIndexResult}");
                            break;
                    }
                }

                CatalogOffersDirty = true;
            }
            else
            {
                Debug.LogError("Error calling QueryOffers: " + queryOffersCallbackInfo.ResultCode);
            }
        }

        public void CheckOutOverlay(int index)
        {
            CheckoutEntry checkoutEntry = new CheckoutEntry();
            checkoutEntry.OfferId = CatalogOffers[index].Id;

            CheckoutOptions checkoutOptions = new CheckoutOptions();
            checkoutOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
            checkoutOptions.Entries = new CheckoutEntry[] { checkoutEntry };

            EOSManager.Instance.GetEOSPlatformInterface().GetEcomInterface().Checkout(checkoutOptions, null, OnCheckout);
        }

        public void OnCheckout(CheckoutCallbackInfo checkoutCallbackInfo)
        {
            Debug.Log($"Checkout {checkoutCallbackInfo.ResultCode}");
        }

        //-------------------------------------------------------------------------
        // Wrapper to handle API differences in EOS 1.12 vs 1.11
        public string GetCurrentPriceAsString(CatalogOffer catalogOffer)
        {
            return string.Format("{0}", catalogOffer.CurrentPrice64);
        }

        //-------------------------------------------------------------------------
        // Wrapper to handle API differences in EOS 1.12 vs 1.11
        public string GetOriginalPriceAsString(CatalogOffer catalogOffer)
        {
            return string.Format("{0}", catalogOffer.OriginalPrice64);
        }
    }
}