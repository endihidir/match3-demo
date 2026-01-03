using System.IO;
using Core.Configs;
using Core.Level;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelDataServiceConfig _config;

        private int _levelNumber = 1;
        private int _gridWidth = 10;
        private int _gridHeight = 10;
        private int _moveCount = 10;

        private JsonGridObjectType[,] _gridObjects;

        [MenuItem("Tools/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnEnable()
        {
            InitializeGrid();
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space();

            DrawConfigField();
            EditorGUILayout.Space();

            DrawLevelControls();
            EditorGUILayout.Space();

            DrawGridControls();
            EditorGUILayout.Space();

            DrawGridTable();
            EditorGUILayout.Space();

            DrawCreateOrOverrideButton();
        }

        #region Header & Config

        private void DrawHeader()
        {
            GUILayout.Label("Level Editor", EditorStyles.boldLabel);
        }

        private void DrawConfigField()
        {
            _config = (LevelDataServiceConfig)EditorGUILayout.ObjectField(
                "Level Data Config",
                _config,
                typeof(LevelDataServiceConfig),
                false
            );

            if (!_config)
            {
                EditorGUILayout.HelpBox("LevelDataServiceConfig is not assigned. I will use LevelsRoot asset location (if it exists) to save JSON files.", MessageType.Info);
            }
            else if (_config.sourceType != LevelSourceType.Resources)
            {
                EditorGUILayout.HelpBox("Currently only LevelSourceType.Resources is supported. (We can add Addressables support later.)", MessageType.Info);
            }
        }

        #endregion

        #region Level & Grid Controls

        private void DrawLevelControls()
        {
            _levelNumber = EditorGUILayout.IntField("Level Number", _levelNumber);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load From File"))
            {
                LoadLevelFromFile();
            }

            if (GUILayout.Button("Clear Grid"))
            {
                InitializeGrid();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGridControls()
        {
            var newGridWidth = EditorGUILayout.IntField("Grid Width", _gridWidth);
            var newGridHeight = EditorGUILayout.IntField("Grid Height", _gridHeight);
            _moveCount = EditorGUILayout.IntField("Move Count", _moveCount);

            if (newGridWidth != _gridWidth || newGridHeight != _gridHeight)
            {
                ResizeGrid(newGridHeight, newGridWidth);
                _gridWidth = newGridWidth;
                _gridHeight = newGridHeight;
            }
        }

        private void DrawGridTable()
        {
            EditorGUILayout.LabelField("Items Table", EditorStyles.boldLabel);

            if (_gridObjects == null)
            {
                InitializeGrid();
            }

            for (int y = 0; y < _gridHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < _gridWidth; x++)
                {
                    _gridObjects[y, x] = (JsonGridObjectType)EditorGUILayout.EnumPopup(_gridObjects[y, x]);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region Create / Override Button

        private void DrawCreateOrOverrideButton()
        {
            if (_config != null && _config.sourceType != LevelSourceType.Resources)
            {
                EditorGUILayout.HelpBox("Currently only LevelSourceType.Resources is supported. (We can add Addressables support later.)", MessageType.Info);
                return;
            }

            var levelExists = LevelFileExists(_levelNumber);
            var buttonLabel = levelExists ? "Override Level" : "Create Level";

            var style = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };

            if (levelExists)
            {
                style.normal.textColor = Color.red;
            }

            if (GUILayout.Button(buttonLabel, style, GUILayout.Height(30)))
            {
                if (levelExists)
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        "Override Level",
                        $"Level {_levelNumber} already exists. Are you sure you want to override it?",
                        "Yes, override",
                        "Cancel"
                    );

                    if (!confirm)
                    {
                        return;
                    }
                }

                SaveOrOverrideLevel();
            }
        }

        #endregion

        #region Grid Management

        private void InitializeGrid()
        {
            _config = Resources.Load<LevelDataServiceConfig>("Configs/LevelDataServiceConfig");

            _gridObjects = new JsonGridObjectType[_gridHeight, _gridWidth];

            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    _gridObjects[y, x] = JsonGridObjectType.rand;
                }
            }
        }

        private void ResizeGrid(int newHeight, int newWidth)
        {
            if (newHeight <= 0 || newWidth <= 0)
            {
                EditorLogger.LogError("Invalid grid dimensions. Height and Width must be greater than 0.");
                return;
            }

            var newGrid = new JsonGridObjectType[newHeight, newWidth];

            var copyHeight = Mathf.Min(_gridHeight, newHeight);
            var copyWidth = Mathf.Min(_gridWidth, newWidth);

            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    newGrid[y, x] = _gridObjects[y, x];
                }
            }

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    if (y >= copyHeight || x >= copyWidth)
                    {
                        newGrid[y, x] = JsonGridObjectType.rand;
                    }
                }
            }

            _gridObjects = newGrid;
            _gridHeight = newHeight;
            _gridWidth = newWidth;
        }

        #endregion

        #region Load / Save

        private void LoadLevelFromFile()
        {
            var path = GetLevelFilePath(_levelNumber);

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Load Level", $"Level {_levelNumber} file was not found.\n{path}", "OK");
                return;
            }

            var json = File.ReadAllText(path);
            var levelJson = JsonUtility.FromJson<LevelJson>(json);

            if (levelJson == null)
            {
                EditorLogger.LogError($"Failed to parse LevelJson from path: {path}");
                return;
            }

            _gridWidth = levelJson.grid_width;
            _gridHeight = levelJson.grid_height;
            _moveCount = levelJson.move_count;

            _gridObjects = LevelJsonEditorUtils.ToEditorGrid(levelJson);

            EditorLogger.Log($"Level {_levelNumber} loaded from {path}");
        }

        private void SaveOrOverrideLevel()
        {
            var path = GetLevelFilePath(_levelNumber);

            var json = LevelJsonEditorUtils.BuildPrettyPrintedJson(_levelNumber, _gridWidth, _gridHeight, _moveCount, _gridObjects);

            File.WriteAllText(path, json);
            EditorLogger.Log($"Level {_levelNumber} saved to {path}");

            AssetDatabase.Refresh();
        }

        #endregion

        #region File Helpers

        private bool LevelFileExists(int levelNumber)
        {
            var path = GetLevelFilePath(levelNumber);
            return File.Exists(path);
        }

        private string GetLevelFilePath(int levelNumber)
        {
            var folderPath = ResolveLevelsFolderForWrite();
            var fileName = ResolveLevelFileName(levelNumber);

            var fullPath = Path.Combine(folderPath, fileName + ".json");
            EnsureFolderExists(fullPath);

            return fullPath;
        }

        private string ResolveLevelFileName(int levelNumber)
        {
            var fileNameFormat = (!_config || string.IsNullOrEmpty(_config.fileNameFormat))
                ? "level_{0:00}"
                : _config.fileNameFormat;

            return string.Format(fileNameFormat, levelNumber);
        }

        private string ResolveLevelsFolderForWrite()
        {
            var levelsRootPath = TryGetLevelsRootFolderAssetPath();
            if (!string.IsNullOrEmpty(levelsRootPath))
            {
                return levelsRootPath;
            }
            
            if (_config && !string.IsNullOrEmpty(_config.resourcesFolder))
            {
                var p = _config.resourcesFolder.Replace("\\", "/");
                
                if (!p.StartsWith("Assets/"))
                {
                    p = Path.Combine("Assets", p).Replace("\\", "/");
                }

                return p;
            }

            return Path.Combine("Assets", "_Project", "Resources", "Levels");
        }

        private string TryGetLevelsRootFolderAssetPath()
        {
            var guids = AssetDatabase.FindAssets("t:LevelsRootSO");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        }

        private void EnsureFolderExists(string fullPath)
        {
            var folder = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        #endregion
    }

}