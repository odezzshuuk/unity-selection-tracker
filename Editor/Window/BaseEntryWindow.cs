using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Odezzshuuk.Editor.SelectionTracker {

  public class BaseEntryWindow<T> : EditorWindow where T : IEntryService, new() {

    public VisualTreeAsset rootVisualTreeAsset;

    protected T _entryService;
    protected VisualElement _windowRoot;
    protected RefState _refStateFilter = RefState.All;
    protected VisualElement _entryContainer;

    private string _searchText;

    public void OnEnable() {
      PreferencePersistence.instance.onUpdated += PreferencesUpdatedCallback;
      _entryService = EntryServicePersistence.instance.GetService<T>();
      _entryService?.OnUpdated.AddListener(EntryServiceUpdatedCallback);
    }

    public void OnDisable() {
      PreferencePersistence.instance.onUpdated -= PreferencesUpdatedCallback;
      _entryService?.OnUpdated.RemoveListener(EntryServiceUpdatedCallback);
    }

    public void CreateGUI() {
      // Query and Reference VisualElement 
      if (_entryService == null) {
        return;
      }
      VisualElement root = rootVisualElement;
      if (rootVisualTreeAsset == null) {
        rootVisualTreeAsset = UIAssetLocator.Instance.WindowTemplate;
      }
      _windowRoot = rootVisualTreeAsset.CloneTree();
      root.Add(_windowRoot);

      _windowRoot.style.width = new StyleLength(Length.Percent(100));
      _windowRoot.style.height = new StyleLength(Length.Percent(100));

      ToolbarSearchField searchBar = _windowRoot.Q<ToolbarSearchField>("SearchField");
      searchBar.RegisterValueChangedCallback(evt => {
        _searchText = evt.newValue;
        ReloadView();
      });
      _entryContainer = _windowRoot.Q<VisualElement>("EntryContainer");

      // Data and Menu
      SetupEntries();
      ReloadView();
      AddContextMenu();
    }

    protected void ReloadView() {
      if (_entryContainer == null) {
        return;
      }
      foreach (EntryElement elt in _entryContainer.Children().OfType<EntryElement>()) {
        bool show = elt.Entry != null && IsMatch(elt) && PassFilter(elt.Entry);
        elt.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
      }
    }

    protected virtual void AddContextMenu() { }

    private void SetupEntries() {
      // _entryContainer can be null if CreateGUI hasn't been called yet
      if (_entryContainer == null) {
        return;
      }
      List<Entry> entries = _entryService.Entries;
      List<EntryElement> entryElements = _entryContainer.Children().OfType<EntryElement>()?.ToList();

      int elementCount = entryElements.Count;
      int counter = 0;
      foreach (Entry entry in entries) {
        if (counter < elementCount) {
          entryElements[counter].Entry = entry;
        } else {
          EntryElement entryElement = new(counter, _entryService) {
            Entry = entry
          };
          _entryContainer.Add(entryElement);
        }
        counter++;
      }

      if (counter < elementCount) {
        for (int i = elementCount - 1; i >= counter; i--) {
          _entryContainer.RemoveAt(i);
        }
      }
    }

    private bool IsMatch(EntryElement elt) {
      if (elt == null) {
        return false;
      }

      if (string.IsNullOrEmpty(_searchText)) {
        return true;
      }

      if (string.IsNullOrEmpty(elt.EntryText)) {
        return false;
      }

      string[] keywords = _searchText.Split(' ');
      bool isMatch = false;
      foreach (string keyword in keywords) {
        if (elt.EntryText.ToLower().Contains(keyword)) {
          isMatch = true;
          break;
        }
      }
      return isMatch;
    }

    private bool PassFilter(Entry entry) {
      if (entry == null) {
        return false;
      }

      if (_refStateFilter != 0 && _refStateFilter.HasFlag(entry.RefState)) {
        return true;
      }

      if (_refStateFilter == 0 && PreferencePersistence.instance.RefStateFilter == RefState.All) {
        return true;
      }

      return false;
    }

    private void PreferencesUpdatedCallback() {
      ReloadView();
    }

    private void EntryServiceUpdatedCallback() {
      SetupEntries();
      ReloadView();
    }
  }
}
