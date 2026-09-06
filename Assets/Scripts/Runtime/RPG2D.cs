using QFramework;
using RPG2D.Utility;
using UnityEngine;

namespace RPG2D
{
    public class RPG2D : Architecture<RPG2D>
    {
        protected override void Init()
        {
            //注册Utility
            this.RegisterUtility<IObjectPoolUtility>(new ObjectPoolUtility());

            //注册Model
            this.RegisterModel<IPlayerModel>(new PlayerModel());
            this.RegisterModel<IUIModel>(new UIModel());
            this.RegisterModel<IDialogueModel>(new DialogueModel());
            this.RegisterModel<IQuestModel>(new QuestModel());
            this.RegisterModel<IInventoryModel>(new InventoryModel());
            this.RegisterModel<IDungeonModel>(new DungeonModel());

            //注册System
            this.RegisterSystem<IRuntimeUpdateSystem>(new RuntimeUpdateSystem());   //特殊：由于后面有几个系统会使用Update，所以放最前面
            this.RegisterSystem<IRPG2DInputSystem>(new RPG2DInputSystem());
            this.RegisterSystem<IAssetSystem>(new AssetSystem());
            this.RegisterSystem<IConfigSystem>(new ConfigSystem());
            this.RegisterSystem<IInventorySystem>(new InventorySystem());
            this.RegisterSystem<IRewardSystem>(new RewardSystem());
            this.RegisterSystem<IQuestSystem>(new QuestSystem());
            this.RegisterSystem<ITimeSystem>(new TimeSystem());
            this.RegisterSystem<IEnemyRegistrySystem>(new EnemyRegistrySystem());
            this.RegisterSystem<IDungeonSystem>(new DungeonSystem());
            this.RegisterSystem<IUISystem>(new UISystem());
            this.RegisterSystem<IDialogueSystem>(new DialogueSystem());
            this.RegisterSystem<ISceneSystem>(new SceneSystem());

        }


        //框架生成物的父节点
        private GameObject frameworkGenerateItemsParentObj;

        public static Transform FrameworkGenerateItemsParent =>
            ((RPG2D)Interface).GenerateItemsParentObj;

        public Transform GenerateItemsParentObj
        {
            get
            {
                if (frameworkGenerateItemsParentObj == null)
                {
                    frameworkGenerateItemsParentObj = GameObject.Find("FrameworkGenerateItems");

                    if (frameworkGenerateItemsParentObj == null)
                    {
                        frameworkGenerateItemsParentObj = new GameObject("FrameworkGenerateItems");
                        Object.DontDestroyOnLoad(frameworkGenerateItemsParentObj);
                    }
                }

                return frameworkGenerateItemsParentObj.transform;
            }
        }

        //掉落物的父节点
        private GameObject dropLootsParentObj;

        public static Transform DropLootsParent =>
            ((RPG2D)Interface).DropLootsParentObj;

        public Transform DropLootsParentObj
        {
            get
            {
                if (dropLootsParentObj == null)
                {
                    dropLootsParentObj = GameObject.Find("Loots");

                    if (dropLootsParentObj == null)
                    {
                        dropLootsParentObj = new GameObject("Loots");
                    }
                }

                return dropLootsParentObj.transform;
            }
        }

    }
}
