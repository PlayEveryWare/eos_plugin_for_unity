/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/



namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Collections.Generic;
    using UnityEngine;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Ecom;
    using Epic.OnlineServices.Auth;

    /// <summary>
    /// Class <c>EOSStoreManager</c> is a simplified wrapper for EOS [Ecom Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Ecom/index.html).
    /// </summary>

    public class EOSStoreManager : IEOSSubManager, IAuthInterfaceEventListener
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

#if !EOS_DISABLE
        public void OnAuthLogin(LoginCallbackInfo loginCallbackInfo)
        {
            OnLoggedIn();
        }

        public void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo)
        {
            OnLoggedOut();
        }
#endif

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

            EOSManager.Instance.GetEOSEcomInterface().QueryOffers(ref queryOfferOptions, null, OnQueryOffers);
        }

        private void OnQueryOffers(ref QueryOffersCallbackInfo queryOffersCallbackInfo)
        {
            CatalogOffers.Clear();

            Debug.Log("QueryOffers callback. ResultCode=" + queryOffersCallbackInfo.ResultCode);

            if (queryOffersCallbackInfo.ResultCode == Result.Success)
            {
                var getOfferCountOptions = new GetOfferCountOptions();
                getOfferCountOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();

                var offerCount = EOSManager.Instance.GetEOSEcomInterface().GetOfferCount(ref getOfferCountOptions);

                Debug.Log(string.Format("QueryOffers found {0} offers.", offerCount));

                for (int offerIndex = 0; offerIndex < offerCount; ++offerIndex)
                {
                    var copyOfferByIndexOptions = new CopyOfferByIndexOptions();
                    copyOfferByIndexOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
                    copyOfferByIndexOptions.OfferIndex = (uint)offerIndex;

                    var copyOfferByIndexResult = EOSManager.Instance.GetEOSEcomInterface().CopyOfferByIndex(ref copyOfferByIndexOptions, out var catalogOffer);
                    switch (copyOfferByIndexResult)
                    {
                        case Result.Success:
                        case Result.EcomCatalogOfferPriceInvalid:
                        case Result.EcomCatalogOfferStale:
                            Debug.Log($"Offer {offerIndex}: {copyOfferByIndexResult}, {catalogOffer?.Id} {catalogOffer?.TitleText} {catalogOffer?.PriceResult} {GetCurrentPriceAsString(catalogOffer)} {GetOriginalPriceAsString(catalogOffer)}");
                            CatalogOffers.Add(catalogOffer.Value);
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
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("Attempting to display the CheckOutOverlay in the Editor: Won't work because the overlay isn't supported in the Unity Editor");
#endif

            CheckoutEntry checkoutEntry = new CheckoutEntry();
            checkoutEntry.OfferId = CatalogOffers[index].Id;

            CheckoutOptions checkoutOptions = new CheckoutOptions();
            checkoutOptions.LocalUserId = EOSManager.Instance.GetLocalUserId();
            checkoutOptions.Entries = new CheckoutEntry[] { checkoutEntry };

            EOSManager.Instance.GetEOSEcomInterface().Checkout(ref checkoutOptions, null, OnCheckout);
        }

        public void OnCheckout(ref CheckoutCallbackInfo checkoutCallbackInfo)
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
        public string GetCurrentPriceAsString(CatalogOffer? catalogOffer)
        {
            return string.Format("{0}", catalogOffer?.CurrentPrice64);
        }

        //-------------------------------------------------------------------------
        // Wrapper to handle API differences in EOS 1.12 vs 1.11
        public string GetOriginalPriceAsString(CatalogOffer catalogOffer)
        {
            return string.Format("{0}", catalogOffer.OriginalPrice64);
        }

        //-------------------------------------------------------------------------
        // Wrapper to handle API differences in EOS 1.12 vs 1.11
        public string GetOriginalPriceAsString(CatalogOffer? catalogOffer)
        {
            return string.Format("{0}", catalogOffer?.OriginalPrice64);
        }
    }
}