using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private Transform bar;
    
    private void Start()
    {
        character.OnHealthChanged += HealthSystem_OnHealthChanged;
    }

    private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        bar.localScale = new Vector3(character.GetHealthPercent(), 1);
    }
}
