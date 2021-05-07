using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace Ferdi{
public class GPM_Window : EditorWindow{

    Vector2 ScrollPos;
    GPM_Data GPMSettings = null;
    static AddRequest AddPackage;
    static ListRequest ListPackages;
    static List<string> DownloadedPackages = new List<string>();
    static bool UpdatingListOfPackages;

    [MenuItem("Window/Git Packages Manager")]
    public static void ShowWindow(){
        EditorWindow.GetWindow<GPM_Window>("Git Packages Manager");
    }

    void Awake(){
        GPMSettings = (GPM_Data)AssetDatabase.LoadAssetAtPath("Packages/com.ferdi.gpm/Settings/GPM.asset", typeof(GPM_Data));
        GetListOfPackages();
    }
    
    void OnGUI(){

        if(GPMSettings == null){
            Awake();
        }
        
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.skin.label.margin.left = 10;

        GUIStyle RefreshButton = new GUIStyle (GUI.skin.button);
        RefreshButton.fixedWidth = 24;
        RefreshButton.fixedHeight = 24;
        RefreshButton.margin.right = 10;
        RefreshButton.margin.top = -3;

        GUIStyle AddButton = new GUIStyle (GUI.skin.button);
        AddButton.fixedWidth = 40;
        AddButton.margin.left = 8;
        AddButton.margin.right = 2;

        GUIStyle ShowButton = new GUIStyle (GUI.skin.button);
        ShowButton.fixedWidth = default;
        ShowButton.margin.left = 28;
        ShowButton.margin.right = 22;
        ShowButton.fixedHeight = 20;

        GUILayout.Space(10); 

        EditorGUI.DrawRect(new Rect(0, 0, Screen.width, 1), new Color32(0,0,0,100));
        EditorGUI.DrawRect(new Rect(0, 1, Screen.width, 32), new Color32(0,0,0,25));
        EditorGUI.DrawRect(new Rect(0, 32, Screen.width, 1), new Color32(0,0,0,100));

        EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace();
            GUILayout.Label ("Git Packages Manager", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("  List of git packages :", EditorStyles.boldLabel);

            GUI.enabled = !UpdatingListOfPackages;

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), RefreshButton)){
                GetListOfPackages();
            }

            GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        for (int i =0; i < GPMSettings.GitPackages.Count; i++){

            EditorGUILayout.BeginHorizontal ();
                if(DownloadedPackages.Contains(GPMSettings.GitPackages[i].Repository) || UpdatingListOfPackages){
                    GUI.enabled = false;
                    GUI.backgroundColor =  Color.grey;
                }else{
                    GUI.enabled = true;
                    GUI.backgroundColor =  Color.green;
                }
    
                if (GUILayout.Button("ADD" , AddButton)){
                    AddPackage = Client.Add(GPMSettings.GitPackages[i].Repository);
                    EditorApplication.update += AddPackageProgress;
                }
                GUI.backgroundColor =  new Color32(0,0,0,25);
                GUILayout.Label (GPMSettings.GitPackages[i].DisplayName, GUI.skin.label);
            EditorGUILayout.EndHorizontal ();
            GUILayout.Space(2);
        }

        GUI.enabled = true;

        GUILayout.Space(10);

        DrawUILine(new Color32(0,0,0,100), 1, 0);

        GUIStyle NewStyle = new GUIStyle();
        NewStyle.normal.background = Texture2D.whiteTexture;
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        if (GUILayout.Button("Edit the list of git packages", ShowButton)){
            EditorGUIUtility.PingObject( GPMSettings );
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Settings/GPM.asset");
        }

        GUILayout.Space(20);

        EditorGUILayout.EndScrollView();

        if (GUI.changed){
            EditorUtility.SetDirty(GPMSettings);
            AssetDatabase.SaveAssets();
        }
    }

    static void AddPackageProgress(){
        if (AddPackage.IsCompleted){
            if (AddPackage.Status == StatusCode.Success){
                Debug.Log("Installed: " + AddPackage.Result.packageId);
            }else if (AddPackage.Status >= StatusCode.Failure){
                Debug.Log(AddPackage.Error.message);
            }
            EditorApplication.update -= AddPackageProgress;
            GetListOfPackages();
        }
    }

    static void GetListOfPackages(){
        UpdatingListOfPackages = true;
        ListPackages = Client.List();
        EditorApplication.update += CheckProgress;
    }

    static void CheckProgress(){
        if (ListPackages.IsCompleted){
            
            DownloadedPackages = new List<string>();

            if (ListPackages.Status == StatusCode.Success){
                foreach (var package in ListPackages.Result){
                    if(package.repository != null){
                        DownloadedPackages.Add(package.repository.url);
                    }
                }
            }else if (ListPackages.Status >= StatusCode.Failure){
                Debug.Log(ListPackages.Error.message);
            }

            EditorApplication.update -= CheckProgress;
            UpdatingListOfPackages = false;
        }
    }

    void DrawUILine(Color color, int thickness = 2, int padding = 10){
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
        r.height = thickness;
        r.y += padding/2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
}
}