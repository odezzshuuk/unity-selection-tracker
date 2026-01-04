using Odezzshuuk.Editor.SelectionTracker;
using UnityEditor;
using static Odezzshuuk.Editor.SelectionTracker.Constants;
using static Odezzshuuk.Editor.SelectionTracker.UnityBuiltInIcons;

namespace UnityEngine.UIElements {

  [UxmlElement("Entry")]
  public partial class EntryElement : VisualElement {

    private readonly VisualElement _entryRoot;
    private readonly IEntryService _entryService;
    private readonly Label _entryLabel;
    private readonly Image _entryIcon;
    private readonly Image _pingIcon;
    private readonly Image _openIcon;
    private readonly Image _favoriteIcon;
    private readonly VisualElement _entryPopupRoot;
    private bool _isFavorite = false;
    private readonly string _entryLabelDefaultClassName = "entry__label";

    public int Index { get; set; }
    public string EntryText => _entryLabel.text;

    private Entry _entry;
    public Entry Entry {
      get => _entry;
      set => SetupEntry(value);
    }

    public EntryElement() {
      _entryRoot = UIAssetLocator.Instance.EntryTemplate.Instantiate();
      this.AddManipulator(new ContextualMenuManipulator(evt => {
        evt.menu.AppendAction("Remove", _ => _entryService.RemoveEntry(Entry), DropdownMenuAction.AlwaysEnabled);
        evt.StopPropagation();
      }));

      VisualElement info = _entryRoot.Q<VisualElement>("Info");
      info?.AddManipulator(new DragAndLeftClickManipulator(this));
      // info?.AddManipulator(new OnHoverManipulator(this));

      RegisterCallback<AttachToPanelEvent>(evt => {
        PreferencePersistence.instance.onUpdated += PreferenceUpdatedCallback;
      });
      RegisterCallback<DetachFromPanelEvent>(evt => {
        PreferencePersistence.instance.onUpdated -= PreferenceUpdatedCallback;
      });

      _entryLabel = _entryRoot.Q<Label>("Name");
      _entryIcon = _entryRoot.Q<Image>("Icon");
      _pingIcon = _entryRoot.Q<Image>("PingIcon");
      _openIcon = _entryRoot.Q<Image>("OpenPrefabIcon");
      _favoriteIcon = _entryRoot.Q<Image>("FavoriteIcon");
      _entryPopupRoot = _entryRoot.Q<VisualElement>("PopupDetail");

      if (_pingIcon != null) {
        _pingIcon.image = EditorGUIUtility.IconContent(SEARCH_ICON_NAME).image;
        _pingIcon.RegisterCallback<MouseUpEvent>(PingIconMouseUpCallback);
      }

      if (_openIcon != null) {
        _openIcon.image = EditorGUIUtility.IconContent(OPEN_ASSET_ICON_NAME).image;
        _openIcon.RegisterCallback<MouseUpEvent>(OpenIconMouseUpCallback);
      }

      _favoriteIcon.style.display = PreferencePersistence.instance.GetToggleValue(DRAW_FAVORITES_KEY)
        ? DisplayStyle.Flex
        : DisplayStyle.None;

      _favoriteIcon.RegisterCallback<MouseUpEvent>(FavoriteIconCallback);
      _favoriteIcon.image = EditorGUIUtility.IconContent(FAVORITE_EMPTY_ICON_NAME).image;

      Add(_entryRoot);
    }

    public EntryElement(int index, IEntryService service) : this() {
      Index = index;
      _entryService = service;
    }

    public void Reset() {
      Entry = null;
    }

    public IEntryService GetEntryService() {
      return _entryService;
    }

    public void PopupDetail() {
      _entryPopupRoot.style.display = DisplayStyle.Flex;
    }

    public void HideDetail() {
      _entryPopupRoot.style.display = DisplayStyle.None;
    }

    private void FavoriteIconCallback(MouseUpEvent evt) {
      if (Entry == null) {
        return;
      }

      _isFavorite = !_isFavorite;
      EntryServicePersistence.instance.RecordFavorites(Entry, _isFavorite);
    }

    private void SetupEntry(Entry value) {
      if (value == null) {
        style.display = DisplayStyle.None;
        _entry?.onFavoriteChanged.RemoveListener(FavoriteChangedCallback);
        _entry = null;
        _entryLabel.text = string.Empty;
        _entryIcon.image = null;
        return;
      }

      _entry?.onFavoriteChanged.RemoveListener(FavoriteChangedCallback);
      _entry = value;
      _entry.onFavoriteChanged.AddListener(FavoriteChangedCallback);

      if (_entryLabel != null) {
        SetNameLabel(value);
      }

      if (_entryIcon != null) {
        _entryIcon.image = value.Icon;
      }

      if (_openIcon != null) {
        _openIcon.style.display = value.RefState.HasFlag(RefState.GameObject)
          ? DisplayStyle.None
          : DisplayStyle.Flex;
      }

      if (_favoriteIcon != null) {
        _isFavorite = value.IsFavorite;
        _favoriteIcon.image = _isFavorite
          ? EditorGUIUtility.IconContent(FAVORITE_ICON_NAME).image
          : EditorGUIUtility.IconContent(FAVORITE_EMPTY_ICON_NAME).image;
      }
    }

    private void SetNameLabel(Entry value) {
      if (Entry == null) {
        return;
      }
      _entryLabel.text = value.DisplayName;

      switch (Entry.RefState) {
        case RefState.Loaded:
        case RefState.Staged:
          AddModifierClassToLabel(ACTIVATED_MODIFIER_CLASS_NAME);
          break;
        case RefState.Unloaded:
        case RefState.Unstaged:
          AddModifierClassToLabel(UNLOADED_MODIFIER_CLASS_NAME);
          break;
        case RefState.Destroyed:
        case RefState.Deleted:
          AddModifierClassToLabel(DELETED_MODIFIER_CLASS_NAME);
          break;
        default:
          _entryLabel.ClearClassList();
          _entryLabel.AddToClassList(_entryLabelDefaultClassName);
          break;
      }
    }

    private void AddModifierClassToLabel(string modifier) {
      _entryLabel.ClearClassList();
      _entryLabel.AddToClassList(_entryLabelDefaultClassName);
      _entryLabel.AddToClassList(modifier);
    }


    private void PingIconMouseUpCallback(MouseUpEvent evt) {
      if (Entry == null) {
        return;
      }

      Entry.Ping();
      evt.StopPropagation();
    }

    private void OpenIconMouseUpCallback(MouseUpEvent evt) {
      Entry.Open();
      evt.StopPropagation();
    }

    private void FavoriteChangedCallback(bool value) {
      _favoriteIcon.image = value
        ? EditorGUIUtility.IconContent(FAVORITE_ICON_NAME).image
        : EditorGUIUtility.IconContent(FAVORITE_EMPTY_ICON_NAME).image;
    }

    private void PreferenceUpdatedCallback() {
      _favoriteIcon.style.display = PreferencePersistence.instance.GetToggleValue(DRAW_FAVORITES_KEY)
        ? DisplayStyle.Flex
        : DisplayStyle.None;
    }

  }
}
