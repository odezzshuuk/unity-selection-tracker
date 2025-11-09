using UnityEngine;
using UnityEngine.UIElements;

namespace Synaptafin.Editor.SelectionTracker {

  public static class Constants {
    public const string MENU_PATH_PREFIX = "Window/Selection Tracker/";

    public const string UPDATE_WHEN_SELECTION_IN_TRACKER = "Update When Selection In Tracker";
    public const string RECORD_GAMEOBJECTS_KEY = "Record GameObject";
    public const string AUTO_REMOVE_DESTROYED_KEY = "Auto Remove Destroyed";
    public const string AUTO_REMOVE_UNLOADED_KEY = "Auto Remove Unloaded";
    public const string AUTO_REMOVE_DUPLICATED_KEY = "Auto Remove Duplicated";
    public const string DRAW_FAVORITES_KEY = "Draw Favorites";
    public const string ORDER_BY_NEWER_KEY = "Order By Newer";
    public const string BACKGROUND_RECORD_KEY = "Background Record";

    public const string SHOW_UNLOADED_GAMEOBJECTS_KEY = "Show Unloaded Objects";
    public const string SHOW_DESTROYED_GAMEOBJECTS_KEY = "Show Destroyed Objects";

    public const string DETAIL_ON_HOVER_KEY = "Detail On Hover";

    public static Color SCENE_OBJECT_COLOR = new(0.56f, 0.74f, 0.56f, 1.0f);
    public static Color PREFAB_COLOR = new(0.37f, 0.37f, 0.37f, 1.0f);
    public static Color DELETED_OR_DESTROYED_COLOR = new(0.83f, 0.31f, 0.31f, 1.0f);
    public static StyleColor ASSET_COLOR = new(Color.white);

    public const string ACTIVATED_MODIFIER_CLASS_NAME = "activated";
    public const string DELETED_MODIFIER_CLASS_NAME = "deleted";
    public const string UNLOADED_MODIFIER_CLASS_NAME = "unloaded";
  }

  public static class UnityBuiltInIcons {
    public const string PICK_OBJECT_ICON_NAME = "d_pick";
    public const string FAVORITE_WINDOW_ICON_NAME = "Favorite Icon";

#if UNITY_2022_3_OR_NEWER
    public const string FAVORITE_ICON_NAME = "Favorite_colored";
    public const string FAVORITE_EMPTY_ICON_NAME = "Favorite icon";
#else
        public const string favoriteIconName = "Toolbar Minus";
        public const string favoriteEmptyIconName = "Toolbar Plus";
#endif

    public const string REMOVE_ICON_NAME = "Toolbar Minus";
    public const string TAG_ICON_NAME = "AssetLabelIcon";
    public const string SEARCH_ICON_NAME = "Search Icon";
    public const string EYEVIEW_TOOL_ICONNAME = "d_ViewToolOrbit";
    public const string REFRESHICONNAME = "TreeEditor.Refresh";

    public const string OPEN_ASSET_ICON_NAME = "FolderOpened Icon";
    public const string CLEAR_SEARCH_TOOLBAR_ICON_NAME = "d_clear";
    public const string DEFAULT_ASSET_ICON_NAME = "DefaultAsset Icon";
    public const string D_VERTICAL_LAYOUT_GROUP_ICON_NAME = "d_VerticalLayoutGroup Icon";
    public const string TRANSFORM_ICON_NAME = "Transform Icon";
  }
}
