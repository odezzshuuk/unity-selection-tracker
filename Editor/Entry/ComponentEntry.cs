using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Synaptafin.Editor.SelectionTracker {

  public class ComponentEntry : Entry {

    private readonly ComponentListSupportService _componentListService;
    public override string DisplayName => Ref.GetType().Name;

    public ComponentEntry(Component component, GlobalObjectId id) : base(component, id) {
      _componentListService = EntryServicePersistence.instance.GetService<ComponentListSupportService>();
    }

    public override bool Equals(Entry other) {

      if (other.Ref != null && Ref != null) {
        return other.Ref.GetType() == Ref.GetType();
      }

      return false;
    }

    public override void Ping() {
      _componentListService.Entries.Clear();
      if (Ref == null) {
        Debug.LogWarning("Cannot ping: Ref is null");
        return;
      }

      Component component = Ref as Component;
      if (component == null) {
        Debug.LogWarning("Cannot ping: Ref is not a Component");
        return;
      }

      Type componentType = component.GetType();
      Scene activeScene = SceneManager.GetActiveScene();

      if (!activeScene.IsValid() || !activeScene.isLoaded) {
        Debug.LogWarning("Cannot ping: No valid active scene");
        return;
      }

      GameObject[] rootObjects = activeScene.GetRootGameObjects();

      // Current Loaded Scene
      foreach (GameObject rootObj in rootObjects) {
        FindGameObjectsWithComponent(rootObj, componentType);
      }

      // DontDestroyOnLoad Scene
      if (Application.isPlaying) {
        GameObject temp = new("TempForDDOL");
        UnityEngine.Object.DontDestroyOnLoad(temp);
        Scene dontDestroyOnLoadScene = temp.scene;
        UnityEngine.Object.DestroyImmediate(temp);

        if (dontDestroyOnLoadScene.IsValid()) {
          GameObject[] ddolRootObjects = dontDestroyOnLoadScene.GetRootGameObjects();
          foreach (GameObject rootObj in ddolRootObjects) {
            FindGameObjectsWithComponent(rootObj, componentType);
          }
        }
      }

      if (_componentListService.Entries.Count == 0) {
        Debug.Log($"No GameObjects found with component type: {componentType.Name}");
        return;
      }

      _componentListService.OnUpdated.Invoke();

      ComponentListSupportWindow wnd = EditorWindow.GetWindow<ComponentListSupportWindow>();
      GUIContent titleContent = new($"Components: {componentType.Name}({_componentListService.Entries.Count})");
      wnd.titleContent = titleContent;
    }

    public override void Open() {
      if (Ref is MonoBehaviour mb) {
        MonoScript script = MonoScript.FromMonoBehaviour(mb);
        EditorUtility.FocusProjectWindow();
        EditorGUIUtility.PingObject(script);
      } else {
        Debug.LogWarning("Built-in components cannot be opened.");
      }
    }

    private void FindGameObjectsWithComponent(GameObject obj, Type componentType) {
      // Check if this GameObject has the component
      if (obj.GetComponent(componentType) != null) {
        _componentListService?.RecordEntry(EntryFactory.Create(obj));
      }

      // Recursively check children
      Transform transform = obj.transform;
      for (int i = 0; i < transform.childCount; i++) {
        FindGameObjectsWithComponent(transform.GetChild(i).gameObject, componentType);
      }
    }
  }
}
