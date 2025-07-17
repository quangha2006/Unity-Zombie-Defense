using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;

    private PlayerController player;
    private Camera cam;
    private RectTransform rectTransform;
    private bool shouldShow = true;
    private float heightOffset = 2f;

    private void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        if (player == null)
            healthBar.gameObject.SetActive(false);
    }
    void LateUpdate()
    {
        if (player == null)
        {
            healthBar.gameObject.SetActive(false);
            return;
        }
        var worldPos = player.transform.position;
        worldPos.y += heightOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        rectTransform.position = screenPos;

        healthBar.gameObject.SetActive(screenPos.z > 0 && shouldShow);
    }

    public void AssignPlayer(PlayerController player)
    {
        this.player = player;
        this.player.onHealthChanged += OnHealthChanged;
        this.player.onDeath += OnPlayerDeath;
        OnHealthChanged(player.health, player.MaxHealth);
    }
    private void OnHealthChanged(int current, int max)
    {
        Debug.Log($"Player OnHealthChanged: {current}/{max}");
        healthBar.value = current / (float)max;
    }

    private void OnPlayerDeath()
    {
        healthBar.gameObject.SetActive(false);
        shouldShow = false;
    }
}
