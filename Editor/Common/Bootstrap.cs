using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Odezzshuuk.Editor.SelectionTracker {

  [InitializeOnLoad]
  public static class Bootstrap {

    private static readonly PreferencePersistence s_PreferenceOptions = PreferencePersistence.instance;

    static Bootstrap() {
      Selection.selectionChanged += SelectionChangedCallback;

      SceneManager.sceneLoaded += SceneLoadedCallback;
      EditorSceneManager.sceneOpened += SceneOpenedCallback;

      PrefabStage.prefabStageOpened += PrefabStageOpenedCallback;
      PrefabStage.prefabStageClosing += static (prefabStage) => {
        Scene activeScene = SceneManager.GetActiveScene();
        ScanAllComponentsInScene(activeScene).GetAwaiter().GetResult();
      };
    }

    private static void SelectionChangedCallback() {
      if (Selection.activeObject == null) {
        return;
      }

      bool isGameObject = Selection.activeObject is GameObject;

      if (isGameObject && !s_PreferenceOptions.GetToggleValue(Constants.RECORD_GAMEOBJECTS_KEY)) {
        return;
      }

      Entry entry = EntryFactory.Create(Selection.activeObject);
      EntryServicePersistence.instance.RecordSelection(entry);
    }

    [Shortcut("Selection Tracker/Previous Selection", KeyCode.O, ShortcutModifiers.Control)]
    public static void PreviousSelection() {
      Entry selection = EntryServicePersistence.instance.JumpToPreviousSelection();
      JumpToSelection(selection);
    }

    [Shortcut("SelectionTracker/Next Selection", KeyCode.I, ShortcutModifiers.Control)]
    public static void NextSelection() {
      Entry selection = EntryServicePersistence.instance.JumpToNextSelection();
      JumpToSelection(selection);
    }

    private static void JumpToSelection(Entry entry) {
      Object obj = entry?.Ref;
      if (obj != null) {
        Selection.activeObject = obj;
      } else {
        if (entry.RefState.HasFlag(RefState.Unloaded)) {
          entry.Ping();
        }
      }
    }

    public static async Awaitable ScanAllComponentsInScene(Scene scene) {

      await Awaitable.MainThreadAsync();

      SceneComponentsService service = EntryServicePersistence.instance.GetService<SceneComponentsService>();
      service.Entries.Clear();
      if (!scene.IsValid() || !scene.isLoaded) {
        Debug.LogWarning($"Scene {scene.name} is not valid or not loaded.");
        return;
      }

      HashSet<Type> uniqueComponentTypes = new();

      // GameObjects in current scene
      GameObject[] rootObjects = scene.GetRootGameObjects();
      foreach (GameObject rootObj in rootObjects) {
        ScanGameObjectAndChildren(rootObj, uniqueComponentTypes);
      }

      // GameObjects in DontDestroyOnLoad scene
      if (Application.isPlaying) {
        GameObject temp = new("TempForDDOL");
        Object.DontDestroyOnLoad(temp);
        Scene dontDestroyOnLoadScene = temp.scene;
        Object.DestroyImmediate(temp);

        if (dontDestroyOnLoadScene.IsValid()) {
          GameObject[] ddolRootObjects = dontDestroyOnLoadScene.GetRootGameObjects();
          foreach (GameObject rootObj in ddolRootObjects) {
            ScanGameObjectAndChildren(rootObj, uniqueComponentTypes);
          }
        }
      }

      service.Refresh();
    }

    private static void ScanGameObjectAndChildren(GameObject obj, HashSet<Type> uniqueTypes) {
      // Get all components on this GameObject
      Component[] components = obj.GetComponents<Component>();
      foreach (Component component in components) {
        if (component != null) {

          if (component is Transform) {
            continue; // Skip Transform components
          }

          if (uniqueTypes.Add(component.GetType())) {
            EntryServicePersistence.instance.RecordComponent(EntryFactory.Create(component));
          }
        }
      }

      // Recursively scan children
      Transform transform = obj.transform;
      for (int i = 0; i < transform.childCount; i++) {
        ScanGameObjectAndChildren(transform.GetChild(i).gameObject, uniqueTypes);
      }
    }

    private static async void SceneOpenedCallback(Scene scene, OpenSceneMode mode) {
      await ScanAllComponentsInScene(scene);
    }

    private static async void SceneLoadedCallback(Scene scene, LoadSceneMode mode) {
      // Seems scene DontDestroyOnLoad load a little later than loaded scene
      await Awaitable.NextFrameAsync();
      await ScanAllComponentsInScene(scene);
    }

    private static async void PrefabStageOpenedCallback(PrefabStage prefabStage) {
      await ScanAllComponentsInScene(prefabStage.scene);
    }
  }
}

