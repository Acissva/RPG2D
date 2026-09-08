using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class PlayerLifeStateView : RPG2DController
    {
        private static readonly Color UnavailableTint = new Color(0.45f, 0.45f, 0.45f, 1f);

        private IPlayerModel playerModel;
        private SpriteRenderer[] spriteRenderers;
        private Color[] aliveColors;
        private Animator playerAnimator;
        private float aliveAnimatorSpeed = 1f;

        private void Awake()
        {
            playerModel = this.GetModel<IPlayerModel>();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            aliveColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                aliveColors[i] = spriteRenderers[i] == null ? Color.white : spriteRenderers[i].color;
            }

            playerAnimator = GetComponentInChildren<Animator>(true);

            if (playerAnimator != null)
            {
                aliveAnimatorSpeed = playerAnimator.speed;
            }

            playerModel.LifeState.RegisterWithInitValue(OnLifeStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnLifeStateChanged(PlayerLifeState lifeState)
        {
            bool isAlive = lifeState == PlayerLifeState.Alive;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null)
                {
                    continue;
                }

                spriteRenderers[i].color = isAlive ? aliveColors[i] : MultiplyColor(aliveColors[i], UnavailableTint);
            }

            if (playerAnimator != null)
            {
                playerAnimator.speed = isAlive ? aliveAnimatorSpeed : 0f;
            }
        }

        private static Color MultiplyColor(Color source, Color tint)
        {
            return new Color(source.r * tint.r, source.g * tint.g, source.b * tint.b, source.a * tint.a);
        }
    }
}
