using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Manages AdMob interstitial ads.
/// Shows an interstitial ad after the player fails a specified number of times.
/// </summary>
public class AdManager : MonoBehaviour
{
    [Header("Ad Settings")]
    [SerializeField] private int failuresBeforeAd = 10;
    
    [Header("Test Mode")]
    [SerializeField] private bool useTestAds = true;
    
    // Test Ad Unit IDs
    private const string TEST_AD_ID_ANDROID = "ca-app-pub-3940256099942544/1033173712";
    private const string TEST_AD_ID_IOS = "ca-app-pub-3940256099942544/4411468910";
    
    // Production Ad Unit IDs (replace with your real IDs)
    private const string PROD_AD_ID_ANDROID = "ca-app-pub-9764584713102923/9526393420";
    private const string PROD_AD_ID_IOS = "ca-app-pub-9764584713102923/1647903405";
    
    public static AdManager Instance { get; private set; }
    
    private InterstitialAd interstitialAd;
    private int failureCount = 0;
    private bool isInitialized = false;
    
    public event Action OnAdShown;
    public event Action OnAdClosed;
    public event Action OnAdFailedToShow;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeAds();
    }
    
    private void InitializeAds()
    {
        Debug.Log("AdManager: Initializing Google Mobile Ads SDK...");
        
        MobileAds.Initialize(initStatus =>
        {
            isInitialized = true;
            Debug.Log("AdManager: SDK initialized successfully!");
            LoadInterstitialAd();
        });
    }
    
    private string GetAdUnitId()
    {
        string adId = "";
        
#if UNITY_ANDROID
        adId = useTestAds ? TEST_AD_ID_ANDROID : PROD_AD_ID_ANDROID;
#elif UNITY_IOS
        adId = useTestAds ? TEST_AD_ID_IOS : PROD_AD_ID_IOS;
#else
        adId = "unused";
#endif
        
        return adId;
    }
    
    public void LoadInterstitialAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager: SDK not initialized yet!");
            return;
        }
        
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        
        Debug.Log("AdManager: Loading interstitial ad...");
        
        var adRequest = new AdRequest();
        string adUnitId = GetAdUnitId();
        
        InterstitialAd.Load(adUnitId, adRequest, OnInterstitialAdLoaded);
    }
    
    private void OnInterstitialAdLoaded(InterstitialAd ad, LoadAdError error)
    {
        if (error != null || ad == null)
        {
            Debug.LogError("AdManager: Interstitial ad failed to load - " + (error != null ? error.GetMessage() : "null ad"));
            return;
        }
        
        Debug.Log("AdManager: Interstitial ad loaded successfully!");
        interstitialAd = ad;
        RegisterEventHandlers(interstitialAd);
    }
    
    private void RegisterEventHandlers(InterstitialAd ad)
    {
        ad.OnAdPaid += HandleAdPaid;
        ad.OnAdImpressionRecorded += HandleAdImpressionRecorded;
        ad.OnAdClicked += HandleAdClicked;
        ad.OnAdFullScreenContentOpened += HandleAdOpened;
        ad.OnAdFullScreenContentClosed += HandleAdClosed;
        ad.OnAdFullScreenContentFailed += HandleAdFailedToShow;
    }
    
    private void HandleAdPaid(AdValue adValue)
    {
        Debug.Log("AdManager: Ad paid - " + adValue.Value + " " + adValue.CurrencyCode);
    }
    
    private void HandleAdImpressionRecorded()
    {
        Debug.Log("AdManager: Ad impression recorded");
    }
    
    private void HandleAdClicked()
    {
        Debug.Log("AdManager: Ad clicked");
    }
    
    private void HandleAdOpened()
    {
        Debug.Log("AdManager: Ad opened");
        OnAdShown?.Invoke();
    }
    
    private void HandleAdClosed()
    {
        Debug.Log("AdManager: Ad closed");
        OnAdClosed?.Invoke();
        LoadInterstitialAd();
    }
    
    private void HandleAdFailedToShow(AdError adError)
    {
        Debug.LogError("AdManager: Ad failed to show - " + adError.GetMessage());
        OnAdFailedToShow?.Invoke();
        LoadInterstitialAd();
    }
    
    public bool ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("AdManager: Showing interstitial ad...");
            interstitialAd.Show();
            return true;
        }
        else
        {
            Debug.LogWarning("AdManager: Interstitial ad not ready");
            LoadInterstitialAd();
            return false;
        }
    }
    
    public void OnPlayerFailed()
    {
        failureCount++;
        Debug.Log("AdManager: Player failed. Count: " + failureCount + "/" + failuresBeforeAd);
        
        if (failureCount >= failuresBeforeAd)
        {
            Debug.Log("AdManager: Threshold reached! Showing ad...");
            
            if (ShowInterstitialAd())
            {
                failureCount = 0;
            }
            else
            {
                Debug.Log("AdManager: Ad not ready, will try after next failure");
            }
        }
    }
    
    public void ResetFailureCount()
    {
        failureCount = 0;
        Debug.Log("AdManager: Failure count reset");
    }
    
    public int GetFailureCount()
    {
        return failureCount;
    }
    
    public int GetFailuresUntilNextAd()
    {
        return Mathf.Max(0, failuresBeforeAd - failureCount);
    }
    
    public bool IsInterstitialAdReady()
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }
    
    private void OnDestroy()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
    }
}
