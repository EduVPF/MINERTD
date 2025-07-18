using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Animation;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.U2D.IK;

namespace UnityEditor.U2D.IK
{
    /// <summary>
    /// Custom Inspector for IKManager2D.
    /// </summary>
    [CustomEditor(typeof(IKManager2D))]
    [CanEditMultipleObjects]
    class IKManager2DEditor : Editor
    {
        class Contents
        {
            public static readonly GUIContent findAllSolversLabel = new GUIContent("Find Solvers", "Find all applicable solvers handled by this manager");
            public static readonly GUIContent weightLabel = new GUIContent("Weight", "Blend between Forward and Inverse Kinematics");
            public static readonly GUIContent alwaysUpdate = new GUIContent("Always Update", "Solvers are being updated even if Sprites driven by bones in the chain are not visible.");
            public static readonly string listHeaderLabel = "IK Solvers";
            public static readonly string createSolverString = "Create Solver";
            public static readonly string restoreDefaultPoseString = "Restore Default Pose";
            public static readonly GUIContent gizmoColorTooltip = new GUIContent("", "Customizes the IK Chain's Gizmo color");
            public static int showGizmoPropertyWidth = 20;
            public static int solverPropertyWidth = 100;
            public static int solverColorPropertyWidth = 40;
            public static readonly GUIContent gizmoVisibilityToolTip = new GUIContent("", L10n.Tr("Show/Hide Gizmo"));
            public readonly GUIStyle visibilityToggleStyle;

            public Contents()
            {
                visibilityToggleStyle = new GUIStyle();
                visibilityToggleStyle.fixedWidth = EditorGUIUtility.singleLineHeight;
                visibilityToggleStyle.onNormal.background = EditorIconUtility.LoadIconResource("Visibility_Tool", EditorIconUtility.LightIconPath, EditorIconUtility.DarkIconPath);
                visibilityToggleStyle.normal.background = EditorIconUtility.LoadIconResource("Visibility_Hidded", EditorIconUtility.LightIconPath, EditorIconUtility.DarkIconPath);
            }
        }

        static Contents s_Contents;
        ReorderableList m_ReorderableList;
        Solver2D m_SelectedSolver;
        Editor m_SelectedSolverEditor;

        SerializedProperty m_SolversProperty;
        SerializedProperty m_SolverEditorDataProperty;
        SerializedProperty m_WeightProperty;
        SerializedProperty m_AlwaysUpdate;
        List<Type> m_SolverTypes;
        IKManager2D m_Manager;

        void OnEnable()
        {
            m_Manager = target as IKManager2D;
            m_SolverTypes = GetDerivedTypes<Solver2D>();
            m_SolversProperty = serializedObject.FindProperty("m_Solvers");
            m_SolverEditorDataProperty = serializedObject.FindProperty("m_SolverEditorData");
            m_WeightProperty = serializedObject.FindProperty("m_Weight");
            m_AlwaysUpdate = serializedObject.FindProperty("m_AlwaysUpdate");
            SetupReorderableList();
        }

        void SetupReorderableList()
        {
            m_ReorderableList = new ReorderableList(serializedObject, m_SolversProperty, true, true, true, true);
            m_ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, Contents.listHeaderLabel);
            };
            m_ReorderableList.elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight + 6;
            };
            m_ReorderableList.drawElementCallback = (Rect rect, int index, bool isactive, bool isfocused) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty element = m_SolversProperty.GetArrayElementAtIndex(index);
                SerializedProperty elementData = m_SolverEditorDataProperty.GetArrayElementAtIndex(index);
                float width = rect.width;
                rect.width = width > Contents.showGizmoPropertyWidth ? Contents.showGizmoPropertyWidth : width;
                SerializedProperty showGizmoProperty = elementData.FindPropertyRelative("showGizmo");
                showGizmoProperty.boolValue = GUI.Toggle(rect, showGizmoProperty.boolValue, Contents.gizmoVisibilityToolTip, s_Contents.visibilityToggleStyle);
                rect.x += rect.width;
                width -= rect.width;
                rect.width = width > Contents.solverPropertyWidth ? width - Contents.solverColorPropertyWidth : Contents.solverPropertyWidth;
                EditorGUI.PropertyField(rect, element, GUIContent.none);
                rect.x += rect.width;
                width -= 100;
                rect.width = width > Contents.solverColorPropertyWidth ? Contents.solverColorPropertyWidth : width;
                EditorGUI.PropertyField(rect, elementData.FindPropertyRelative("color"), Contents.gizmoColorTooltip);
            };
            m_ReorderableList.onAddCallback = (ReorderableList list) =>
            {
                GenericMenu menu = new GenericMenu();

                foreach (Type type in m_SolverTypes)
                {
                    Solver2DMenuAttribute attribute = Attribute.GetCustomAttribute(type, typeof(Solver2DMenuAttribute)) as Solver2DMenuAttribute;

                    if (attribute != null)
                        menu.AddItem(new GUIContent(attribute.menuPath), false, OnSelectMenu, type);
                    else
                        menu.AddItem(new GUIContent(type.Name), false, OnSelectMenu, type);
                }

                menu.ShowAsContext();
            };
            m_ReorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                Solver2D solver = m_Manager.solvers[list.index];
                if (solver)
                {
                    Undo.RegisterCompleteObjectUndo(m_Manager, Undo.GetCurrentGroupName());

                    m_Manager.RemoveSolver(solver);

                    GameObject solverGO = solver.gameObject;

                    if (solverGO.transform.childCount == 0)
                        Undo.DestroyObjectImmediate(solverGO);
                    else
                        Undo.DestroyObjectImmediate(solver);

                    EditorUtility.SetDirty(m_Manager);
                }
                else
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }

        void OnSelectMenu(object param)
        {
            Type solverType = param as Type;

            GameObject solverGO = new GameObject(GameObjectUtility.GetUniqueNameForSibling(m_Manager.transform, "New " + solverType.Name));
            solverGO.transform.SetParent(m_Manager.transform);
            solverGO.transform.localPosition = Vector3.zero;
            solverGO.transform.rotation = Quaternion.identity;
            solverGO.transform.localScale = Vector3.one;

            Solver2D solver = solverGO.AddComponent(solverType) as Solver2D;

            Undo.RegisterCreatedObjectUndo(solverGO, Contents.createSolverString);
            Undo.RegisterCompleteObjectUndo(m_Manager, Contents.createSolverString);
            m_Manager.AddSolver(solver);
            EditorUtility.SetDirty(m_Manager);

            Selection.activeGameObject = solverGO;
        }

        /// <summary>
        /// Custom Inspector OnInspectorGUI override.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (s_Contents == null)
                s_Contents = new Contents();

            serializedObject.Update();

            EditorGUILayout.Space();

            m_ReorderableList.DoLayoutList();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_WeightProperty, Contents.weightLabel);

            EditorGUILayout.PropertyField(m_AlwaysUpdate, Contents.alwaysUpdate);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            DoRestoreDefaultPoseButton();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        void DoRestoreDefaultPoseButton()
        {
            if (GUILayout.Button(Contents.restoreDefaultPoseString, GUILayout.MaxWidth(150f)))
            {
                foreach (UnityEngine.Object l_target in targets)
                {
                    IKManager2D manager = l_target as IKManager2D;

                    IKEditorManager.instance.Record(manager, Contents.restoreDefaultPoseString);

                    foreach (Solver2D solver in manager.solvers)
                    {
                        for (int i = 0; i < solver.chainCount; ++i)
                        {
                            IKChain2D chain = solver.GetChain(i);
                            chain.RestoreDefaultPose(solver.constrainRotation);

                            if (chain.target)
                            {
                                chain.target.position = chain.effector.position;
                                chain.target.rotation = chain.effector.rotation;
                            }
                        }
                    }

                    IKEditorManager.instance.UpdateManagerImmediate(manager, true);
                }
            }
        }

        static List<Type> GetDerivedTypes<T>() where T : class
        {
            TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom<T>();
            return typeCollection.ToList();
        }
    }
}
