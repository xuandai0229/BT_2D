using UnityEngine;

public class GameManager : MonoBehaviour
{

  [SerializeField] FlippyBardController _player;

    private void Start()
    {
        _player.Die += OnPlayerDie;
    }
    void OnPlayerDie()
    {
        Time.timeScale = 0f;
    }
}
