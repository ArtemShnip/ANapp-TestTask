using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class DemoStorePage : MonoBehaviour, IStoreListener
{
    [SerializeField] private GameObject LoadingOverlay;
    [SerializeField] private bool UseFakeStore = true;
    
    private Action OnPurchaseCompleted;
    private IStoreController StoreController;
    private IExtensionProvider ExtensionProvider;
    private Product[] _products;

    private async void Awake()
    {
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName("test");

        await UnityServices.InitializeAsync(options);
        ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
        operation.completed += HandleIAPCatalogLoaded;
    }

    private void HandleIAPCatalogLoaded(AsyncOperation Operation)
    {
        ResourceRequest request = Operation as ResourceRequest;

        Debug.Log($"Loaded Asset: {request.asset}");
        ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
        Debug.Log($"Loaded catalog with {catalog.allProducts.Count} items");

        if (UseFakeStore) // Use bool in editor to control fake store behavior.
        {
            StandardPurchasingModule.Instance().useFakeStoreUIMode =
                FakeStoreUIMode.StandardUser; // Comment out this line if you are building the game for publishing.
            StandardPurchasingModule.Instance().useFakeStoreAlways =
                true; // Comment out this line if you are building the game for publishing.
        }

#if UNITY_ANDROID
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.GooglePlay)
        );
#elif UNITY_IOS
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.AppleAppStore)
        );
#else
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.NotSpecified)
        );
#endif
        foreach (ProductCatalogItem item in catalog.allProducts)
        {
            builder.AddProduct(item.id, item.type);
        }

        Debug.Log($"Initializing Unity IAP with {builder.products.Count} products");
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        StoreController = controller;
        ExtensionProvider = extensions;
        _products = StoreController.products.all;
        Debug.Log(
            $"Successfully Initialized Unity IAP. Store Controller has {StoreController.products.all.Length} products");
    }

    public void HandlePurchase(int productIndex)
    {
        LoadingOverlay.SetActive(true);
        // this.OnPurchaseCompleted = OnPurchaseCompleted;
        StoreController.InitiatePurchase(_products[productIndex]);
    }

    public void RestorePurchase() // Use a button to restore purchase only in iOS device.
    {
#if UNITY_IOS
        ExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(OnRestore);
#endif
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Error initializing IAP because of {error}." +
                       $"\r\nShow a message to the player depending on the error.");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Failed to purchase {product.definition.id} because {failureReason}");
        OnPurchaseCompleted?.Invoke();
        OnPurchaseCompleted = null;
        LoadingOverlay.SetActive(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        Debug.Log($"Successfully purchased {purchaseEvent.purchasedProduct.definition.id}");
        OnPurchaseCompleted?.Invoke();
        OnPurchaseCompleted = null;
        LoadingOverlay.SetActive(false);

        switch (purchaseEvent.purchasedProduct.definition.id)
        {
            case "epic_chest":
                DailyBonus.Balance += 500;
                break;
            case "lucky_chest":
                DailyBonus.Balance += 1200;
                break;
        }

        DailyBonus.OnBalanceChange.Invoke();

        return PurchaseProcessingResult.Complete;
    }

    public void BuyShopItemWithTickets(ShopItemTickets item)
    {
        if (DailyBonus.Balance >= item.price && item.available)
        {
            DailyBonus.Balance -= item.price;
            DailyBonus.OnBalanceChange.Invoke();
            item.purchased.SetActive(true);
        }
    }
}