using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // 引入 System.IO

namespace MTool
{
    public class ScriptableObjectCreator : EditorWindow
    {
        private List<Type> _soTypes = new List<Type>();
        private Vector2 _scrollPosition;
        private string _searchPath;

        [MenuItem("Tools/ScriptableObject Creator")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableObjectCreator>("SO CREATOR");
        }

        private void OnEnable()
        {
            _searchPath = EditorPrefs.GetString("ScriptableObjectCreator_SearchPath", "Assets/Scripts");
            // FindAllScriptableObjectTypes();
        }

        void OnDisable()
        {
            // 保存当前的搜索路径到 EditorPrefs
            EditorPrefs.SetString("ScriptableObjectCreator_SearchPath", _searchPath);
        }

        private void OnGUI()
        {
            GUILayout.Label("Create ScriptableObject Instances", EditorStyles.boldLabel);

            // 路径选择区域
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Path:", GUILayout.Width(40)); // 设置标签的宽度为40像素
            _searchPath = EditorGUILayout.TextField(_searchPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Script Folder", _searchPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 将绝对路径转换为相对于项目的路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        _searchPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogError("Selected path is not within the project's Assets folder.");
                        // 可以选择在这里给 _searchPath 设置一个默认值，或者禁用“确定”按钮
                    }
                }
            }
            if (GUILayout.Button("OK", GUILayout.Width(60)))
            {
                FindAllScriptableObjectTypes(); // 点击“确定”按钮时，重新查找 SO 类型
            }

            // 结束一个水平布局组
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // 遍历 _soTypes 集合中的每一个 Type 对象
            foreach (Type soType in _soTypes)
            {
                // 使用 GUILayout.Button 创建一个按钮，按钮的文本为 "Create " 加上当前 Type 的名称
                if (GUILayout.Button(soType.Name))
                {
                    // 如果按钮被点击，则调用 CreateScriptableObjectAsset 方法，并传入当前的 Type 对象
                    CreateScriptableObjectAsset(soType);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void FindAllScriptableObjectTypes()
        {
            _soTypes.Clear();

            // 1. 获取 Scripts 文件夹及其子文件夹中的所有 .cs 文件
            string[] scriptFiles = Directory.GetFiles(_searchPath, "*.cs", SearchOption.AllDirectories);

            // 2. 遍历所有脚本文件
            foreach (string scriptFile in scriptFiles)
            {
                // 3. 从文件路径获取脚本的类名 (不含 .cs 扩展名)
                string className = Path.GetFileNameWithoutExtension(scriptFile);

                // 4.  尝试从当前程序集中加载该类
                Type type = Type.GetType(className); // Type.GetType 可能会因为命名空间问题失败，见下文改进
                if (type == null)
                {
                    //如果直接Type.GetType失败了, 就遍历所有程序集
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .FirstOrDefault(t => t.Name == className);

                }

                // 5. 检查是否是 ScriptableObject 的子类且非抽象类
                if (type != null && type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract)
                {
                    _soTypes.Add(type);
                }
            }
        }
        private void CreateScriptableObjectAsset(Type soType)
        {
            // 创建 ScriptableObject 实例
            ScriptableObject instance = CreateInstance(soType);

            // 获取保存路径
            string path = EditorUtility.SaveFilePanelInProject(
                "Save ScriptableObject",
                soType.Name,
                "asset",
                "Please enter a file name to save the ScriptableObject to");

            if (string.IsNullOrEmpty(path))
            {
                return; // 如果用户取消了保存，则不执行任何操作
            }

            // 创建资源文件
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 选中新创建的资源
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;
        }
    }
}