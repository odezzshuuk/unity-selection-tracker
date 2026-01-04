using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Odezzshuuk.Editor.SelectionTracker {

  public class UIAssetLocator {

    private static readonly Lazy<UIAssetLocator> s_instance = new(static () => new UIAssetLocator());
    public static UIAssetLocator Instance => s_instance.Value;

    private readonly UIAssetManager _uiAssetManager;

    public StyleSheet GlobalStyle => _uiAssetManager.globalStyle;
    public VisualTreeAsset PreferenceTemplate => _uiAssetManager.preferenceTemplate;
    public VisualTreeAsset WindowTemplate => _uiAssetManager.windowTemplate;
    public VisualTreeAsset EntryTemplate => _uiAssetManager.entryTemplate;
    public VisualTreeAsset DetailInfoTemplate => _uiAssetManager.detailInfoTemplate;

    private UIAssetLocator() {
      string filter = $"t:{typeof(UIAssetManager).FullName}";
      string guid = AssetDatabase.FindAssets(filter).FirstOrDefault();
      _uiAssetManager = AssetDatabase.LoadAssetAtPath<UIAssetManager>(AssetDatabase.GUIDToAssetPath(guid));
    }
  }
}
