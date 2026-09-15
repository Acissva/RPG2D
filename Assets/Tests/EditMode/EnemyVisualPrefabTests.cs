using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RPG2D.Tests.EditMode
{
    public class EnemyVisualPrefabTests
    {
        private const string EnemyPrefabPath = "Assets/Resources/Prefab/Character/Enemy.prefab";
        private const string SlimeControllerPath = "Assets/Art/Animations/Characters/Enemy/Slime/Slime.controller";

        [Test]
        public void EnemyPrefab_WhenLoaded_UsesSlimeAnimationContract()
        {
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            RuntimeAnimatorController slimeRuntimeController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SlimeControllerPath);

            Assert.That(enemyPrefab, Is.Not.Null);
            Assert.That(slimeRuntimeController, Is.Not.Null);

            Animator animator = enemyPrefab.GetComponent<Animator>();
            EnemyAnimationView animationView = enemyPrefab.GetComponent<EnemyAnimationView>();
            EnemyMotor2D motor = enemyPrefab.GetComponent<EnemyMotor2D>();
            SpriteRenderer bodySprite = enemyPrefab.transform.Find("Visual/BodySprite")?.GetComponent<SpriteRenderer>();

            Assert.That(animator, Is.Not.Null);
            Assert.That(animationView, Is.Not.Null);
            Assert.That(motor, Is.Not.Null);
            Assert.That(bodySprite, Is.Not.Null);
            Assert.That(bodySprite.sprite, Is.Not.Null);
            Assert.That(bodySprite.sprite.texture.name, Is.EqualTo("XiaoSuZaoShui_Slime_Idle"));
            Assert.That(animator.runtimeAnimatorController, Is.EqualTo(slimeRuntimeController));

            AnimatorController slimeController = slimeRuntimeController as AnimatorController;
            Assert.That(slimeController, Is.Not.Null);
            Assert.That(slimeController.layers[0].stateMachine.defaultState.name, Is.EqualTo("Normal"));

            string[] stateNames = slimeController.layers[0].stateMachine.states.Select(childState => childState.state.name).ToArray();
            string[] requiredStateNames = { "Normal", "Walk", "GetHurt", "Die" };

            CollectionAssert.IsSubsetOf(requiredStateNames, stateNames);

            AnimatorControllerParameter isMovingParameter = slimeController.parameters.SingleOrDefault(parameter => parameter.name == "IsMoving");

            Assert.That(isMovingParameter, Is.Not.Null);
            Assert.That(isMovingParameter.type, Is.EqualTo(AnimatorControllerParameterType.Bool));
        }
    }
}
