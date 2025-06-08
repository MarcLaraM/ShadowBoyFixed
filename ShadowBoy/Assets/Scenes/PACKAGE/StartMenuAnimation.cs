using UnityEngine;

public class StartMenu : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("StartMenuAnim"); // Nombre de la animaci�n para el men�
    }

    // Opcional: Cambiar animaci�n al interactuar con botones
    public void PlayAttackAnimation()
    {
        animator.Play("attack");
    }
    public void PlayDeathAnimation()
    {
        animator.Play("Death");
    }
}
