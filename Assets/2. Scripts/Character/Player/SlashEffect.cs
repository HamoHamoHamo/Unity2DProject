using System.Collections;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public void PlaySlashEffect()
    {
        gameObject.SetActive(true);
    }
    public void StopSlashEffect()
    {
        gameObject.SetActive(false);
    }
}