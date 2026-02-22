using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectDirectoryTools
{
    /// <summary>
    /// 项目目录扫描器 - Unity编辑器工具
    /// </summary>
    public class ProjectDirectoryScannerEditor : EditorWindow
    {
        private string outputFileName = "ProjectDirectoryTree";
        private bool includeFiles = false;
        private bool useEmojiIcons = true;
        private string foldersToExclude = ".git;Library;Logs;Temp;Obj;Build;.vs";
        private string extensionsToExclude = ".meta;.tmp;.temp;.DS_Store";
        private int maxDepth = 0;
        private bool showFileCount = true;
        private bool openAfterGeneration = true;
        private bool relativeToAssets = true;
        
        private Vector2 scrollPosition;
        private string lastOutputPath;
        private int totalScannedItems;
        private float progress = 0f;
        private bool isScanning = false;
        
        // 样式
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle statusStyle;

        [MenuItem("Tools/项目目录扫描器")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectDirectoryScannerEditor>("目录扫描器");
            window.minSize = new Vector2(450, 550);
            window.Show();
        }

        private void OnEnable()
        {
            // 从EditorPrefs加载设置
            LoadSettings();
        }

        private void OnGUI()
        {
            InitializeStyles();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawSettings();
            DrawButtons();
            DrawStatus();
            
            EditorGUILayout.EndScrollView();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.largeLabel)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 10, 20)
                };
                
                boxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(15, 15, 15, 15),
                    margin = new RectOffset(5, 5, 10, 10)
                };
                
                statusStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(5, 5, 10, 10),
                    fontSize = 11
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("📁 项目目录结构扫描器", headerStyle);
            EditorGUILayout.Separator();
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("输出设置", EditorStyles.boldLabel);
            outputFileName = EditorGUILayout.TextField("输出文件名", outputFileName);
            openAfterGeneration = EditorGUILayout.Toggle("生成后打开", openAfterGeneration);
            relativeToAssets = EditorGUILayout.Toggle("相对Assets路径", relativeToAssets);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("扫描设置", EditorStyles.boldLabel);
            includeFiles = EditorGUILayout.Toggle("包含文件", includeFiles);
            showFileCount = EditorGUILayout.Toggle("显示项目计数", showFileCount);
            useEmojiIcons = EditorGUILayout.Toggle("使用表情图标", useEmojiIcons);
            maxDepth = EditorGUILayout.IntSlider("最大深度 (0=无限制)", maxDepth, 0, 10);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("排除设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("使用分号(;)分隔多个项目", MessageType.Info);
            foldersToExclude = EditorGUILayout.TextField("排除的文件夹", foldersToExclude);
            extensionsToExclude = EditorGUILayout.TextField("排除的扩展名", extensionsToExclude);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !isScanning;
            
            if (GUILayout.Button("🔍 扫描并生成", GUILayout.Height(40)))
            {
                ScanAndGenerate();
            }
            
            if (GUILayout.Button("⚡ 快速扫描（仅目录）", GUILayout.Height(40)))
            {
                QuickScan();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("💾 保存设置"))
            {
                SaveSettings();
            }
            
            if (GUILayout.Button("🔄 恢复默认"))
            {
                RestoreDefaults();
            }
            
            if (!string.IsNullOrEmpty(lastOutputPath) && File.Exists(lastOutputPath))
            {
                if (GUILayout.Button("📄 打开上次文件"))
                {
                    EditorUtility.RevealInFinder(lastOutputPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            if (isScanning)
            {
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, progress, "正在扫描...");
                Repaint();
            }
            
            if (totalScannedItems > 0)
            {
                EditorGUILayout.BeginVertical(statusStyle);
                EditorGUILayout.LabelField($"上次扫描: {totalScannedItems} 个项目", EditorStyles.miniBoldLabel);
                if (!string.IsNullOrEmpty(lastOutputPath))
                {
                    EditorGUILayout.LabelField($"输出文件: {lastOutputPath}", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 扫描并生成目录树
        /// </summary>
        private void ScanAndGenerate()
        {
            try
            {
                isScanning = true;
                progress = 0f;
                
                // 确定扫描根路径
                string rootPath = relativeToAssets ? "Assets" : Application.dataPath + "/..";
                string fullRootPath = relativeToAssets ? 
                    Path.Combine(Application.dataPath) : 
                    Path.GetDirectoryName(Application.dataPath);
                
                // 输出路径
                string outputPath = Path.Combine(Application.dataPath, "..", outputFileName + ".txt");
                
                // 准备排除列表
                string[] excludedFolders = foldersToExclude.Split(';');
                string[] excludedExtensions = extensionsToExclude.Split(';');
                
                // 构建目录树
                StringBuilder directoryTree = new StringBuilder();
                AddHeader(directoryTree, rootPath);
                
                // 扫描目录
                totalScannedItems = ScanDirectoryRecursive(rootPath, "", directoryTree, 
                    excludedFolders, excludedExtensions, 0);
                
                // 添加统计信息
                if (showFileCount)
                {
                    directoryTree.AppendLine();
                    directoryTree.AppendLine($"总计扫描: {totalScannedItems} 个项目");
                }
                
                directoryTree.AppendLine($"生成时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                directoryTree.AppendLine($"生成工具: Unity项目目录扫描器 v1.0");
                
                // 写入文件
                File.WriteAllText(outputPath, directoryTree.ToString(), Encoding.UTF8);
                lastOutputPath = outputPath;
                
                // 完成后操作
                if (openAfterGeneration)
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
                
                Debug.Log($"✅ 目录树已生成: {outputPath} (共 {totalScannedItems} 个项目)");
                EditorUtility.DisplayDialog("完成", $"目录树已生成！\n共扫描 {totalScannedItems} 个项目", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 扫描失败: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"扫描失败: {e.Message}", "确定");
            }
            finally
            {
                isScanning = false;
                progress = 0f;
            }
        }

        /// <summary>
        /// 快速扫描（仅目录）
        /// </summary>
        private void QuickScan()
        {
            try
            {
                string outputPath = Path.Combine(Application.dataPath, "..", "QuickDirectoryTree.txt");
                StringBuilder tree = new StringBuilder();
                
                tree.AppendLine("快速目录扫描");
                tree.AppendLine("=".PadRight(60, '='));
                
                ScanDirectorySimple("Assets", tree, 0);
                
                File.WriteAllText(outputPath, tree.ToString(), Encoding.UTF8);
                lastOutputPath = outputPath;
                
                EditorUtility.RevealInFinder(outputPath);
                Debug.Log($"⚡ 快速目录树已生成: {outputPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"快速扫描失败: {e.Message}");
            }
        }

        /// <summary>
        /// 递归扫描目录
        /// </summary>
        private int ScanDirectoryRecursive(string basePath, string relativePath, StringBuilder output,
            string[] excludedFolders, string[] excludedExtensions, int currentDepth)
        {
            if (maxDepth > 0 && currentDepth > maxDepth)
                return 0;
            
            string fullPath = Path.Combine(basePath, relativePath);
            if (!Directory.Exists(fullPath))
                return 0;
            
            // 检查是否排除
            string folderName = Path.GetFileName(relativePath);
            if (ShouldExcludeFolder(folderName, excludedFolders))
                return 0;
            
            // 更新进度
            progress = Mathf.Clamp01((float)currentDepth / 10f);
            
            // 添加目录到输出
            string indent = new string(' ', currentDepth * 2);
            string prefix = GetDepthPrefix(currentDepth);
            output.AppendLine($"{indent}{prefix}{folderName}/");
            
            int itemCount = 0;
            
            try
            {
                // 扫描子目录
                string[] subDirectories = Directory.GetDirectories(fullPath);
                foreach (string subDir in subDirectories)
                {
                    string subDirName = Path.GetFileName(subDir);
                    string subRelativePath = string.IsNullOrEmpty(relativePath) ? 
                        subDirName : Path.Combine(relativePath, subDirName);
                    
                    itemCount += ScanDirectoryRecursive(basePath, subRelativePath, output, 
                        excludedFolders, excludedExtensions, currentDepth + 1);
                }
                
                // 扫描文件
                if (includeFiles)
                {
                    string[] files = Directory.GetFiles(fullPath);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string extension = Path.GetExtension(file);
                        
                        if (ShouldExcludeExtension(extension, excludedExtensions))
                            continue;
                        
                        string fileIndent = new string(' ', (currentDepth + 1) * 2);
                        string fileIcon = useEmojiIcons ? GetFileIcon(extension) : "•";
                        output.AppendLine($"{fileIndent}{fileIcon} {fileName}");
                        itemCount++;
                    }
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                string errorIndent = new string(' ', (currentDepth + 1) * 2);
                output.AppendLine($"{errorIndent}⚠️ [访问被拒绝]");
            }
            catch (System.Exception e)
            {
                string errorIndent = new string(' ', (currentDepth + 1) * 2);
                output.AppendLine($"{errorIndent}❌ [错误: {e.Message}]");
            }
            
            return itemCount + 1;
        }

        /// <summary>
        /// 简单扫描（仅目录）
        /// </summary>
        private static void ScanDirectorySimple(string path, StringBuilder output, int depth)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path);
            
            if (!Directory.Exists(fullPath))
                return;
            
            // 跳过排除的文件夹
            string folderName = Path.GetFileName(path);
            if (folderName.StartsWith(".") || 
                folderName.Equals("Library", System.StringComparison.OrdinalIgnoreCase) ||
                folderName.Equals("Temp", System.StringComparison.OrdinalIgnoreCase))
                return;
            
            // 添加缩进和前缀
            string indent = new string(' ', depth * 2);
            string prefix = depth == 0 ? "📁 " : "├─ ";
            output.AppendLine($"{indent}{prefix}{folderName}/");
            
            // 扫描子目录
            try
            {
                string[] subDirs = Directory.GetDirectories(fullPath);
                for (int i = 0; i < subDirs.Length; i++)
                {
                    string subDirName = Path.GetFileName(subDirs[i]);
                    ScanDirectorySimple(Path.Combine(path, subDirName), output, depth + 1);
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 添加文件头信息
        /// </summary>
        private void AddHeader(StringBuilder sb, string rootPath)
        {
            sb.AppendLine("Unity项目目录结构");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"项目名称: {Application.productName}");
            sb.AppendLine($"Unity版本: {Application.unityVersion}");
            sb.AppendLine($"扫描根路径: {rootPath}");
            sb.AppendLine($"包含文件: {(includeFiles ? "是" : "否")}");
            sb.AppendLine($"扫描深度: {(maxDepth == 0 ? "无限制" : maxDepth.ToString())}");
            sb.AppendLine();
        }

        /// <summary>
        /// 检查是否排除文件夹
        /// </summary>
        private bool ShouldExcludeFolder(string folderName, string[] excludedFolders)
        {
            foreach (string excludedFolder in excludedFolders)
            {
                if (!string.IsNullOrEmpty(excludedFolder.Trim()) && 
                    folderName.Equals(excludedFolder.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否排除扩展名
        /// </summary>
        private bool ShouldExcludeExtension(string extension, string[] excludedExtensions)
        {
            foreach (string excludedExtension in excludedExtensions)
            {
                if (!string.IsNullOrEmpty(excludedExtension.Trim()) && 
                    extension.Equals(excludedExtension.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取深度前缀
        /// </summary>
        private string GetDepthPrefix(int depth)
        {
            if (!useEmojiIcons) return "";
            
            if (depth == 0) return "📁 ";
            if (depth == 1) return "├─ ";
            if (depth == 2) return "│  ├─ ";
            if (depth == 3) return "│  │  ├─ ";
            return new string(' ', depth * 2 - 4) + "├─ ";
        }

        /// <summary>
        /// 获取文件图标
        /// </summary>
        private string GetFileIcon(string extension)
        {
            if (!useEmojiIcons) return "•";
            
            switch (extension.ToLower())
            {
                case ".cs": return "📄";
                case ".shader": case ".cginc": case ".hlsl": return "🔷";
                case ".mat": return "🎨";
                case ".prefab": return "📦";
                case ".unity": return "🏠";
                case ".png": case ".jpg": case ".jpeg": case ".tga": case ".psd": return "🖼️";
                case ".fbx": case ".obj": case ".blend": return "🎯";
                case ".wav": case ".mp3": case ".ogg": return "🎵";
                case ".txt": case ".json": case ".xml": case ".yaml": case ".yml": return "📝";
                case ".asset": return "💾";
                case ".anim": case ".controller": return "🎬";
                case ".ttf": case ".otf": return "🔤";
                default: return "📄";
            }
        }

        /// <summary>
        /// 保存设置到EditorPrefs
        /// </summary>
        private void SaveSettings()
        {
            EditorPrefs.SetString("PDS_OutputFileName", outputFileName);
            EditorPrefs.SetBool("PDS_IncludeFiles", includeFiles);
            EditorPrefs.SetBool("PDS_UseEmojiIcons", useEmojiIcons);
            EditorPrefs.SetString("PDS_FoldersToExclude", foldersToExclude);
            EditorPrefs.SetString("PDS_ExtensionsToExclude", extensionsToExclude);
            EditorPrefs.SetInt("PDS_MaxDepth", maxDepth);
            EditorPrefs.SetBool("PDS_ShowFileCount", showFileCount);
            EditorPrefs.SetBool("PDS_OpenAfterGeneration", openAfterGeneration);
            EditorPrefs.SetBool("PDS_RelativeToAssets", relativeToAssets);
            
            Debug.Log("✅ 设置已保存");
        }

        /// <summary>
        /// 从EditorPrefs加载设置
        /// </summary>
        private void LoadSettings()
        {
            outputFileName = EditorPrefs.GetString("PDS_OutputFileName", "ProjectDirectoryTree");
            includeFiles = EditorPrefs.GetBool("PDS_IncludeFiles", false);
            useEmojiIcons = EditorPrefs.GetBool("PDS_UseEmojiIcons", true);
            foldersToExclude = EditorPrefs.GetString("PDS_FoldersToExclude", ".git;Library;Logs;Temp;Obj;Build;.vs");
            extensionsToExclude = EditorPrefs.GetString("PDS_ExtensionsToExclude", ".meta;.tmp;.temp;.DS_Store");
            maxDepth = EditorPrefs.GetInt("PDS_MaxDepth", 0);
            showFileCount = EditorPrefs.GetBool("PDS_ShowFileCount", true);
            openAfterGeneration = EditorPrefs.GetBool("PDS_OpenAfterGeneration", true);
            relativeToAssets = EditorPrefs.GetBool("PDS_RelativeToAssets", true);
        }

        /// <summary>
        /// 恢复默认设置
        /// </summary>
        private void RestoreDefaults()
        {
            outputFileName = "ProjectDirectoryTree";
            includeFiles = false;
            useEmojiIcons = true;
            foldersToExclude = ".git;Library;Logs;Temp;Obj;Build;.vs";
            extensionsToExclude = ".meta;.tmp;.temp;.DS_Store";
            maxDepth = 0;
            showFileCount = true;
            openAfterGeneration = true;
            relativeToAssets = true;
            
            SaveSettings();
            Debug.Log("🔄 已恢复默认设置");
        }
    }
}