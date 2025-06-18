using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TooIEditor : EditorWindow
{
    //[MenuItem("Tools/TooIEditor")]

    //static void ShowWindow()
    //{
    //    var window = EditorWindow.GetWindow<TooIEditor>("Base Editor");
    //    window.Show();
    //    window.minSize = new Vector2(300, 200);
    //    window.maxSize = new Vector2(800, 400);

    //}


// 确保编辑器脚本放在Editor文件夹下


    // 存储选中的游戏对象及其信息
    private GameObject selectedGameObject;
    private Vector3 position;
    private Vector3 scale;
    private Quaternion rotation;
    private string objectName;

    // 用于存储和加载的序列化数据类
    [Serializable]
    private class ObjectData
    {
        public string name;
        public float posX, posY, posZ;
        public float scaleX, scaleY, scaleZ;
        public float rotX, rotY, rotZ, rotW;
    }

    // 菜单条目创建
    [MenuItem("编辑器/自定义编辑器")]
    public static void ShowWindow()
    {
        // 获取或创建窗口
        GetWindow<TooIEditor>("Basic Editor");
    }

    // 窗口初始化
    private void OnEnable()
    {
        // 设置窗口最小和最大尺寸
        minSize = new Vector2(300, 200);
        maxSize = new Vector2(800, 400);

        // 监听选择变化事件
        Selection.selectionChanged += OnSelectionChanged;
    }

    // 窗口销毁时清理
    private void OnDisable()
    {
        // 移除选择变化事件监听
        Selection.selectionChanged -= OnSelectionChanged;
    }

    // 选择变化时的回调
    private void OnSelectionChanged()
    {
        // 更新选中的游戏对象
        if (Selection.activeGameObject != null)
        {
            selectedGameObject = Selection.activeGameObject;
            UpdateObjectInfo();
        }
        else
        {
            selectedGameObject = null;
        }
    }

    // 更新对象信息
    private void UpdateObjectInfo()
    {
        if (selectedGameObject != null)
        {
            Transform transform = selectedGameObject.transform;
            objectName = selectedGameObject.name;
            position = transform.position;
            scale = transform.localScale;
            rotation = transform.rotation;
        }
    }

    // 绘制窗口内容
    private void OnGUI()
    {
        // 标题样式
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.UpperCenter;
        titleStyle.margin = new RectOffset(0, 0, 10, 10);

        // 绘制标题
        GUILayout.Label("Basic Editor - 对象信息编辑器", titleStyle);

        // 分隔线
       // EditorGUI.DrawSeparator(new Rect(0, 30, position.width, 2));

        // 显示选中对象信息或提示
        if (selectedGameObject == null)
        {
            GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.fontStyle = FontStyle.Italic;
            warningStyle.normal.textColor = Color.red;
            GUILayout.Label("No object selected", warningStyle);
        }
        else
        {
            // 对象名称编辑
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label("对象名称:", labelStyle);
            objectName = EditorGUILayout.TextField(objectName);

            // 分隔线
          //  EditorGUI.DrawSeparator(new Rect(0, 70, position.width, 1));

            // 位置编辑
            GUILayout.Label("位置:", labelStyle);
            position = EditorGUILayout.Vector3Field("", position);

            // 旋转编辑
            GUILayout.Label("旋转:", labelStyle);
            rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", rotation.eulerAngles));

            // 缩放编辑
            GUILayout.Label("缩放:", labelStyle);
            scale = EditorGUILayout.Vector3Field("", scale);

            // 应用更改按钮
            if (GUILayout.Button("应用更改"))
            {
                if (selectedGameObject != null)
                {
                    Transform transform = selectedGameObject.transform;
                    transform.position = position;
                    transform.rotation = rotation;
                    transform.localScale = scale;
                    selectedGameObject.name = objectName;

                    // 刷新选择以更新Unity编辑器中的显示
                    Selection.activeGameObject = selectedGameObject;
                }
            }
        }

        // 按钮区域
        EditorGUILayout.Space(20);
        GUILayout.BeginHorizontal();

        // 清空按钮
        if (GUILayout.Button("清空", GUILayout.Width(100)))
        {
            ClearEditor();
        }

        // 保存按钮
        if (GUILayout.Button("保存", GUILayout.Width(100)))
        {
            SaveObjectInfo();
        }

        // 加载按钮
        if (GUILayout.Button("加载", GUILayout.Width(100)))
        {
            LoadObjectInfo();
        }

        GUILayout.EndHorizontal();
    }

    // 清空编辑器内容
    private void ClearEditor()
    {
        selectedGameObject = null;
        objectName = "";
        position = Vector3.zero;
        scale = Vector3.one;
        rotation = Quaternion.identity;

        // 清除选择
        Selection.activeGameObject = null;
    }

    // 保存对象信息到文件
    private void SaveObjectInfo()
    {
        if (selectedGameObject == null)
        {
            Debug.Log("没有选中的对象，无法保存");
            return;
        }

        try
        {
            // 创建数据对象
            ObjectData data = new ObjectData();
            data.name = objectName;
            data.posX = position.x;
            data.posY = position.y;
            data.posZ = position.z;
            data.scaleX = scale.x;
            data.scaleY = scale.y;
            data.scaleZ = scale.z;
            data.rotX = rotation.x;
            data.rotY = rotation.y;
            data.rotZ = rotation.z;
            data.rotW = rotation.w;

            // 转换为JSON
            string json = JsonUtility.ToJson(data);

            // 保存路径 (使用持久化数据路径)
            string path = Path.Combine(Application.persistentDataPath, "SelectedObjectInfo.txt");

            // 写入文件
            File.WriteAllText(path, json);

            Debug.Log("对象信息已保存到: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save the file! " + e.Message);
        }
    }

    // 从文件加载对象信息
    private void LoadObjectInfo()
    {
        try
        {
            // 加载路径
            string path = Path.Combine(Application.persistentDataPath, "SelectedObjectInfo.txt");

            // 检查文件是否存在
            if (!File.Exists(path))
            {
                Debug.LogError("File not found!");
                return;
            }

            // 读取文件
            string json = File.ReadAllText(path);

            // 解析JSON
            ObjectData data = JsonUtility.FromJson<ObjectData>(json);

            // 应用加载的信息
            if (selectedGameObject != null)
            {
                selectedGameObject.name = data.name;
                Transform transform = selectedGameObject.transform;
                transform.position = new Vector3(data.posX, data.posY, data.posZ);
                transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
                transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

                // 更新本地变量
                objectName = data.name;
                position = new Vector3(data.posX, data.posY, data.posZ);
                scale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
                rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

                Debug.Log("对象信息已从文件加载");
            }
            else
            {
                Debug.Log("没有选中的对象，无法加载");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("加载文件时出错: " + e.Message);
        }
    }
}

