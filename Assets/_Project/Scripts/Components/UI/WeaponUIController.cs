using UnityEngine;
using UnityEngine.UI;
using Weapon;

public class WeaponUIController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Button buttonLeft;
    [SerializeField] private Button buttonRight;
    [SerializeField] private Button buttonGrenade;
    
    private PlayerController currentPlayer;

    [SerializeField] private string weaponLeftName;
    [SerializeField] private string weaponRightName;
    [SerializeField] private string weaponGrenadeName;
    private bool isWeaponNameSetted = false;
    private void Awake()
    {
        buttonLeft.gameObject.SetActive(false);
        buttonRight.gameObject.SetActive(false);
        buttonGrenade.gameObject.SetActive(false);
    }
    void Start()
    {
        levelManager.OnPlayerSpawned += OnPlayerSpawned;
        levelManager.OnMatchStateChanged += OnMatchStateChanged;
        buttonLeft.onClick.AddListener(OnButtonLeftPressed);
        buttonRight.onClick.AddListener(OnButtonRightPressed);
        buttonGrenade.onClick.AddListener(OnButtonGrenadePressed);
    }
    
    private void Update()
    {
        if (currentPlayer != null && !isWeaponNameSetted)
        {
            var weaponList = currentPlayer.GetWeaponListName();
            if (weaponList == null)
                return;

            if (weaponList.Count > 0)
            {
                weaponLeftName = weaponList[0];
                buttonLeft.gameObject.SetActive(true);
            }
            if (weaponList.Count > 1)
            {
                weaponRightName = weaponList[1];
                buttonRight.gameObject.SetActive(true);
            }
            if (weaponList.Count > 2)
            {
                weaponGrenadeName = weaponList[2];
                buttonGrenade.gameObject.SetActive(true);
            }
            var currentWeaponName = currentPlayer.currentWeaponName;
            if (!string.IsNullOrEmpty(currentWeaponName))
            {
                OnWeaponChanged(currentWeaponName);
            }
            isWeaponNameSetted = true;
        }
    }

    private void OnButtonLeftPressed() 
    {
        currentPlayer.LoadWeapon(weaponLeftName);
    }
    private void OnButtonRightPressed()
    {
        currentPlayer.LoadWeapon(weaponRightName);
    }
    private void OnButtonGrenadePressed()
    {
        currentPlayer.LoadWeapon(weaponGrenadeName);
    }
    private void OnPlayerSpawned(PlayerController player)
    {
        isWeaponNameSetted = false;
        currentPlayer = player;
        currentPlayer.onWeaponChanged += OnWeaponChanged;
    }

    private void OnWeaponChanged(WeaponBase weapon)
    {
        var weaponName = weapon.WeaponName;
        OnWeaponChanged(weaponName);
    }
    private void OnWeaponChanged(string weaponName)
    {
        if (!string.IsNullOrEmpty(weaponLeftName))
        {
            buttonLeft.interactable = (weaponName != weaponLeftName);
        }
        if (!string.IsNullOrEmpty(weaponRightName))
        {
            buttonRight.interactable = (weaponName != weaponRightName);
        }
        if (!string.IsNullOrEmpty(weaponGrenadeName))
        {
            buttonGrenade.interactable = (weaponName != weaponGrenadeName);
        }
    }
    private void OnMatchStateChanged(LevelManager.MatchState matchState)
    {
        if (matchState > LevelManager.MatchState.Playing)
        {
            buttonLeft.gameObject.SetActive(false);
            buttonRight.gameObject.SetActive(false);
            buttonGrenade.gameObject.SetActive(false);
        }
    }
}
