using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Odezzshuuk.Editor.SelectionTracker {

  [Serializable]
  public class NormalAssetEntry : Entry {

    // public Object Ref { get; set; }

    public override string DisplayName => RefState.HasFlag(RefState.Deleted)
      ? $"<s>{_cachedName}</s>"
      : _cachedName;

    public override RefState RefState => _cachedRef == null
          ? RefState.Deleted
          : RefState.Asset;

    public NormalAssetEntry(Object obj, GlobalObjectId id) : base(obj, id) {
      _cachedRefState = RefState.Asset;
    }

    public override void Ping() {
      EditorUtility.FocusProjectWindow();
      EditorGUIUtility.PingObject(Ref);
    }

    public override void Open() {
      AssetDatabase.OpenAsset(Ref);
    }
  }
}
