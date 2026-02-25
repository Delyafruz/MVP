#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VCableSystem.Editor
{
    public static class VCableCreator
    {
        [MenuItem("GameObject/VCableSystem/Create VCable", false, 10)]
        private static void CreateVCable(MenuCommand cmd)
        {
            // Корневой объект
            GameObject root = new GameObject("VCable");
            root.AddComponent<LineRenderer>();
            root.AddComponent<CableRenderer>();
            VCable cable = root.AddComponent<VCable>();

            // StartEndpoint
            GameObject startGO = CreateEndpoint("StartEndpoint", root.transform,
                Vector3.zero, EndpointSide.Start);

            // EndEndpoint — смещаем вперёд на 1.5м чтобы кабель сразу был виден
            GameObject endGO = CreateEndpoint("EndEndpoint", root.transform,
                new Vector3(0f, -0.3f, 1.5f), EndpointSide.End);

            // Назначаем endpoints через SerializedObject
            SerializedObject so = new SerializedObject(cable);
            so.FindProperty("startEndpoint").objectReferenceValue = startGO.GetComponent<CableEndpoint>();
            so.FindProperty("endEndpoint").objectReferenceValue   = endGO.GetComponent<CableEndpoint>();
            so.ApplyModifiedProperties();

            // Регистрируем и выделяем
            Undo.RegisterCreatedObjectUndo(root, "Create VCable");
            GameObjectUtility.SetParentAndAlign(root, cmd.context as GameObject);
            Selection.activeGameObject = root;

            EditorApplication.delayCall += () =>
            {
                if (root != null)
                    EditorUtility.SetDirty(root);
            };

            Debug.Log(
                "[VCableSystem] VCable создан!\n" +
                "Настрой Connector на каждом Endpoint:\n" +
                "  • StartEndpoint → ConnectionType = Male, ConnectorType = LAN (или нужный)\n" +
                "  • EndEndpoint   → ConnectionType = Female, ConnectorType = тот же\n" +
                "  • Добавь SphereCollider (radius 0.05) на каждый Endpoint\n" +
                "  • Выставь слой Collider'а в PlayerInteractions.selectLayer\n" +
                "  • На порту устройства тоже добавь Connector (противоположный тип)"
            );
        }

        [MenuItem("GameObject/VCableSystem/Create CableManager", false, 11)]
        private static void CreateCableManager(MenuCommand cmd)
        {
            if (Object.FindObjectOfType<CableManager>() != null)
            {
                Debug.LogWarning("[VCableSystem] CableManager уже есть в сцене.");
                return;
            }

            GameObject go = new GameObject("CableManager");
            go.AddComponent<CableManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create CableManager");
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            Selection.activeGameObject = go;
        }

        private static GameObject CreateEndpoint(string name, Transform parent,
            Vector3 localPos, EndpointSide side)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            CableEndpoint ep = go.AddComponent<CableEndpoint>();
            SerializedObject epSo = new SerializedObject(ep);
            epSo.FindProperty("side").enumValueIndex = (int)side;
            epSo.ApplyModifiedProperties();

            // Rigidbody (нужен для Liftable). По умолчанию делаем динамическим,
            // чтобы при старте сцены endpoint падал под действием гравитации.
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity  = true;

            // Connector — существующая система соединений (Male/Female, CableType)
            // Настрой ConnectionType и ConnectorType в инспекторе после создания
            go.AddComponent<HPhysic.Connector>();

            // CableEndpointInteractable — мост с системой взаимодействия
            go.AddComponent<CableEndpointInteractable>();

            return go;
        }
    }
}
#endif
