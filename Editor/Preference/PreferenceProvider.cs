using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Odezzshuuk.Editor.SelectionTracker {
  public static class PreferenceProvider {

    private static readonly VisualTreeAsset s_preferenceUxml = UIAssetLocator.Instance.PreferenceTemplate;
    private static readonly StyleSheet s_styleSheet = UIAssetLocator.Instance.GlobalStyle;

    private static readonly PreferencePersistence s_preferencePersistence = PreferencePersistence.instance;

    [SettingsProvider]
    public static SettingsProvider CreateSelectionHistorySettingsProvider() {

      SettingsProvider provider = new("Selection Tracker", SettingsScope.User) {
        label = "Selection Tracker",
        activateHandler = static (searchContext, rootElement) => {

          VisualElement prefRoot = s_preferenceUxml.CloneTree();

          rootElement.Add(prefRoot);
          rootElement.styleSheets.Add(s_styleSheet);

          VisualElement toggleListRoot = prefRoot.Q<VisualElement>("toggle-list-root");

          foreach ((string, bool) keyValue in s_preferencePersistence.Toggles) {
            Toggle toggle = new();
            SetupToggles(toggle, keyValue.Item1, keyValue.Item2);
            toggleListRoot.Add(toggle);
          }

          EnumFlagsField stateFilter = prefRoot.Q<EnumFlagsField>("state-filter");
          stateFilter.Init(s_preferencePersistence.RefStateFilter);
          stateFilter.RegisterCallback<ChangeEvent<RefState>>(static evt => {
            s_preferencePersistence.RefStateFilter = evt.newValue;
            s_preferencePersistence.UpdateSettings();
          });
        }
      };
      return provider;
    }

    private static void SetupToggles(Toggle toggle, string label, bool value) {
      if (toggle == null) {
        Debug.LogWarning("Toggle is null");
        return;
      }
      toggle.label = label;
      toggle.value = value;
      toggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => {
        s_preferencePersistence.SetToggleValue(label, evt.newValue);
        s_preferencePersistence.UpdateSettings();
      });
    }
  }
}
