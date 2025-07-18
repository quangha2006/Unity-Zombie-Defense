using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Button buttonLeft;
    [SerializeField] private Button buttonRight;
    
    private PlayerController currentPlayer;

    [SerializeField] private string weaponLeftName;
    [SerializeField] private string weaponRightName;
    private bool isWeaponNameSetted = false;
    private void Awake()
    {
        buttonLeft.gameObject.SetActive(false);
        buttonRight.gameObject.SetActive(false);
    }
    void Start()
    {
        levelManager.OnPlayerSpawned += OnPlayerSpawned;
        buttonLeft.onClick.AddListener(OnButtonLeftPressed);
        buttonRight.onClick.AddListener(OnButtonRightPressed);
    }
    
    private void Update()
    {
        if (currentPlayer != null && !isWeaponNameSetted)
        {
            var weaponList = currentPlayer.GetWeaponListName();
            if (weaponList == null)
                return;
            Debug.Log("weaponList: " + weaponList.Count);
            if (weaponList.Count > 0)
            {
                Debug.Log("weaponList[0] " + weaponList[0]);
                weaponLeftName = weaponList[0];
                buttonLeft.gameObject.SetActive(true);
            }
            if (weaponList.Count > 1)
            {
                Debug.Log("weaponList[1] " + weaponList[1]);
                weaponRightName = weaponList[1];
                buttonRight.gameObject.SetActive(true);
            }
            var currentWeaponName = currentPlayer.currentWeaponName;
            if (!string.IsNullOrEmpty(currentWeaponName))
            {
                OnWeaponSwitch(currentWeaponName);
            }
            isWeaponNameSetted = true;
        }
    }

    private void OnButtonLeftPressed() 
    {
        Debug.Log("OnButtonLeftPressed " + weaponLeftName);
        currentPlayer.LoadWeapon(weaponLeftName);
    }
    private void OnButtonRightPressed()
    {
        Debug.Log("OnButtonRightPressed " + weaponRightName);
        currentPlayer.LoadWeapon(weaponRightName);
    }
    private void OnPlayerSpawned(PlayerController player)
    {
        isWeaponNameSetted = false;
        currentPlayer = player;
        currentPlayer.OnWeaponChanged += OnWeaponSwitch;
    }

    private void OnWeaponSwitch(string weaponName)
    {
        if (!string.IsNullOrEmpty(weaponLeftName))
        {
            buttonLeft.interactable = (weaponName != weaponLeftName);
        }
        if (!string.IsNullOrEmpty(weaponRightName))
        {
            buttonRight.interactable = (weaponName != weaponRightName);
        }
    }
}
